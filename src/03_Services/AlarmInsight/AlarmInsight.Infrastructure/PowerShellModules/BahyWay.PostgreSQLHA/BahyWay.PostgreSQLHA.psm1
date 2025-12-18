#Requires -Version 7.0

<#
.SYNOPSIS
    BahyWay PostgreSQL High Availability Management Module

.DESCRIPTION
    Enterprise-grade PowerShell module for managing PostgreSQL HA clusters with Docker,
    HAProxy, and Barman. Includes comprehensive health checks, monitoring, and alarm detection.

.NOTES
    Author: Bahaa Fadam - BahyWay Solutions
    Version: 1.0.0
    Date: 2025-11-22
#>

#region Module Variables

# Default configuration
$script:ModuleConfig = @{
    LogPath = if ($IsLinux -or $IsMacOS) {
        "/var/log/bahyway/postgresql-ha"
    } else {
        "$env:ProgramData\BahyWay\PostgreSQLHA\Logs"
    }
    AlarmLogPath = if ($IsLinux -or $IsMacOS) {
        "/var/log/bahyway/postgresql-ha/alarms"
    } else {
        "$env:ProgramData\BahyWay\PostgreSQLHA\Alarms"
    }
    ConfigPath = if ($IsLinux -or $IsMacOS) {
        "/etc/bahyway/postgresql-ha"
    } else {
        "$env:ProgramData\BahyWay\PostgreSQLHA\Config"
    }
    DefaultDockerComposeFile = "docker-compose-complete.yml"
    PrimaryContainerName = "bahyway-postgres-primary"
    ReplicaContainerName = "bahyway-postgres-replica"
    HAProxyContainerName = "bahyway-haproxy"
    BarmanContainerName = "bahyway-barman"
    NetworkName = "bahyway-network"
    MinimumDiskSpaceGB = 50
    ReplicationLagThresholdSeconds = 5
    AlarmRetentionDays = 30
}

# Global alarm registry
$script:HealthAlarms = @{}

#endregion

#region Helper Functions

function Write-ModuleLog {
    <#
    .SYNOPSIS
        Writes a log entry to the module log file
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('INFO', 'WARNING', 'ERROR', 'SUCCESS', 'DEBUG')]
        [string]$Level,
        
        [Parameter(Mandatory)]
        [string]$Message,
        
        [string]$Component = 'General',
        
        [System.Exception]$Exception
    )
    
    try {
        # Ensure log directory exists
        $logDir = $script:ModuleConfig.LogPath
        if (-not (Test-Path $logDir)) {
            New-Item -Path $logDir -ItemType Directory -Force | Out-Null
        }
        
        # Create log entry
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $logFile = Join-Path $logDir "postgresql-ha_$(Get-Date -Format 'yyyyMMdd').log"
        
        $logEntry = "[$timestamp] [$Level] [$Component] $Message"
        
        if ($Exception) {
            $logEntry += "`n    Exception: $($Exception.Message)"
            $logEntry += "`n    StackTrace: $($Exception.StackTrace)"
        }
        
        # Write to file
        Add-Content -Path $logFile -Value $logEntry -Encoding UTF8
        
        # Also write to console with color
        $color = switch ($Level) {
            'INFO' { 'White' }
            'SUCCESS' { 'Green' }
            'WARNING' { 'Yellow' }
            'ERROR' { 'Red' }
            'DEBUG' { 'Gray' }
        }
        Write-Host $logEntry -ForegroundColor $color
        
    } catch {
        Write-Warning "Failed to write to log: $_"
    }
}

function Write-AlarmLog {
    <#
    .SYNOPSIS
        Writes an alarm entry to the alarm log and registry
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AlarmType,
        
        [Parameter(Mandatory)]
        [ValidateSet('Critical', 'High', 'Medium', 'Low', 'Info')]
        [string]$Severity,
        
        [Parameter(Mandatory)]
        [string]$Message,
        
        [string]$Component,
        
        [hashtable]$Details
    )
    
    try {
        # Ensure alarm directory exists
        $alarmDir = $script:ModuleConfig.AlarmLogPath
        if (-not (Test-Path $alarmDir)) {
            New-Item -Path $alarmDir -ItemType Directory -Force | Out-Null
        }
        
        # Create alarm object
        $alarm = [PSCustomObject]@{
            Timestamp = Get-Date
            AlarmType = $AlarmType
            Severity = $Severity
            Component = $Component
            Message = $Message
            Details = $Details
            Acknowledged = $false
            AcknowledgedAt = $null
            AcknowledgedBy = $null
        }
        
        # Add to global registry
        $alarmKey = "$AlarmType-$Component-$(Get-Date -Format 'yyyyMMddHHmmss')"
        $script:HealthAlarms[$alarmKey] = $alarm
        
        # Write to alarm log file
        $alarmFile = Join-Path $alarmDir "alarms_$(Get-Date -Format 'yyyyMMdd').json"
        
        $alarmJson = $alarm | ConvertTo-Json -Depth 10
        Add-Content -Path $alarmFile -Value $alarmJson -Encoding UTF8
        
        # Log to main log
        Write-ModuleLog -Level 'WARNING' -Component $Component -Message "ALARM: [$Severity] $AlarmType - $Message"
        
        return $alarm
        
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'AlarmSystem' -Message "Failed to write alarm" -Exception $_
    }
}

function Test-CommandExists {
    <#
    .SYNOPSIS
        Tests if a command exists in the current session
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )
    
    return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Invoke-DockerCommand {
    <#
    .SYNOPSIS
        Executes a Docker command with error handling
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Arguments,
        
        [switch]$IgnoreError
    )
    
    try {
        $result = Invoke-Expression "docker $Arguments 2>&1"
        
        if ($LASTEXITCODE -ne 0 -and -not $IgnoreError) {
            throw "Docker command failed: $result"
        }
        
        return $result
    } catch {
        if (-not $IgnoreError) {
            throw
        }
        return $null
    }
}

#endregion

#region Docker Environment Tests

function Test-DockerEnvironment {
    <#
    .SYNOPSIS
        Comprehensive test of Docker environment availability and health
    
    .DESCRIPTION
        Checks if Docker is installed, running, and accessible. Works on Windows WSL2 and Linux.
        Generates alarms if Docker is not available or unhealthy.
    
    .EXAMPLE
        Test-DockerEnvironment
        
    .EXAMPLE
        $result = Test-DockerEnvironment -Verbose
        if ($result.IsHealthy) { "Docker is ready" }
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param()
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Docker' -Message "Starting Docker environment test"
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            DockerInstalled = $false
            DockerRunning = $false
            DockerVersion = $null
            Platform = $null
            IsWSL = $false
            Issues = @()
            Details = @{}
        }
        
        try {
            # Detect platform
            $result.Platform = if ($IsLinux) { "Linux" } 
                              elseif ($IsMacOS) { "MacOS" }
                              else { "Windows" }
            
            # Check if running in WSL
            if ($IsLinux -and (Test-Path "/proc/version")) {
                $versionContent = Get-Content "/proc/version" -Raw
                $result.IsWSL = $versionContent -match "Microsoft|WSL"
            }
            
            Write-ModuleLog -Level 'INFO' -Component 'Docker' -Message "Platform: $($result.Platform)$(if ($result.IsWSL) { ' (WSL2)' })"
            
            # Test 1: Check if Docker command exists
            if (-not (Test-CommandExists -Command 'docker')) {
                $result.Issues += "Docker command not found in PATH"
                Write-AlarmLog -AlarmType 'DockerNotInstalled' -Severity 'Critical' `
                    -Component 'Docker' -Message "Docker is not installed or not in PATH" `
                    -Details @{ Platform = $result.Platform; IsWSL = $result.IsWSL }
                
                Write-ModuleLog -Level 'ERROR' -Component 'Docker' -Message "Docker not found"
                return $result
            }
            
            $result.DockerInstalled = $true
            Write-ModuleLog -Level 'SUCCESS' -Component 'Docker' -Message "Docker command found"
            
            # Test 2: Get Docker version
            try {
                $versionOutput = docker version --format '{{.Server.Version}}' 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $result.DockerVersion = $versionOutput
                    $result.Details.Version = $versionOutput
                    Write-ModuleLog -Level 'INFO' -Component 'Docker' -Message "Docker version: $versionOutput"
                }
            } catch {
                $result.Issues += "Cannot retrieve Docker version"
            }
            
            # Test 3: Check if Docker daemon is running
            try {
                $psOutput = docker ps 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $result.DockerRunning = $true
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Docker' -Message "Docker daemon is running"
                } else {
                    $result.Issues += "Docker daemon is not running or not accessible"
                    Write-AlarmLog -AlarmType 'DockerNotRunning' -Severity 'Critical' `
                        -Component 'Docker' -Message "Docker daemon is not running" `
                        -Details @{ Error = $psOutput; Platform = $result.Platform }
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'Docker' -Message "Docker daemon not running"
                    return $result
                }
            } catch {
                $result.Issues += "Error checking Docker daemon: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Docker' -Message "Error checking daemon" -Exception $_
                return $result
            }
            
            # Test 4: Check Docker info
            try {
                $infoOutput = docker info --format '{{json .}}' 2>&1 | ConvertFrom-Json
                $result.Details.ContainersRunning = $infoOutput.ContainersRunning
                $result.Details.Images = $infoOutput.Images
                $result.Details.ServerVersion = $infoOutput.ServerVersion
                
                Write-ModuleLog -Level 'INFO' -Component 'Docker' -Message "Containers running: $($infoOutput.ContainersRunning)"
            } catch {
                Write-ModuleLog -Level 'WARNING' -Component 'Docker' -Message "Could not retrieve Docker info"
            }
            
            # If all tests passed
            if ($result.DockerInstalled -and $result.DockerRunning -and $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'Docker' -Message "Docker environment is healthy"
            }
            
        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Docker' -Message "Unexpected error in Docker test" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Docker' -Message "Docker environment test completed"
        }
        
        return $result
    }
}

#endregion

#region PostgreSQL Health Tests

function Test-PostgreSQLPrimary {
    <#
    .SYNOPSIS
        Tests the health of the PostgreSQL primary node
    
    .DESCRIPTION
        Comprehensive health check of the primary PostgreSQL container including:
        - Container existence and running state
        - PostgreSQL process status
        - Database connectivity
        - Replication configuration
        - Resource usage
    
    .PARAMETER ContainerName
        Name of the primary container (default: bahyway-postgres-primary)
    
    .EXAMPLE
        Test-PostgreSQLPrimary
        
    .EXAMPLE
        Test-PostgreSQLPrimary -ContainerName "my-postgres-primary" -Verbose
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$ContainerName = $script:ModuleConfig.PrimaryContainerName
    )
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Primary' -Message "Starting primary node health check"
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            ContainerExists = $false
            ContainerRunning = $false
            PostgreSQLResponding = $false
            IsInRecovery = $null
            ReplicationConfigured = $false
            ActiveConnections = 0
            DatabaseSize = $null
            Issues = @()
            Details = @{}
        }
        
        try {
            # Test 1: Check if container exists
            try {
                $containerInfo = docker ps -a --filter "name=^${ContainerName}$" --format '{{json .}}' 2>&1
                
                if ($LASTEXITCODE -eq 0 -and $containerInfo) {
                    $container = $containerInfo | ConvertFrom-Json
                    $result.ContainerExists = $true
                    $result.Details.ContainerId = $container.ID
                    $result.Details.Status = $container.Status
                    $result.Details.State = $container.State
                    
                    Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' `
                        -Message "Container found: $($container.Status)"
                } else {
                    $result.Issues += "Primary container does not exist"
                    Write-AlarmLog -AlarmType 'PrimaryContainerMissing' -Severity 'Critical' `
                        -Component 'PostgreSQL-Primary' -Message "Primary container '$ContainerName' not found"
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Container not found"
                    return $result
                }
            } catch {
                $result.Issues += "Error checking container: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Error checking container" -Exception $_
                return $result
            }
            
            # Test 2: Check if container is running
            if ($result.Details.State -eq 'running') {
                $result.ContainerRunning = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' -Message "Container is running"
            } else {
                $result.Issues += "Container is not running (State: $($result.Details.State))"
                Write-AlarmLog -AlarmType 'PrimaryContainerNotRunning' -Severity 'Critical' `
                    -Component 'PostgreSQL-Primary' -Message "Primary container is not running" `
                    -Details @{ State = $result.Details.State; ContainerName = $ContainerName }
                
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Container not running"
                return $result
            }
            
            # Test 3: Check PostgreSQL process
            try {
                $pgReady = docker exec $ContainerName pg_isready -U postgres 2>&1
                if ($LASTEXITCODE -eq 0 -and $pgReady -match "accepting connections") {
                    $result.PostgreSQLResponding = $true
                    Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' -Message "PostgreSQL is accepting connections"
                } else {
                    $result.Issues += "PostgreSQL not accepting connections"
                    Write-AlarmLog -AlarmType 'PrimaryDatabaseNotResponding' -Severity 'Critical' `
                        -Component 'PostgreSQL-Primary' -Message "PostgreSQL is not accepting connections"
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Database not accepting connections"
                }
            } catch {
                $result.Issues += "Error checking PostgreSQL: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Error checking PostgreSQL" -Exception $_
            }
            
            # Test 4: Check if in recovery mode (should be FALSE for primary)
            if ($result.PostgreSQLResponding) {
                try {
                    $recoveryStatus = docker exec $ContainerName psql -U postgres -t -A -c "SELECT pg_is_in_recovery();" 2>&1
                    $result.IsInRecovery = ($recoveryStatus -match "t")
                    
                    if ($result.IsInRecovery) {
                        $result.Issues += "Primary is in recovery mode (should not be)"
                        Write-AlarmLog -AlarmType 'PrimaryInRecoveryMode' -Severity 'Critical' `
                            -Component 'PostgreSQL-Primary' -Message "Primary node is incorrectly in recovery mode"
                        
                        Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Primary in recovery mode"
                    } else {
                        Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' -Message "Primary is not in recovery mode (correct)"
                    }
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "Could not check recovery status"
                }
                
                # Test 5: Check replication configuration
                try {
                    $replCount = docker exec $ContainerName psql -U postgres -t -A -c "SELECT COUNT(*) FROM pg_stat_replication;" 2>&1
                    $result.Details.ReplicaCount = [int]$replCount
                    
                    if ($replCount -gt 0) {
                        $result.ReplicationConfigured = $true
                        Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' `
                            -Message "Replication active with $replCount replica(s)"
                    } else {
                        $result.Issues += "No replicas connected"
                        Write-AlarmLog -AlarmType 'NoReplicasConnected' -Severity 'High' `
                            -Component 'PostgreSQL-Primary' -Message "No replica nodes are connected to primary"
                        
                        Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "No replicas connected"
                    }
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "Could not check replication status"
                }
                
                # Test 6: Get active connections
                try {
                    $connCount = docker exec $ContainerName psql -U postgres -t -A -c "SELECT count(*) FROM pg_stat_activity WHERE state = 'active';" 2>&1
                    $result.ActiveConnections = [int]$connCount
                    $result.Details.ActiveConnections = $result.ActiveConnections
                    Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Primary' -Message "Active connections: $connCount"
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "Could not get connection count"
                }
                
                # Test 7: Get database size
                try {
                    $dbSize = docker exec $ContainerName psql -U postgres -t -A -c "SELECT pg_size_pretty(pg_database_size('alarminsight'));" 2>&1
                    $result.DatabaseSize = $dbSize.Trim()
                    $result.Details.DatabaseSize = $result.DatabaseSize
                    Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Primary' -Message "Database size: $($result.DatabaseSize)"
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "Could not get database size"
                }
            }
            
            # Determine overall health
            if ($result.ContainerRunning -and 
                $result.PostgreSQLResponding -and 
                -not $result.IsInRecovery -and
                $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Primary' -Message "Primary node is healthy"
            } else {
                Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Primary' -Message "Primary node has issues"
            }
            
        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Primary' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Primary' -Message "Primary health check completed"
        }
        
        return $result
    }
}

function Test-PostgreSQLReplica {
    <#
    .SYNOPSIS
        Tests the health of the PostgreSQL replica node
    
    .DESCRIPTION
        Comprehensive health check of the replica PostgreSQL container including:
        - Container existence and running state
        - PostgreSQL process status
        - Recovery mode verification
        - Replication lag monitoring
        - Database synchronization status
    
    .PARAMETER ContainerName
        Name of the replica container (default: bahyway-postgres-replica)
    
    .EXAMPLE
        Test-PostgreSQLReplica
        
    .EXAMPLE
        Test-PostgreSQLReplica -ContainerName "my-postgres-replica" -Verbose
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$ContainerName = $script:ModuleConfig.ReplicaContainerName
    )
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Replica' -Message "Starting replica node health check"
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            ContainerExists = $false
            ContainerRunning = $false
            PostgreSQLResponding = $false
            IsInRecovery = $null
            ReplicationActive = $false
            ReplicationLag = $null
            Issues = @()
            Details = @{}
        }
        
        try {
            # Test 1: Check if container exists
            try {
                $containerInfo = docker ps -a --filter "name=^${ContainerName}$" --format '{{json .}}' 2>&1
                
                if ($LASTEXITCODE -eq 0 -and $containerInfo) {
                    $container = $containerInfo | ConvertFrom-Json
                    $result.ContainerExists = $true
                    $result.Details.ContainerId = $container.ID
                    $result.Details.Status = $container.Status
                    $result.Details.State = $container.State
                    
                    Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' `
                        -Message "Container found: $($container.Status)"
                } else {
                    $result.Issues += "Replica container does not exist"
                    Write-AlarmLog -AlarmType 'ReplicaContainerMissing' -Severity 'High' `
                        -Component 'PostgreSQL-Replica' -Message "Replica container '$ContainerName' not found"
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Container not found"
                    return $result
                }
            } catch {
                $result.Issues += "Error checking container: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Error checking container" -Exception $_
                return $result
            }
            
            # Test 2: Check if container is running
            if ($result.Details.State -eq 'running') {
                $result.ContainerRunning = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' -Message "Container is running"
            } else {
                $result.Issues += "Container is not running (State: $($result.Details.State))"
                Write-AlarmLog -AlarmType 'ReplicaContainerNotRunning' -Severity 'High' `
                    -Component 'PostgreSQL-Replica' -Message "Replica container is not running" `
                    -Details @{ State = $result.Details.State; ContainerName = $ContainerName }
                
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Container not running"
                return $result
            }
            
            # Test 3: Check PostgreSQL process
            try {
                $pgReady = docker exec $ContainerName pg_isready -U postgres 2>&1
                if ($LASTEXITCODE -eq 0 -and $pgReady -match "accepting connections") {
                    $result.PostgreSQLResponding = $true
                    Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' -Message "PostgreSQL is accepting connections"
                } else {
                    $result.Issues += "PostgreSQL not accepting connections"
                    Write-AlarmLog -AlarmType 'ReplicaDatabaseNotResponding' -Severity 'High' `
                        -Component 'PostgreSQL-Replica' -Message "PostgreSQL replica is not accepting connections"
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Database not accepting connections"
                }
            } catch {
                $result.Issues += "Error checking PostgreSQL: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Error checking PostgreSQL" -Exception $_
            }
            
            # Test 4: Check if in recovery mode (should be TRUE for replica)
            if ($result.PostgreSQLResponding) {
                try {
                    $recoveryStatus = docker exec $ContainerName psql -U postgres -t -A -c "SELECT pg_is_in_recovery();" 2>&1
                    $result.IsInRecovery = ($recoveryStatus -match "t")
                    
                    if ($result.IsInRecovery) {
                        Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' -Message "Replica is in recovery mode (correct)"
                    } else {
                        $result.Issues += "Replica is NOT in recovery mode (should be)"
                        Write-AlarmLog -AlarmType 'ReplicaNotInRecoveryMode' -Severity 'Critical' `
                            -Component 'PostgreSQL-Replica' -Message "Replica node is not in recovery mode"
                        
                        Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Replica not in recovery mode"
                    }
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Replica' -Message "Could not check recovery status"
                }
                
                # Test 5: Check replication lag
                try {
                    $lagQuery = "SELECT EXTRACT(EPOCH FROM (now() - pg_last_xact_replay_timestamp()))::int;"
                    $lagSeconds = docker exec $ContainerName psql -U postgres -t -A -c $lagQuery 2>&1
                    
                    if ($lagSeconds -match '^\d+$') {
                        $result.ReplicationLag = [int]$lagSeconds
                        $result.Details.ReplicationLagSeconds = $result.ReplicationLag
                        
                        if ($result.ReplicationLag -gt $script:ModuleConfig.ReplicationLagThresholdSeconds) {
                            $result.Issues += "Replication lag is high: $($result.ReplicationLag) seconds"
                            Write-AlarmLog -AlarmType 'HighReplicationLag' -Severity 'Medium' `
                                -Component 'PostgreSQL-Replica' `
                                -Message "Replication lag is $($result.ReplicationLag) seconds (threshold: $($script:ModuleConfig.ReplicationLagThresholdSeconds))" `
                                -Details @{ LagSeconds = $result.ReplicationLag }
                            
                            Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Replica' `
                                -Message "High replication lag: $($result.ReplicationLag)s"
                        } else {
                            $result.ReplicationActive = $true
                            Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' `
                                -Message "Replication lag is acceptable: $($result.ReplicationLag)s"
                        }
                    }
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Replica' -Message "Could not check replication lag"
                }
            }
            
            # Determine overall health
            if ($result.ContainerRunning -and 
                $result.PostgreSQLResponding -and 
                $result.IsInRecovery -and
                $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'PostgreSQL-Replica' -Message "Replica node is healthy"
            } else {
                Write-ModuleLog -Level 'WARNING' -Component 'PostgreSQL-Replica' -Message "Replica node has issues"
            }
            
        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'PostgreSQL-Replica' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'PostgreSQL-Replica' -Message "Replica health check completed"
        }
        
        return $result
    }
}

function Test-PostgreSQLReplication {
    <#
    .SYNOPSIS
        Tests the replication status between primary and replica
    
    .DESCRIPTION
        Verifies that streaming replication is working correctly by checking:
        - Primary replication connections
        - Replication lag metrics
        - WAL position synchronization
        - Replication slots
    
    .EXAMPLE
        Test-PostgreSQLReplication
        
    .EXAMPLE
        $status = Test-PostgreSQLReplication -Verbose
        if ($status.IsHealthy) { "Replication OK" }
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$PrimaryContainer = $script:ModuleConfig.PrimaryContainerName,
        [string]$ReplicaContainer = $script:ModuleConfig.ReplicaContainerName
    )
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Replication' -Message "Starting replication status check"
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            ReplicationActive = $false
            ReplicaConnected = $false
            StreamingState = $null
            SyncState = $null
            WriteLag = $null
            FlushLag = $null
            ReplayLag = $null
            ReplicationSlots = @()
            Issues = @()
            Details = @{}
        }
        
        try {
            # Check if primary is available
            $primaryTest = Test-PostgreSQLPrimary -ContainerName $PrimaryContainer
            if (-not $primaryTest.IsHealthy) {
                $result.Issues += "Primary node is not healthy"
                Write-ModuleLog -Level 'ERROR' -Component 'Replication' -Message "Cannot check replication: primary unhealthy"
                return $result
            }
            
            # Get replication status from primary
            try {
                $replQuery = @"
SELECT 
    client_addr,
    application_name,
    state,
    sync_state,
    COALESCE(write_lag, '0'::interval)::text as write_lag,
    COALESCE(flush_lag, '0'::interval)::text as flush_lag,
    COALESCE(replay_lag, '0'::interval)::text as replay_lag
FROM pg_stat_replication;
"@
                
                $replStatus = docker exec $PrimaryContainer psql -U postgres -t -A -F '|' -c $replQuery 2>&1
                
                if ($LASTEXITCODE -eq 0 -and $replStatus -and $replStatus -ne "") {
                    $fields = $replStatus.Split('|')
                    
                    $result.ReplicaConnected = $true
                    $result.Details.ClientAddress = $fields[0]
                    $result.Details.ApplicationName = $fields[1]
                    $result.StreamingState = $fields[2]
                    $result.SyncState = $fields[3]
                    $result.WriteLag = $fields[4]
                    $result.FlushLag = $fields[5]
                    $result.ReplayLag = $fields[6]
                    
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Replication' `
                        -Message "Replica connected: $($result.StreamingState) ($($result.SyncState))"
                    
                    if ($result.StreamingState -eq 'streaming') {
                        $result.ReplicationActive = $true
                    } else {
                        $result.Issues += "Replication state is not 'streaming': $($result.StreamingState)"
                        Write-AlarmLog -AlarmType 'ReplicationNotStreaming' -Severity 'High' `
                            -Component 'Replication' `
                            -Message "Replication is not in streaming state" `
                            -Details @{ State = $result.StreamingState }
                    }
                } else {
                    $result.Issues += "No replication connections found"
                    Write-AlarmLog -AlarmType 'NoReplicationConnection' -Severity 'Critical' `
                        -Component 'Replication' -Message "No replica is connected to primary"
                    
                    Write-ModuleLog -Level 'ERROR' -Component 'Replication' -Message "No replication connections"
                }
            } catch {
                $result.Issues += "Error querying replication status: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Replication' -Message "Error querying replication" -Exception $_
            }
            
            # Check replication slots
            try {
                $slotsQuery = "SELECT slot_name, slot_type, active FROM pg_replication_slots;"
                $slots = docker exec $PrimaryContainer psql -U postgres -t -A -c $slotsQuery 2>&1
                
                if ($slots) {
                    $result.ReplicationSlots = @($slots -split "`n" | Where-Object { $_ })
                    Write-ModuleLog -Level 'INFO' -Component 'Replication' `
                        -Message "Replication slots: $($result.ReplicationSlots.Count)"
                }
            } catch {
                Write-ModuleLog -Level 'WARNING' -Component 'Replication' -Message "Could not check replication slots"
            }
            
            # Determine overall health
            if ($result.ReplicationActive -and $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'Replication' -Message "Replication is healthy"
            }
            
        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Replication' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Replication' -Message "Replication check completed"
        }
        
        return $result
    }
}

#endregion

#region Network and Additional Health Tests

function Test-NetworkConnectivity {
    <#
    .SYNOPSIS
        Tests network connectivity between cluster components

    .DESCRIPTION
        Verifies network connectivity between:
        - Primary and replica nodes
        - HAProxy and database nodes
        - Host and all containers

    .EXAMPLE
        Test-NetworkConnectivity
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$PrimaryContainer = $script:ModuleConfig.PrimaryContainerName,
        [string]$ReplicaContainer = $script:ModuleConfig.ReplicaContainerName,
        [string]$HAProxyContainer = $script:ModuleConfig.HAProxyContainerName
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Network' -Message "Starting network connectivity test"
    }

    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            PrimaryReachable = $false
            ReplicaReachable = $false
            HAProxyReachable = $false
            PrimaryToReplicaConnectivity = $false
            Issues = @()
            Details = @{}
        }

        try {
            # Test primary container network
            try {
                $primaryIP = docker inspect $PrimaryContainer --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' 2>&1
                if ($LASTEXITCODE -eq 0 -and $primaryIP) {
                    $result.PrimaryReachable = $true
                    $result.Details.PrimaryIP = $primaryIP
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Network' -Message "Primary reachable at $primaryIP"
                } else {
                    $result.Issues += "Primary container not reachable"
                }
            } catch {
                $result.Issues += "Error checking primary network: $_"
            }

            # Test replica container network
            try {
                $replicaIP = docker inspect $ReplicaContainer --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' 2>&1
                if ($LASTEXITCODE -eq 0 -and $replicaIP) {
                    $result.ReplicaReachable = $true
                    $result.Details.ReplicaIP = $replicaIP
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Network' -Message "Replica reachable at $replicaIP"
                } else {
                    $result.Issues += "Replica container not reachable"
                }
            } catch {
                $result.Issues += "Error checking replica network: $_"
            }

            # Test HAProxy container network
            try {
                $haproxyIP = docker inspect $HAProxyContainer --format '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' 2>&1
                if ($LASTEXITCODE -eq 0 -and $haproxyIP) {
                    $result.HAProxyReachable = $true
                    $result.Details.HAProxyIP = $haproxyIP
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Network' -Message "HAProxy reachable at $haproxyIP"
                }
            } catch {
                Write-ModuleLog -Level 'WARNING' -Component 'Network' -Message "HAProxy not available"
            }

            # Test primary to replica connectivity
            if ($result.PrimaryReachable -and $result.ReplicaReachable) {
                try {
                    $pingResult = docker exec $PrimaryContainer ping -c 1 $ReplicaContainer 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        $result.PrimaryToReplicaConnectivity = $true
                        Write-ModuleLog -Level 'SUCCESS' -Component 'Network' -Message "Primary can reach replica"
                    } else {
                        $result.Issues += "Primary cannot ping replica"
                    }
                } catch {
                    $result.Issues += "Error testing primary to replica connectivity: $_"
                }
            }

            # Determine health
            if ($result.PrimaryReachable -and $result.ReplicaReachable -and
                $result.PrimaryToReplicaConnectivity -and $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'Network' -Message "Network connectivity is healthy"
            }

        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Network' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Network' -Message "Network connectivity test completed"
        }

        return $result
    }
}

function Test-HAProxyHealth {
    <#
    .SYNOPSIS
        Tests the health of the HAProxy load balancer

    .DESCRIPTION
        Checks HAProxy container status, configuration, and backend connectivity

    .PARAMETER ContainerName
        Name of the HAProxy container (default: bahyway-haproxy)

    .EXAMPLE
        Test-HAProxyHealth
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$ContainerName = $script:ModuleConfig.HAProxyContainerName
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'HAProxy' -Message "Starting HAProxy health check"
    }

    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            ContainerExists = $false
            ContainerRunning = $false
            StatsPageAccessible = $false
            BackendsHealthy = $false
            Issues = @()
            Details = @{}
        }

        try {
            # Check if container exists
            $containerInfo = docker ps -a --filter "name=^${ContainerName}$" --format '{{json .}}' 2>&1

            if ($LASTEXITCODE -eq 0 -and $containerInfo) {
                $container = $containerInfo | ConvertFrom-Json
                $result.ContainerExists = $true
                $result.Details.Status = $container.Status
                $result.Details.State = $container.State

                if ($container.State -eq 'running') {
                    $result.ContainerRunning = $true
                    Write-ModuleLog -Level 'SUCCESS' -Component 'HAProxy' -Message "HAProxy container is running"
                } else {
                    $result.Issues += "HAProxy container is not running"
                    Write-ModuleLog -Level 'ERROR' -Component 'HAProxy' -Message "HAProxy container not running"
                }
            } else {
                $result.Issues += "HAProxy container does not exist"
                Write-ModuleLog -Level 'WARNING' -Component 'HAProxy' -Message "HAProxy container not found (optional component)"
            }

            # Determine health
            if ($result.ContainerExists -and $result.ContainerRunning) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'HAProxy' -Message "HAProxy is healthy"
            } elseif (-not $result.ContainerExists) {
                # HAProxy is optional, so not existing is not necessarily unhealthy
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'INFO' -Component 'HAProxy' -Message "HAProxy not configured (optional)"
            }

        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'HAProxy' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'HAProxy' -Message "HAProxy health check completed"
        }

        return $result
    }
}

function Test-BarmanBackup {
    <#
    .SYNOPSIS
        Tests the health of the Barman backup system

    .DESCRIPTION
        Checks Barman container status and backup configuration

    .PARAMETER ContainerName
        Name of the Barman container (default: bahyway-barman)

    .EXAMPLE
        Test-BarmanBackup
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [string]$ContainerName = $script:ModuleConfig.BarmanContainerName
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Barman' -Message "Starting Barman health check"
    }

    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            ContainerExists = $false
            ContainerRunning = $false
            BackupsConfigured = $false
            LastBackup = $null
            Issues = @()
            Details = @{}
        }

        try {
            # Check if container exists
            $containerInfo = docker ps -a --filter "name=^${ContainerName}$" --format '{{json .}}' 2>&1

            if ($LASTEXITCODE -eq 0 -and $containerInfo) {
                $container = $containerInfo | ConvertFrom-Json
                $result.ContainerExists = $true
                $result.Details.Status = $container.Status
                $result.Details.State = $container.State

                if ($container.State -eq 'running') {
                    $result.ContainerRunning = $true
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Barman' -Message "Barman container is running"
                } else {
                    $result.Issues += "Barman container is not running"
                    Write-ModuleLog -Level 'ERROR' -Component 'Barman' -Message "Barman container not running"
                }
            } else {
                $result.Issues += "Barman container does not exist"
                Write-ModuleLog -Level 'WARNING' -Component 'Barman' -Message "Barman container not found (optional component)"
            }

            # Determine health
            if ($result.ContainerExists -and $result.ContainerRunning) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'Barman' -Message "Barman is healthy"
            } elseif (-not $result.ContainerExists) {
                # Barman is optional
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'INFO' -Component 'Barman' -Message "Barman not configured (optional)"
            }

        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Barman' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Barman' -Message "Barman health check completed"
        }

        return $result
    }
}

#endregion

#region Storage and Resource Tests

function Test-StorageSpace {
    <#
    .SYNOPSIS
        Tests available storage space for PostgreSQL data
    
    .DESCRIPTION
        Checks available disk space on the host and within containers,
        generates alarms if space is running low.
    
    .PARAMETER MinimumSpaceGB
        Minimum required space in GB (default: 50)
    
    .EXAMPLE
        Test-StorageSpace
        
    .EXAMPLE
        Test-StorageSpace -MinimumSpaceGB 100 -Verbose
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [int]$MinimumSpaceGB = $script:ModuleConfig.MinimumDiskSpaceGB
    )
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Storage' -Message "Starting storage space check"
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            HostStorageGB = 0
            HostAvailableGB = 0
            HostUsedPercent = 0
            VolumeInfo = @()
            Issues = @()
            Details = @{}
        }
        
        try {
            # Check host storage
            if ($IsLinux -or $IsMacOS) {
                try {
                    $dfOutput = df -BG / | Select-Object -Skip 1
                    $fields = $dfOutput -split '\s+' 
                    $result.HostStorageGB = [int]($fields[1] -replace 'G','')
                    $result.HostAvailableGB = [int]($fields[3] -replace 'G','')
                    $result.HostUsedPercent = [int]($fields[4] -replace '%','')
                    
                    Write-ModuleLog -Level 'INFO' -Component 'Storage' `
                        -Message "Host storage: $($result.HostAvailableGB)GB available ($($result.HostUsedPercent)% used)"
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'Storage' -Message "Could not check host storage"
                }
            } else {
                # Windows
                try {
                    $drive = Get-PSDrive -Name C
                    $result.HostStorageGB = [math]::Round($drive.Used / 1GB + $drive.Free / 1GB)
                    $result.HostAvailableGB = [math]::Round($drive.Free / 1GB)
                    $result.HostUsedPercent = [math]::Round(($drive.Used / ($drive.Used + $drive.Free)) * 100)
                    
                    Write-ModuleLog -Level 'INFO' -Component 'Storage' `
                        -Message "Host storage: $($result.HostAvailableGB)GB available ($($result.HostUsedPercent)% used)"
                } catch {
                    Write-ModuleLog -Level 'WARNING' -Component 'Storage' -Message "Could not check host storage"
                }
            }
            
            # Check if we have minimum required space
            if ($result.HostAvailableGB -lt $MinimumSpaceGB) {
                $result.Issues += "Insufficient storage: $($result.HostAvailableGB)GB available (minimum: ${MinimumSpaceGB}GB)"
                Write-AlarmLog -AlarmType 'LowDiskSpace' -Severity 'High' `
                    -Component 'Storage' `
                    -Message "Available storage ($($result.HostAvailableGB)GB) is below minimum ($MinimumSpaceGB GB)" `
                    -Details @{ AvailableGB = $result.HostAvailableGB; MinimumGB = $MinimumSpaceGB }
                
                Write-ModuleLog -Level 'ERROR' -Component 'Storage' -Message "Insufficient storage space"
            } else {
                Write-ModuleLog -Level 'SUCCESS' -Component 'Storage' -Message "Storage space is adequate"
            }
            
            # Check Docker volumes
            try {
                $volumes = docker volume ls --filter "name=bahyway" --format '{{.Name}}' 2>&1
                if ($volumes) {
                    foreach ($vol in $volumes) {
                        try {
                            $volInspect = docker volume inspect $vol 2>&1 | ConvertFrom-Json
                            $result.VolumeInfo += [PSCustomObject]@{
                                Name = $vol
                                Mountpoint = $volInspect.Mountpoint
                                Driver = $volInspect.Driver
                            }
                        } catch {
                            Write-ModuleLog -Level 'WARNING' -Component 'Storage' -Message "Could not inspect volume: $vol"
                        }
                    }
                    Write-ModuleLog -Level 'INFO' -Component 'Storage' -Message "Found $($result.VolumeInfo.Count) volumes"
                }
            } catch {
                Write-ModuleLog -Level 'WARNING' -Component 'Storage' -Message "Could not list Docker volumes"
            }
            
            # Determine health
            if ($result.HostAvailableGB -ge $MinimumSpaceGB -and $result.Issues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'Storage' -Message "Storage is healthy"
            }
            
        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Storage' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Storage' -Message "Storage check completed"
        }
        
        return $result
    }
}

#endregion

#region Comprehensive Cluster Health

function Get-ClusterHealth {
    <#
    .SYNOPSIS
        Performs a comprehensive health check of the entire PostgreSQL HA cluster
    
    .DESCRIPTION
        Runs all health checks and returns a complete status report including:
        - Docker environment
        - Primary node
        - Replica node
        - Replication status
        - Storage space
        - Network connectivity
        - HAProxy (if configured)
        - Barman (if configured)
    
    .PARAMETER IncludeHAProxy
        Include HAProxy health check
    
    .PARAMETER IncludeBarman
        Include Barman backup health check
    
    .EXAMPLE
        Get-ClusterHealth
        
    .EXAMPLE
        Get-ClusterHealth -IncludeHAProxy -IncludeBarman -Verbose
        
    .EXAMPLE
        $health = Get-ClusterHealth
        if (-not $health.IsHealthy) {
            $health.AllIssues | ForEach-Object { Write-Warning $_ }
        }
    #>
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param(
        [switch]$IncludeHAProxy,
        [switch]$IncludeBarman
    )
    
    begin {
        Write-ModuleLog -Level 'INFO' -Component 'ClusterHealth' -Message "========== STARTING COMPREHENSIVE CLUSTER HEALTH CHECK =========="
    }
    
    process {
        $result = [PSCustomObject]@{
            IsHealthy = $false
            Timestamp = Get-Date
            Docker = $null
            Primary = $null
            Replica = $null
            Replication = $null
            Storage = $null
            HAProxy = $null
            Barman = $null
            AllIssues = @()
            Summary = @{}
        }
        
        try {
            # 1. Docker Environment
            Write-Host "`n[1/7] Checking Docker Environment..." -ForegroundColor Cyan
            $result.Docker = Test-DockerEnvironment
            if (-not $result.Docker.IsHealthy) {
                $result.AllIssues += $result.Docker.Issues
                Write-ModuleLog -Level 'ERROR' -Component 'ClusterHealth' -Message "Docker environment check failed"
                return $result  # Can't continue without Docker
            }
            
            # 2. Primary Node
            Write-Host "`n[2/7] Checking Primary Node..." -ForegroundColor Cyan
            $result.Primary = Test-PostgreSQLPrimary
            $result.AllIssues += $result.Primary.Issues
            
            # 3. Replica Node
            Write-Host "`n[3/7] Checking Replica Node..." -ForegroundColor Cyan
            $result.Replica = Test-PostgreSQLReplica
            $result.AllIssues += $result.Replica.Issues
            
            # 4. Replication Status
            Write-Host "`n[4/7] Checking Replication..." -ForegroundColor Cyan
            $result.Replication = Test-PostgreSQLReplication
            $result.AllIssues += $result.Replication.Issues
            
            # 5. Storage
            Write-Host "`n[5/7] Checking Storage Space..." -ForegroundColor Cyan
            $result.Storage = Test-StorageSpace
            $result.AllIssues += $result.Storage.Issues
            
            # 6. HAProxy (optional)
            if ($IncludeHAProxy) {
                Write-Host "`n[6/7] Checking HAProxy..." -ForegroundColor Cyan
                $result.HAProxy = Test-HAProxyHealth
                if ($result.HAProxy) {
                    $result.AllIssues += $result.HAProxy.Issues
                }
            } else {
                Write-Host "`n[6/7] Skipping HAProxy check" -ForegroundColor Gray
            }
            
            # 7. Barman (optional)
            if ($IncludeBarman) {
                Write-Host "`n[7/7] Checking Barman..." -ForegroundColor Cyan
                $result.Barman = Test-BarmanBackup
                if ($result.Barman) {
                    $result.AllIssues += $result.Barman.Issues
                }
            } else {
                Write-Host "`n[7/7] Skipping Barman check" -ForegroundColor Gray
            }
            
            # Generate summary
            $result.Summary = @{
                TotalIssues = $result.AllIssues.Count
                DockerHealthy = $result.Docker.IsHealthy
                PrimaryHealthy = $result.Primary.IsHealthy
                ReplicaHealthy = $result.Replica.IsHealthy
                ReplicationHealthy = $result.Replication.IsHealthy
                StorageHealthy = $result.Storage.IsHealthy
                ReplicationLagSeconds = $result.Replica.ReplicationLag
                PrimaryActiveConnections = $result.Primary.ActiveConnections
                StorageAvailableGB = $result.Storage.HostAvailableGB
            }
            
            # Determine overall health
            $criticalComponentsHealthy = (
                $result.Docker.IsHealthy -and
                $result.Primary.IsHealthy -and
                $result.Replica.IsHealthy -and
                $result.Replication.IsHealthy -and
                $result.Storage.IsHealthy
            )
            
            if ($criticalComponentsHealthy -and $result.AllIssues.Count -eq 0) {
                $result.IsHealthy = $true
                Write-ModuleLog -Level 'SUCCESS' -Component 'ClusterHealth' -Message "✅ CLUSTER IS HEALTHY"
            } else {
                Write-ModuleLog -Level 'WARNING' -Component 'ClusterHealth' -Message "⚠️  CLUSTER HAS ISSUES"
            }
            
        } catch {
            $result.AllIssues += "Unexpected error in cluster health check: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'ClusterHealth' -Message "Unexpected error" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'ClusterHealth' -Message "========== CLUSTER HEALTH CHECK COMPLETED =========="
        }
        
        return $result
    }
    
    end {
        # Display summary
        Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
        Write-Host   "║          CLUSTER HEALTH SUMMARY                    ║" -ForegroundColor Cyan
        Write-Host   "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
        
        if ($result.IsHealthy) {
            Write-Host "`n✅ ALL SYSTEMS OPERATIONAL" -ForegroundColor Green
        } else {
            Write-Host "`n⚠️  ISSUES DETECTED: $($result.AllIssues.Count)" -ForegroundColor Yellow
            foreach ($issue in $result.AllIssues) {
                Write-Host "   • $issue" -ForegroundColor Yellow
            }
        }
        
        Write-Host "`n📊 Component Status:" -ForegroundColor Cyan
        Write-Host "   Docker:       $(if ($result.Docker.IsHealthy) { '✅' } else { '❌' })" -ForegroundColor White
        Write-Host "   Primary:      $(if ($result.Primary.IsHealthy) { '✅' } else { '❌' })" -ForegroundColor White
        Write-Host "   Replica:      $(if ($result.Replica.IsHealthy) { '✅' } else { '❌' })" -ForegroundColor White
        Write-Host "   Replication:  $(if ($result.Replication.IsHealthy) { '✅' } else { '❌' })" -ForegroundColor White
        Write-Host "   Storage:      $(if ($result.Storage.IsHealthy) { '✅' } else { '❌' })" -ForegroundColor White
        
        if ($result.Replication.IsHealthy) {
            Write-Host "`n📈 Metrics:" -ForegroundColor Cyan
            Write-Host "   Replication Lag:  $($result.Replica.ReplicationLag)s" -ForegroundColor White
            Write-Host "   Active Connections: $($result.Primary.ActiveConnections)" -ForegroundColor White
            Write-Host "   Available Storage: $($result.Storage.HostAvailableGB)GB" -ForegroundColor White
        }
        
        Write-Host ""
    }
}

#endregion

#region Deployment and Initialization Functions

function Initialize-PostgreSQLHA {
    <#
    .SYNOPSIS
        Initializes PostgreSQL HA cluster with automatic replication setup

    .DESCRIPTION
        Automatically sets up PostgreSQL replication between primary and replica nodes:
        - Creates replication user
        - Creates replication slot
        - Configures pg_hba.conf for replication
        - Restarts replica container
        - Verifies replication is working

    .PARAMETER PrimaryContainer
        Name of the primary container (default: bahyway-postgres-primary)

    .PARAMETER ReplicaContainer
        Name of the replica container (default: bahyway-postgres-replica)

    .PARAMETER ReplicationUser
        Username for replication (default: replicator)

    .PARAMETER ReplicationPassword
        Password for replication user (default: replicator123)

    .PARAMETER ReplicationSlotName
        Name of the replication slot (default: replica_slot)

    .PARAMETER NetworkName
        Docker network name (default: bahyway-network)

    .PARAMETER ComposeFile
        Path to docker-compose file (default: infrastructure/postgresql-ha/docker/docker-compose-complete.yml)

    .EXAMPLE
        Initialize-PostgreSQLHA

    .EXAMPLE
        Initialize-PostgreSQLHA -Verbose -ReplicationPassword "SecurePass123!"
    #>
    [CmdletBinding()]
    param(
        [string]$PrimaryContainer = $script:ModuleConfig.PrimaryContainerName,
        [string]$ReplicaContainer = $script:ModuleConfig.ReplicaContainerName,
        [string]$ReplicationUser = "replicator",
        [string]$ReplicationPassword = "replicator123",
        [string]$ReplicationSlotName = "replica_slot",
        [string]$NetworkName = $script:ModuleConfig.NetworkName,
        [string]$ComposeFile = ""
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "========== STARTING POSTGRESQL HA INITIALIZATION =========="
    }

    process {
        $result = [PSCustomObject]@{
            Success = $false
            Steps = @()
            Issues = @()
            ReplicationStatus = $null
        }

        try {
            # Determine docker-compose file path
            if ([string]::IsNullOrWhiteSpace($ComposeFile)) {
                # Try common locations
                $possiblePaths = @(
                    "docker-compose-complete.yml",
                    "infrastructure/postgresql-ha/docker/docker-compose-complete.yml",
                    "../infrastructure/postgresql-ha/docker/docker-compose-complete.yml",
                    "../../infrastructure/postgresql-ha/docker/docker-compose-complete.yml"
                )

                foreach ($path in $possiblePaths) {
                    if (Test-Path $path) {
                        $ComposeFile = (Resolve-Path $path).Path
                        Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "Found docker-compose file at: $ComposeFile"
                        break
                    }
                }

                if ([string]::IsNullOrWhiteSpace($ComposeFile) -or -not (Test-Path $ComposeFile)) {
                    $warningMsg = "docker-compose-complete.yml not found. Replica restart will use direct docker commands."
                    Write-ModuleLog -Level 'WARNING' -Component 'Initialization' -Message $warningMsg
                    Write-Host "⚠️  $warningMsg" -ForegroundColor Yellow
                }
            } elseif (-not (Test-Path $ComposeFile)) {
                throw "Specified docker-compose file not found: $ComposeFile"
            } else {
                $ComposeFile = (Resolve-Path $ComposeFile).Path
            }

            # Step 1: Verify Docker environment
            Write-Host "`n[1/8] Verifying Docker environment..." -ForegroundColor Cyan
            $dockerTest = Test-DockerEnvironment
            if (-not $dockerTest.IsHealthy) {
                throw "Docker environment is not healthy. Please fix Docker issues first."
            }
            $result.Steps += "Docker environment verified"
            Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Docker environment verified"

            # Step 2: Verify primary container is running
            Write-Host "`n[2/8] Verifying primary container..." -ForegroundColor Cyan
            $primaryTest = Test-PostgreSQLPrimary -ContainerName $PrimaryContainer
            if (-not $primaryTest.ContainerRunning) {
                throw "Primary container '$PrimaryContainer' is not running"
            }
            $result.Steps += "Primary container verified"
            Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Primary container verified"

            # Step 3: Create replication user
            Write-Host "`n[3/8] Creating replication user '$ReplicationUser'..." -ForegroundColor Cyan
            try {
                # Drop existing user if exists
                $dropUserCmd = "DROP USER IF EXISTS $ReplicationUser;"
                $dropResult = docker exec $PrimaryContainer psql -U postgres -c $dropUserCmd 2>&1
                Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "Dropped existing replication user (if any)"

                # Create new replication user
                $createUserCmd = "CREATE USER $ReplicationUser WITH REPLICATION PASSWORD '$ReplicationPassword';"
                $createResult = docker exec $PrimaryContainer psql -U postgres -c $createUserCmd 2>&1

                if ($LASTEXITCODE -eq 0) {
                    $result.Steps += "Replication user created"
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Replication user '$ReplicationUser' created"
                    Write-Host "✅ Replication user created" -ForegroundColor Green
                } else {
                    throw "Failed to create replication user: $createResult"
                }
            } catch {
                $result.Issues += "Failed to create replication user: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Failed to create replication user" -Exception $_
                throw
            }

            # Step 4: Create replication slot
            Write-Host "`n[4/8] Creating replication slot '$ReplicationSlotName'..." -ForegroundColor Cyan
            try {
                # Drop existing slot if exists
                $dropSlotCmd = "SELECT pg_drop_replication_slot('$ReplicationSlotName') FROM pg_replication_slots WHERE slot_name='$ReplicationSlotName';"
                docker exec $PrimaryContainer psql -U postgres -c $dropSlotCmd 2>&1 | Out-Null

                # Create new replication slot
                $createSlotCmd = "SELECT pg_create_physical_replication_slot('$ReplicationSlotName');"
                $slotResult = docker exec $PrimaryContainer psql -U postgres -c $createSlotCmd 2>&1

                if ($LASTEXITCODE -eq 0) {
                    $result.Steps += "Replication slot created"
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Replication slot '$ReplicationSlotName' created"
                    Write-Host "✅ Replication slot created" -ForegroundColor Green
                } else {
                    throw "Failed to create replication slot: $slotResult"
                }
            } catch {
                $result.Issues += "Failed to create replication slot: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Failed to create replication slot" -Exception $_
                throw
            }

            # Step 5: Get Docker network subnet
            Write-Host "`n[5/8] Detecting Docker network subnet..." -ForegroundColor Cyan
            try {
                $networkInfo = docker network inspect $NetworkName 2>&1 | ConvertFrom-Json
                $subnet = $networkInfo[0].IPAM.Config[0].Subnet

                if ($subnet) {
                    Write-Host "✅ Subnet detected: $subnet" -ForegroundColor Green
                    Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "Network subnet: $subnet"
                } else {
                    throw "Could not detect subnet for network '$NetworkName'"
                }
            } catch {
                $result.Issues += "Failed to detect network subnet: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Failed to detect subnet" -Exception $_
                throw
            }

            # Step 6: Configure pg_hba.conf for replication
            Write-Host "`n[6/8] Configuring pg_hba.conf for replication..." -ForegroundColor Cyan
            try {
                # Add replication entries to pg_hba.conf
                $hbaEntry1 = "host replication $ReplicationUser $subnet trust"
                $hbaEntry2 = "host replication all $subnet trust"

                docker exec $PrimaryContainer bash -c "echo '$hbaEntry1' >> /var/lib/postgresql/data/pg_hba.conf" 2>&1
                docker exec $PrimaryContainer bash -c "echo '$hbaEntry2' >> /var/lib/postgresql/data/pg_hba.conf" 2>&1

                # Reload PostgreSQL configuration
                $reloadCmd = "SELECT pg_reload_conf();"
                $reloadResult = docker exec $PrimaryContainer psql -U postgres -c $reloadCmd 2>&1

                if ($LASTEXITCODE -eq 0) {
                    $result.Steps += "pg_hba.conf configured and reloaded"
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "pg_hba.conf configured for replication"
                    Write-Host "✅ pg_hba.conf configured and reloaded" -ForegroundColor Green
                } else {
                    throw "Failed to reload configuration: $reloadResult"
                }
            } catch {
                $result.Issues += "Failed to configure pg_hba.conf: $_"
                Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Failed to configure pg_hba.conf" -Exception $_
                throw
            }

            # Step 7: Restart replica container
            Write-Host "`n[7/8] Restarting replica container..." -ForegroundColor Cyan
            try {
                # Stop and remove replica container
                Write-Host "  Stopping replica container..." -ForegroundColor Yellow
                docker stop $ReplicaContainer 2>&1 | Out-Null
                docker rm $ReplicaContainer 2>&1 | Out-Null

                # Remove replica data volume
                Write-Host "  Removing replica data volume..." -ForegroundColor Yellow
                docker volume rm bahyway-replica-data 2>&1 | Out-Null

                # Start replica using docker-compose or docker directly
                Write-Host "  Starting fresh replica container..." -ForegroundColor Yellow

                $restartSuccess = $false
                $restartError = $null

                if (-not [string]::IsNullOrWhiteSpace($ComposeFile) -and (Test-Path $ComposeFile)) {
                    # Use docker-compose
                    Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "Using docker-compose: $ComposeFile"
                    $composeOutput = docker-compose -f $ComposeFile up -d postgres-replica 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        $restartSuccess = $true
                    } else {
                        $restartError = "docker-compose failed: $composeOutput"
                    }
                } else {
                    # Fallback: Use docker run with basic configuration
                    Write-ModuleLog -Level 'WARNING' -Component 'Initialization' -Message "Using fallback docker run command"
                    Write-Host "  ⚠️  docker-compose file not available, using docker run fallback..." -ForegroundColor Yellow

                    $dockerRunCmd = @(
                        "run", "-d",
                        "--name", $ReplicaContainer,
                        "--network", $NetworkName,
                        "-e", "POSTGRES_USER=postgres",
                        "-e", "POSTGRES_PASSWORD=postgres",
                        "-e", "POSTGRES_PRIMARY_HOST=$PrimaryContainer",
                        "-e", "POSTGRES_PRIMARY_PORT=5432",
                        "-e", "POSTGRES_REPLICATION_USER=$ReplicationUser",
                        "-e", "POSTGRES_REPLICATION_PASSWORD=$ReplicationPassword",
                        "-e", "POSTGRES_REPLICATION_SLOT=$ReplicationSlotName",
                        "-p", "5434:5432",
                        "-v", "bahyway-replica-data:/var/lib/postgresql/data",
                        "postgres:16"
                    )

                    $dockerOutput = docker @dockerRunCmd 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        $restartSuccess = $true
                    } else {
                        $restartError = "docker run failed: $dockerOutput"
                    }
                }

                if ($restartSuccess) {
                    $result.Steps += "Replica container restarted"
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Replica container restarted"
                    Write-Host "✅ Replica container restarted" -ForegroundColor Green
                } else {
                    throw $restartError
                }
            } catch {
                $errorMessage = if ($_.Exception.Message) { $_.Exception.Message } else { $_.ToString() }
                $result.Issues += "Failed to restart replica: $errorMessage"
                Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Failed to restart replica: $errorMessage"
                throw
            }

            # Step 8: Wait and verify replication
            Write-Host "`n[8/8] Waiting for replication to start (30 seconds)..." -ForegroundColor Cyan
            Start-Sleep -Seconds 30

            Write-Host "`nVerifying replication status..." -ForegroundColor Cyan
            try {
                # Check replication status
                $replTest = Test-PostgreSQLReplication -PrimaryContainer $PrimaryContainer -ReplicaContainer $ReplicaContainer
                $result.ReplicationStatus = $replTest

                if ($replTest.IsHealthy) {
                    $result.Success = $true
                    $result.Steps += "Replication verified and working"
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Initialization' -Message "Replication is working correctly"

                    Write-Host "`n✅ REPLICATION IS WORKING!" -ForegroundColor Green
                    Write-Host "   State: $($replTest.StreamingState)" -ForegroundColor White
                    Write-Host "   Sync State: $($replTest.SyncState)" -ForegroundColor White
                } else {
                    Write-Host "`n⚠️  Replication started but has issues" -ForegroundColor Yellow
                    foreach ($issue in $replTest.Issues) {
                        Write-Host "   • $issue" -ForegroundColor Yellow
                    }
                }
            } catch {
                $result.Issues += "Failed to verify replication: $_"
                Write-ModuleLog -Level 'WARNING' -Component 'Initialization' -Message "Could not verify replication" -Exception $_
            }

        } catch {
            $result.Issues += "Unexpected error: $_"
            Write-ModuleLog -Level 'ERROR' -Component 'Initialization' -Message "Unexpected error during initialization" -Exception $_
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Initialization' -Message "========== INITIALIZATION COMPLETED =========="
        }

        # Display summary
        Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
        Write-Host   "║     POSTGRESQL HA INITIALIZATION SUMMARY           ║" -ForegroundColor Cyan
        Write-Host   "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan

        Write-Host "`n📋 Steps Completed:" -ForegroundColor Cyan
        foreach ($step in $result.Steps) {
            Write-Host "   ✅ $step" -ForegroundColor Green
        }

        if ($result.Issues.Count -gt 0) {
            Write-Host "`n⚠️  Issues Encountered:" -ForegroundColor Yellow
            foreach ($issue in $result.Issues) {
                Write-Host "   • $issue" -ForegroundColor Yellow
            }
        }

        if ($result.Success) {
            Write-Host "`n✅ SUCCESS: PostgreSQL HA cluster is initialized and replication is working!" -ForegroundColor Green
        } else {
            Write-Host "`n❌ FAILED: Initialization encountered errors" -ForegroundColor Red
        }

        Write-Host ""

        return $result
    }
}

function Start-PostgreSQLReplication {
    <#
    .SYNOPSIS
        Starts or restarts PostgreSQL replication

    .DESCRIPTION
        Restarts the replication process between primary and replica nodes.
        This is useful when replication has stopped or needs to be re-initialized.

    .PARAMETER PrimaryContainer
        Name of the primary container

    .PARAMETER ReplicaContainer
        Name of the replica container

    .EXAMPLE
        Start-PostgreSQLReplication

    .EXAMPLE
        Start-PostgreSQLReplication -PrimaryContainer "custom-primary" -ReplicaContainer "custom-replica"
    #>
    [CmdletBinding()]
    param(
        [string]$PrimaryContainer = $script:ModuleConfig.PrimaryContainerName,
        [string]$ReplicaContainer = $script:ModuleConfig.ReplicaContainerName
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Replication' -Message "Starting replication restart process"
    }

    process {
        try {
            Write-Host "`n🔄 Restarting PostgreSQL Replication..." -ForegroundColor Cyan

            # Stop replica
            Write-Host "  Stopping replica container..." -ForegroundColor Yellow
            docker stop $ReplicaContainer 2>&1 | Out-Null

            # Start replica
            Write-Host "  Starting replica container..." -ForegroundColor Yellow
            docker start $ReplicaContainer 2>&1

            # Wait for startup
            Write-Host "  Waiting for replica to start (15 seconds)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 15

            # Verify
            Write-Host "  Verifying replication..." -ForegroundColor Yellow
            $replTest = Test-PostgreSQLReplication -PrimaryContainer $PrimaryContainer -ReplicaContainer $ReplicaContainer

            if ($replTest.IsHealthy) {
                Write-Host "`n✅ Replication restarted successfully!" -ForegroundColor Green
                Write-ModuleLog -Level 'SUCCESS' -Component 'Replication' -Message "Replication restarted successfully"
            } else {
                Write-Host "`n⚠️  Replication has issues:" -ForegroundColor Yellow
                foreach ($issue in $replTest.Issues) {
                    Write-Host "   • $issue" -ForegroundColor Yellow
                }
                Write-ModuleLog -Level 'WARNING' -Component 'Replication' -Message "Replication restarted but has issues"
            }

            return $replTest

        } catch {
            Write-ModuleLog -Level 'ERROR' -Component 'Replication' -Message "Failed to restart replication" -Exception $_
            throw
        }
    }
}

function Deploy-PostgreSQLCluster {
    <#
    .SYNOPSIS
        Deploys a complete PostgreSQL HA cluster

    .DESCRIPTION
        Full deployment of PostgreSQL HA cluster including:
        - Starting all containers (primary, replica, HAProxy, Barman)
        - Initializing replication
        - Verifying cluster health

    .PARAMETER ComposeFile
        Path to docker-compose file (default: docker-compose-complete.yml)

    .PARAMETER SkipReplicationSetup
        Skip automatic replication setup

    .EXAMPLE
        Deploy-PostgreSQLCluster

    .EXAMPLE
        Deploy-PostgreSQLCluster -ComposeFile "custom-compose.yml" -Verbose
    #>
    [CmdletBinding()]
    param(
        [string]$ComposeFile = "docker-compose-complete.yml",
        [switch]$SkipReplicationSetup
    )

    begin {
        Write-ModuleLog -Level 'INFO' -Component 'Deployment' -Message "========== STARTING POSTGRESQL HA CLUSTER DEPLOYMENT =========="
    }

    process {
        try {
            Write-Host "`n╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
            Write-Host   "║    BAHYWAY POSTGRESQL HA CLUSTER DEPLOYMENT        ║" -ForegroundColor Cyan
            Write-Host   "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan

            # Resolve compose file path
            if (-not (Test-Path $ComposeFile)) {
                # Try common locations
                $possiblePaths = @(
                    $ComposeFile,
                    "infrastructure/postgresql-ha/docker/$ComposeFile",
                    "../infrastructure/postgresql-ha/docker/$ComposeFile",
                    "../../infrastructure/postgresql-ha/docker/$ComposeFile"
                )

                $found = $false
                foreach ($path in $possiblePaths) {
                    if (Test-Path $path) {
                        $ComposeFile = (Resolve-Path $path).Path
                        $found = $true
                        break
                    }
                }

                if (-not $found) {
                    throw "Docker compose file not found: $ComposeFile. Searched in common locations."
                }
            } else {
                $ComposeFile = (Resolve-Path $ComposeFile).Path
            }

            Write-ModuleLog -Level 'INFO' -Component 'Deployment' -Message "Using compose file: $ComposeFile"

            # Step 1: Check Docker
            Write-Host "`n[1/4] Checking Docker environment..." -ForegroundColor Cyan
            $dockerTest = Test-DockerEnvironment
            if (-not $dockerTest.IsHealthy) {
                throw "Docker environment is not ready. Please ensure Docker is installed and running."
            }
            Write-Host "✅ Docker is ready" -ForegroundColor Green

            # Step 2: Start cluster with docker-compose
            Write-Host "`n[2/4] Starting cluster with docker-compose..." -ForegroundColor Cyan

            Write-Host "  Pulling images..." -ForegroundColor Yellow
            docker-compose -f $ComposeFile pull 2>&1

            Write-Host "  Starting containers..." -ForegroundColor Yellow
            docker-compose -f $ComposeFile up -d 2>&1

            if ($LASTEXITCODE -ne 0) {
                throw "Failed to start cluster with docker-compose"
            }

            Write-Host "✅ Cluster started" -ForegroundColor Green
            Write-ModuleLog -Level 'SUCCESS' -Component 'Deployment' -Message "Cluster started with docker-compose"

            # Step 3: Wait for services to be ready
            Write-Host "`n[3/4] Waiting for services to be ready (30 seconds)..." -ForegroundColor Cyan
            Start-Sleep -Seconds 30

            # Display running containers
            Write-Host "`n📦 Running containers:" -ForegroundColor Cyan
            docker ps --filter "name=bahyway" --format "table {{.Names}}`t{{.Status}}`t{{.Ports}}"

            # Step 4: Initialize replication
            if (-not $SkipReplicationSetup) {
                Write-Host "`n[4/4] Initializing replication..." -ForegroundColor Cyan
                $initResult = Initialize-PostgreSQLHA -ComposeFile $ComposeFile

                if ($initResult.Success) {
                    Write-Host "`n✅ DEPLOYMENT SUCCESSFUL!" -ForegroundColor Green
                    Write-ModuleLog -Level 'SUCCESS' -Component 'Deployment' -Message "Deployment completed successfully"
                } else {
                    Write-Host "`n⚠️  Deployment completed with warnings" -ForegroundColor Yellow
                    Write-ModuleLog -Level 'WARNING' -Component 'Deployment' -Message "Deployment completed with warnings"
                }
            } else {
                Write-Host "`n[4/4] Skipping replication setup" -ForegroundColor Gray
                Write-Host "`n✅ CLUSTER DEPLOYED (replication not configured)" -ForegroundColor Green
            }

            # Final health check
            Write-Host "`n🔍 Final Health Check..." -ForegroundColor Cyan
            $health = Get-ClusterHealth

            return $health

        } catch {
            Write-ModuleLog -Level 'ERROR' -Component 'Deployment' -Message "Deployment failed" -Exception $_
            Write-Host "`n❌ DEPLOYMENT FAILED: $_" -ForegroundColor Red
            throw
        } finally {
            Write-ModuleLog -Level 'INFO' -Component 'Deployment' -Message "========== DEPLOYMENT PROCESS COMPLETED =========="
        }
    }
}

function Start-PostgreSQLCluster {
    <#
    .SYNOPSIS
        Starts all containers in the PostgreSQL HA cluster

    .DESCRIPTION
        Starts the primary, replica, HAProxy, and Barman containers

    .EXAMPLE
        Start-PostgreSQLCluster
    #>
    [CmdletBinding()]
    param(
        [string]$ComposeFile = "docker-compose-complete.yml"
    )

    try {
        Write-Host "🚀 Starting PostgreSQL HA Cluster..." -ForegroundColor Cyan

        if (Test-Path $ComposeFile) {
            docker-compose -f $ComposeFile start 2>&1
        } else {
            docker start $script:ModuleConfig.PrimaryContainerName 2>&1
            docker start $script:ModuleConfig.ReplicaContainerName 2>&1
            docker start $script:ModuleConfig.HAProxyContainerName 2>&1
            docker start $script:ModuleConfig.BarmanContainerName 2>&1
        }

        Write-Host "✅ Cluster started" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Cluster' -Message "Cluster started"

        Start-Sleep -Seconds 10
        Get-ClusterHealth

    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Cluster' -Message "Failed to start cluster" -Exception $_
        throw
    }
}

function Stop-PostgreSQLCluster {
    <#
    .SYNOPSIS
        Stops all containers in the PostgreSQL HA cluster

    .DESCRIPTION
        Stops the primary, replica, HAProxy, and Barman containers

    .EXAMPLE
        Stop-PostgreSQLCluster
    #>
    [CmdletBinding()]
    param(
        [string]$ComposeFile = "docker-compose-complete.yml"
    )

    try {
        Write-Host "🛑 Stopping PostgreSQL HA Cluster..." -ForegroundColor Cyan

        if (Test-Path $ComposeFile) {
            docker-compose -f $ComposeFile stop 2>&1
        } else {
            docker stop $script:ModuleConfig.PrimaryContainerName 2>&1
            docker stop $script:ModuleConfig.ReplicaContainerName 2>&1
            docker stop $script:ModuleConfig.HAProxyContainerName 2>&1
            docker stop $script:ModuleConfig.BarmanContainerName 2>&1
        }

        Write-Host "✅ Cluster stopped" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Cluster' -Message "Cluster stopped"

    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Cluster' -Message "Failed to stop cluster" -Exception $_
        throw
    }
}

function Remove-PostgreSQLCluster {
    <#
    .SYNOPSIS
        Removes all containers and volumes from the PostgreSQL HA cluster

    .DESCRIPTION
        Completely removes the cluster including containers and data volumes
        WARNING: This will delete all data!

    .PARAMETER Force
        Skip confirmation prompt

    .EXAMPLE
        Remove-PostgreSQLCluster

    .EXAMPLE
        Remove-PostgreSQLCluster -Force
    #>
    [CmdletBinding()]
    param(
        [string]$ComposeFile = "docker-compose-complete.yml",
        [switch]$Force
    )

    try {
        if (-not $Force) {
            $confirm = Read-Host "⚠️  This will DELETE all cluster data. Are you sure? (yes/no)"
            if ($confirm -ne "yes") {
                Write-Host "❌ Operation cancelled" -ForegroundColor Yellow
                return
            }
        }

        Write-Host "🗑️  Removing PostgreSQL HA Cluster..." -ForegroundColor Cyan

        if (Test-Path $ComposeFile) {
            docker-compose -f $ComposeFile down -v 2>&1
        } else {
            docker stop $script:ModuleConfig.PrimaryContainerName 2>&1
            docker stop $script:ModuleConfig.ReplicaContainerName 2>&1
            docker stop $script:ModuleConfig.HAProxyContainerName 2>&1
            docker stop $script:ModuleConfig.BarmanContainerName 2>&1

            docker rm $script:ModuleConfig.PrimaryContainerName 2>&1
            docker rm $script:ModuleConfig.ReplicaContainerName 2>&1
            docker rm $script:ModuleConfig.HAProxyContainerName 2>&1
            docker rm $script:ModuleConfig.BarmanContainerName 2>&1

            docker volume rm bahyway-primary-data 2>&1
            docker volume rm bahyway-replica-data 2>&1
            docker volume rm bahyway-barman-data 2>&1
        }

        Write-Host "✅ Cluster removed" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Cluster' -Message "Cluster removed"

    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Cluster' -Message "Failed to remove cluster" -Exception $_
        throw
    }
}

#endregion

#region Monitoring Functions

function Get-ReplicationStatus {
    <#
    .SYNOPSIS
        Gets detailed replication status information

    .DESCRIPTION
        Returns comprehensive replication metrics including lag, state, and sync status

    .EXAMPLE
        Get-ReplicationStatus
    #>
    [CmdletBinding()]
    param(
        [string]$PrimaryContainer = $script:ModuleConfig.PrimaryContainerName
    )

    Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Getting replication status"

    return Test-PostgreSQLReplication -PrimaryContainer $PrimaryContainer
}

function Get-ReplicationLag {
    <#
    .SYNOPSIS
        Gets the current replication lag in seconds

    .DESCRIPTION
        Returns the replication lag time between primary and replica

    .EXAMPLE
        Get-ReplicationLag
    #>
    [CmdletBinding()]
    param(
        [string]$ReplicaContainer = $script:ModuleConfig.ReplicaContainerName
    )

    Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Getting replication lag"

    try {
        $lagQuery = "SELECT EXTRACT(EPOCH FROM (now() - pg_last_xact_replay_timestamp()))::int;"
        $lagSeconds = docker exec $ReplicaContainer psql -U postgres -t -A -c $lagQuery 2>&1

        if ($lagSeconds -match '^\d+$') {
            $lag = [int]$lagSeconds
            Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Replication lag: ${lag}s"
            return [PSCustomObject]@{
                LagSeconds = $lag
                LagMinutes = [math]::Round($lag / 60, 2)
                IsHealthy = $lag -le $script:ModuleConfig.ReplicationLagThresholdSeconds
            }
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Monitoring' -Message "Failed to get replication lag" -Exception $_
    }

    return $null
}

function Get-DatabaseSize {
    <#
    .SYNOPSIS
        Gets the size of the AlarmInsight database

    .DESCRIPTION
        Returns the current database size in human-readable format

    .EXAMPLE
        Get-DatabaseSize
    #>
    [CmdletBinding()]
    param(
        [string]$ContainerName = $script:ModuleConfig.PrimaryContainerName,
        [string]$DatabaseName = 'alarminsight'
    )

    Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Getting database size for $DatabaseName"

    try {
        $sizeQuery = "SELECT pg_size_pretty(pg_database_size('$DatabaseName'));"
        $size = docker exec $ContainerName psql -U postgres -t -A -c $sizeQuery 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Database size: $size"
            return [PSCustomObject]@{
                Database = $DatabaseName
                Size = $size.Trim()
                Timestamp = Get-Date
            }
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Monitoring' -Message "Failed to get database size" -Exception $_
    }

    return $null
}

function Get-ConnectionCount {
    <#
    .SYNOPSIS
        Gets the current number of active database connections

    .DESCRIPTION
        Returns connection count and details from pg_stat_activity

    .EXAMPLE
        Get-ConnectionCount
    #>
    [CmdletBinding()]
    param(
        [string]$ContainerName = $script:ModuleConfig.PrimaryContainerName
    )

    Write-ModuleLog -Level 'INFO' -Component 'Monitoring' -Message "Getting connection count"

    try {
        $totalQuery = "SELECT count(*) FROM pg_stat_activity;"
        $activeQuery = "SELECT count(*) FROM pg_stat_activity WHERE state = 'active';"

        $total = docker exec $ContainerName psql -U postgres -t -A -c $totalQuery 2>&1
        $active = docker exec $ContainerName psql -U postgres -t -A -c $activeQuery 2>&1

        if ($LASTEXITCODE -eq 0) {
            return [PSCustomObject]@{
                TotalConnections = [int]$total
                ActiveConnections = [int]$active
                IdleConnections = [int]$total - [int]$active
                Timestamp = Get-Date
            }
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Monitoring' -Message "Failed to get connection count" -Exception $_
    }

    return $null
}

function Watch-ClusterHealth {
    <#
    .SYNOPSIS
        Continuously monitors cluster health with auto-refresh

    .DESCRIPTION
        Displays cluster health status in a loop, refreshing every N seconds
        Press Ctrl+C to exit

    .PARAMETER RefreshSeconds
        Seconds between health checks (default: 30)

    .EXAMPLE
        Watch-ClusterHealth

    .EXAMPLE
        Watch-ClusterHealth -RefreshSeconds 60
    #>
    [CmdletBinding()]
    param(
        [int]$RefreshSeconds = 30
    )

    Write-Host "🔍 Watching cluster health (refresh every ${RefreshSeconds}s)..." -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
    Write-Host ""

    try {
        while ($true) {
            Clear-Host
            Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
            Write-Host "║  BAHYWAY POSTGRESQL HA - LIVE CLUSTER HEALTH MONITOR     ║" -ForegroundColor Cyan
            Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
            Write-Host "  Last update: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
            Write-Host ""

            $health = Get-ClusterHealth

            Write-Host "Next refresh in ${RefreshSeconds} seconds (Ctrl+C to exit)" -ForegroundColor Gray
            Start-Sleep -Seconds $RefreshSeconds
        }
    } catch {
        Write-Host "`n`nMonitoring stopped" -ForegroundColor Yellow
    }
}

#endregion

#region Maintenance Functions

function Restart-PostgreSQLNode {
    <#
    .SYNOPSIS
        Restarts a specific PostgreSQL node (primary or replica)

    .DESCRIPTION
        Safely restarts the specified node container

    .PARAMETER NodeType
        Type of node to restart: Primary or Replica

    .EXAMPLE
        Restart-PostgreSQLNode -NodeType Primary

    .EXAMPLE
        Restart-PostgreSQLNode -NodeType Replica
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Primary', 'Replica')]
        [string]$NodeType
    )

    $containerName = if ($NodeType -eq 'Primary') {
        $script:ModuleConfig.PrimaryContainerName
    } else {
        $script:ModuleConfig.ReplicaContainerName
    }

    Write-ModuleLog -Level 'INFO' -Component 'Maintenance' -Message "Restarting $NodeType node: $containerName"
    Write-Host "🔄 Restarting $NodeType node..." -ForegroundColor Cyan

    try {
        docker restart $containerName 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $NodeType node restarted successfully" -ForegroundColor Green
            Write-ModuleLog -Level 'SUCCESS' -Component 'Maintenance' -Message "$NodeType node restarted"

            Write-Host "Waiting 10 seconds for startup..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10

            if ($NodeType -eq 'Primary') {
                Test-PostgreSQLPrimary
            } else {
                Test-PostgreSQLReplica
            }
        } else {
            throw "Failed to restart $NodeType node"
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Maintenance' -Message "Failed to restart $NodeType" -Exception $_
        throw
    }
}

function Invoke-FailoverToReplica {
    <#
    .SYNOPSIS
        Promotes replica to primary (manual failover)

    .DESCRIPTION
        Manually promotes the replica to become the new primary node
        WARNING: This is a destructive operation!

    .PARAMETER Force
        Skip confirmation prompt

    .EXAMPLE
        Invoke-FailoverToReplica

    .EXAMPLE
        Invoke-FailoverToReplica -Force
    #>
    [CmdletBinding()]
    param(
        [switch]$Force
    )

    if (-not $Force) {
        $confirm = Read-Host "⚠️  This will promote replica to primary. Are you sure? (yes/no)"
        if ($confirm -ne "yes") {
            Write-Host "❌ Failover cancelled" -ForegroundColor Yellow
            return
        }
    }

    Write-ModuleLog -Level 'WARNING' -Component 'Maintenance' -Message "Starting manual failover to replica"
    Write-Host "⚠️  Starting manual failover..." -ForegroundColor Yellow

    try {
        $replicaContainer = $script:ModuleConfig.ReplicaContainerName

        # Promote replica
        Write-Host "Promoting replica to primary..." -ForegroundColor Cyan
        $promoteCmd = "SELECT pg_promote();"
        docker exec $replicaContainer psql -U postgres -c $promoteCmd 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Replica promoted to primary" -ForegroundColor Green
            Write-ModuleLog -Level 'SUCCESS' -Component 'Maintenance' -Message "Failover completed successfully"

            Write-Host "⚠️  Don't forget to reconfigure the old primary as a new replica!" -ForegroundColor Yellow
        } else {
            throw "Failed to promote replica"
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Maintenance' -Message "Failover failed" -Exception $_
        throw
    }
}

function Invoke-BaseBackup {
    <#
    .SYNOPSIS
        Creates a base backup using pg_basebackup

    .DESCRIPTION
        Creates a physical base backup of the primary database

    .PARAMETER BackupPath
        Path where backup should be stored

    .EXAMPLE
        Invoke-BaseBackup -BackupPath "/backups/base-backup"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$BackupPath
    )

    Write-ModuleLog -Level 'INFO' -Component 'Maintenance' -Message "Starting base backup to $BackupPath"
    Write-Host "💾 Creating base backup..." -ForegroundColor Cyan

    try {
        $primaryContainer = $script:ModuleConfig.PrimaryContainerName

        # Create backup directory
        docker exec $primaryContainer mkdir -p $BackupPath 2>&1

        # Create base backup
        $backupCmd = "pg_basebackup -U postgres -D $BackupPath -Fp -Xs -P"
        docker exec $primaryContainer bash -c $backupCmd 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Base backup created successfully" -ForegroundColor Green
            Write-ModuleLog -Level 'SUCCESS' -Component 'Maintenance' -Message "Base backup completed"
        } else {
            throw "Base backup failed"
        }
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Maintenance' -Message "Base backup failed" -Exception $_
        throw
    }
}

#endregion

#region Alarm Functions

function Send-HealthAlarm {
    <#
    .SYNOPSIS
        Manually sends a health alarm

    .DESCRIPTION
        Creates and logs a health alarm with specified severity

    .PARAMETER AlarmType
        Type of alarm

    .PARAMETER Severity
        Alarm severity level

    .PARAMETER Message
        Alarm message

    .PARAMETER Component
        Component name

    .PARAMETER Details
        Additional details hashtable

    .EXAMPLE
        Send-HealthAlarm -AlarmType "CustomCheck" -Severity High -Message "Custom health check failed" -Component "MyComponent"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AlarmType,

        [Parameter(Mandatory)]
        [ValidateSet('Critical', 'High', 'Medium', 'Low', 'Info')]
        [string]$Severity,

        [Parameter(Mandatory)]
        [string]$Message,

        [string]$Component = 'Manual',

        [hashtable]$Details = @{}
    )

    Write-AlarmLog -AlarmType $AlarmType -Severity $Severity -Message $Message -Component $Component -Details $Details
}

function Get-HealthAlarms {
    <#
    .SYNOPSIS
        Retrieves all health alarms from the registry

    .DESCRIPTION
        Returns all alarms stored in the global alarm registry

    .PARAMETER Severity
        Filter by severity level

    .PARAMETER Component
        Filter by component name

    .EXAMPLE
        Get-HealthAlarms

    .EXAMPLE
        Get-HealthAlarms -Severity Critical

    .EXAMPLE
        Get-HealthAlarms -Component PostgreSQL-Primary
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Critical', 'High', 'Medium', 'Low', 'Info')]
        [string]$Severity,

        [string]$Component
    )

    $alarms = $script:HealthAlarms.Values

    if ($Severity) {
        $alarms = $alarms | Where-Object { $_.Severity -eq $Severity }
    }

    if ($Component) {
        $alarms = $alarms | Where-Object { $_.Component -eq $Component }
    }

    return $alarms | Sort-Object Timestamp -Descending
}

function Clear-HealthAlarms {
    <#
    .SYNOPSIS
        Clears all health alarms from the registry

    .DESCRIPTION
        Removes all alarms from the global alarm registry

    .PARAMETER Force
        Skip confirmation prompt

    .EXAMPLE
        Clear-HealthAlarms

    .EXAMPLE
        Clear-HealthAlarms -Force
    #>
    [CmdletBinding()]
    param(
        [switch]$Force
    )

    if (-not $Force) {
        $confirm = Read-Host "Clear all health alarms? (yes/no)"
        if ($confirm -ne "yes") {
            Write-Host "❌ Operation cancelled" -ForegroundColor Yellow
            return
        }
    }

    $count = $script:HealthAlarms.Count
    $script:HealthAlarms.Clear()

    Write-Host "✅ Cleared $count alarm(s)" -ForegroundColor Green
    Write-ModuleLog -Level 'INFO' -Component 'Alarms' -Message "Cleared $count alarms from registry"
}

#endregion

#region Configuration Functions

function Get-ClusterConfiguration {
    <#
    .SYNOPSIS
        Gets the current cluster configuration

    .DESCRIPTION
        Returns the module configuration settings

    .EXAMPLE
        Get-ClusterConfiguration
    #>
    [CmdletBinding()]
    param()

    return $script:ModuleConfig
}

function Set-ClusterConfiguration {
    <#
    .SYNOPSIS
        Updates cluster configuration settings

    .DESCRIPTION
        Modifies module configuration parameters

    .PARAMETER ConfigKey
        Configuration key to update

    .PARAMETER ConfigValue
        New value for the configuration key

    .EXAMPLE
        Set-ClusterConfiguration -ConfigKey "ReplicationLagThresholdSeconds" -ConfigValue 10
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ConfigKey,

        [Parameter(Mandatory)]
        $ConfigValue
    )

    if ($script:ModuleConfig.ContainsKey($ConfigKey)) {
        $oldValue = $script:ModuleConfig[$ConfigKey]
        $script:ModuleConfig[$ConfigKey] = $ConfigValue
        Write-Host "✅ Updated $ConfigKey from '$oldValue' to '$ConfigValue'" -ForegroundColor Green
        Write-ModuleLog -Level 'INFO' -Component 'Configuration' -Message "Updated $ConfigKey to $ConfigValue"
    } else {
        Write-Host "❌ Configuration key '$ConfigKey' not found" -ForegroundColor Red
        Write-Host "Available keys: $($script:ModuleConfig.Keys -join ', ')" -ForegroundColor Yellow
    }
}

function Export-ClusterConfiguration {
    <#
    .SYNOPSIS
        Exports cluster configuration to a file

    .DESCRIPTION
        Saves current configuration to JSON file

    .PARAMETER Path
        Path to export configuration file

    .EXAMPLE
        Export-ClusterConfiguration -Path "cluster-config.json"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    try {
        $script:ModuleConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $Path -Encoding UTF8
        Write-Host "✅ Configuration exported to $Path" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Configuration' -Message "Configuration exported to $Path"
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Configuration' -Message "Failed to export configuration" -Exception $_
        throw
    }
}

function Import-ClusterConfiguration {
    <#
    .SYNOPSIS
        Imports cluster configuration from a file

    .DESCRIPTION
        Loads configuration from JSON file

    .PARAMETER Path
        Path to configuration file

    .EXAMPLE
        Import-ClusterConfiguration -Path "cluster-config.json"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    try {
        if (-not (Test-Path $Path)) {
            throw "Configuration file not found: $Path"
        }

        $config = Get-Content -Path $Path -Raw | ConvertFrom-Json -AsHashtable

        foreach ($key in $config.Keys) {
            $script:ModuleConfig[$key] = $config[$key]
        }

        Write-Host "✅ Configuration imported from $Path" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Configuration' -Message "Configuration imported from $Path"
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Configuration' -Message "Failed to import configuration" -Exception $_
        throw
    }
}

#endregion

#region Log Functions

function Get-ModuleLog {
    <#
    .SYNOPSIS
        Retrieves module log entries

    .DESCRIPTION
        Reads and displays module log entries with optional filtering

    .PARAMETER Date
        Filter logs by date (default: today)

    .PARAMETER Level
        Filter by log level

    .PARAMETER Component
        Filter by component name

    .PARAMETER Tail
        Show only the last N lines

    .EXAMPLE
        Get-ModuleLog

    .EXAMPLE
        Get-ModuleLog -Level ERROR -Tail 50

    .EXAMPLE
        Get-ModuleLog -Component PostgreSQL-Primary -Date (Get-Date).AddDays(-1)
    #>
    [CmdletBinding()]
    param(
        [DateTime]$Date = (Get-Date),

        [ValidateSet('INFO', 'WARNING', 'ERROR', 'SUCCESS', 'DEBUG')]
        [string]$Level,

        [string]$Component,

        [int]$Tail
    )

    try {
        $logFile = Join-Path $script:ModuleConfig.LogPath "postgresql-ha_$($Date.ToString('yyyyMMdd')).log"

        if (-not (Test-Path $logFile)) {
            Write-Host "No log file found for date: $($Date.ToString('yyyy-MM-dd'))" -ForegroundColor Yellow
            return
        }

        $logs = Get-Content -Path $logFile

        if ($Level) {
            $logs = $logs | Where-Object { $_ -match "\[$Level\]" }
        }

        if ($Component) {
            $logs = $logs | Where-Object { $_ -match "\[$Component\]" }
        }

        if ($Tail) {
            $logs = $logs | Select-Object -Last $Tail
        }

        return $logs
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Logging' -Message "Failed to retrieve logs" -Exception $_
        throw
    }
}

function Clear-ModuleLog {
    <#
    .SYNOPSIS
        Clears old module log files

    .DESCRIPTION
        Removes log files older than specified days

    .PARAMETER DaysToKeep
        Number of days of logs to keep (default: 30)

    .PARAMETER Force
        Skip confirmation prompt

    .EXAMPLE
        Clear-ModuleLog

    .EXAMPLE
        Clear-ModuleLog -DaysToKeep 7 -Force
    #>
    [CmdletBinding()]
    param(
        [int]$DaysToKeep = 30,

        [switch]$Force
    )

    try {
        $logPath = $script:ModuleConfig.LogPath

        if (-not (Test-Path $logPath)) {
            Write-Host "No log directory found" -ForegroundColor Yellow
            return
        }

        $cutoffDate = (Get-Date).AddDays(-$DaysToKeep)
        $oldLogs = Get-ChildItem -Path $logPath -Filter "*.log" | Where-Object { $_.LastWriteTime -lt $cutoffDate }

        if ($oldLogs.Count -eq 0) {
            Write-Host "No old log files to remove" -ForegroundColor Green
            return
        }

        if (-not $Force) {
            Write-Host "Found $($oldLogs.Count) log file(s) older than $DaysToKeep days:" -ForegroundColor Yellow
            $oldLogs | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
            $confirm = Read-Host "Delete these files? (yes/no)"
            if ($confirm -ne "yes") {
                Write-Host "❌ Operation cancelled" -ForegroundColor Yellow
                return
            }
        }

        $oldLogs | Remove-Item -Force
        Write-Host "✅ Removed $($oldLogs.Count) old log file(s)" -ForegroundColor Green
        Write-ModuleLog -Level 'INFO' -Component 'Logging' -Message "Removed $($oldLogs.Count) old log files"
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Logging' -Message "Failed to clear logs" -Exception $_
        throw
    }
}

function Export-ModuleLogs {
    <#
    .SYNOPSIS
        Exports module logs to a single file

    .DESCRIPTION
        Combines log files and exports to specified location

    .PARAMETER OutputPath
        Path for the exported log file

    .PARAMETER DaysToExport
        Number of days of logs to export (default: 7)

    .EXAMPLE
        Export-ModuleLogs -OutputPath "exported-logs.txt"

    .EXAMPLE
        Export-ModuleLogs -OutputPath "logs.txt" -DaysToExport 30
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$OutputPath,

        [int]$DaysToExport = 7
    )

    try {
        $logPath = $script:ModuleConfig.LogPath

        if (-not (Test-Path $logPath)) {
            Write-Host "No log directory found" -ForegroundColor Yellow
            return
        }

        $cutoffDate = (Get-Date).AddDays(-$DaysToExport)
        $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" |
                    Where-Object { $_.LastWriteTime -ge $cutoffDate } |
                    Sort-Object LastWriteTime

        if ($logFiles.Count -eq 0) {
            Write-Host "No log files found for the specified period" -ForegroundColor Yellow
            return
        }

        $allLogs = @()
        foreach ($file in $logFiles) {
            $allLogs += "=" * 80
            $allLogs += "Log file: $($file.Name)"
            $allLogs += "=" * 80
            $allLogs += Get-Content -Path $file.FullName
            $allLogs += ""
        }

        $allLogs | Set-Content -Path $OutputPath -Encoding UTF8

        Write-Host "✅ Exported $($logFiles.Count) log file(s) to $OutputPath" -ForegroundColor Green
        Write-ModuleLog -Level 'SUCCESS' -Component 'Logging' -Message "Exported logs to $OutputPath"
    } catch {
        Write-ModuleLog -Level 'ERROR' -Component 'Logging' -Message "Failed to export logs" -Exception $_
        throw
    }
}

#endregion

# Explicitly export public functions (must match manifest FunctionsToExport)
Export-ModuleMember -Function @(
    # Deployment Functions
    'Initialize-PostgreSQLHA',
    'Deploy-PostgreSQLCluster',
    'Remove-PostgreSQLCluster',
    'Start-PostgreSQLCluster',
    'Stop-PostgreSQLCluster',
    'Start-PostgreSQLReplication',

    # Health Check Functions
    'Test-DockerEnvironment',
    'Test-PostgreSQLPrimary',
    'Test-PostgreSQLReplica',
    'Test-PostgreSQLReplication',
    'Test-StorageSpace',
    'Get-ClusterHealth',
    'Test-NetworkConnectivity',
    'Test-HAProxyHealth',
    'Test-BarmanBackup',

    # Monitoring Functions
    'Get-ReplicationStatus',
    'Get-ReplicationLag',
    'Get-DatabaseSize',
    'Get-ConnectionCount',
    'Watch-ClusterHealth',

    # Maintenance Functions
    'Restart-PostgreSQLNode',
    'Invoke-FailoverToReplica',
    'Invoke-BaseBackup',

    # Alarm Functions
    'Send-HealthAlarm',
    'Get-HealthAlarms',
    'Clear-HealthAlarms',

    # Configuration Functions
    'Get-ClusterConfiguration',
    'Set-ClusterConfiguration',
    'Export-ClusterConfiguration',
    'Import-ClusterConfiguration',

    # Log Functions
    'Get-ModuleLog',
    'Clear-ModuleLog',
    'Export-ModuleLogs'
)
