@{
    # Module manifest for BahyWay PostgreSQL High Availability Management
    
    # Script module or binary module file associated with this manifest
    RootModule = 'BahyWay.PostgreSQLHA.psm1'
    
    # Version number of this module
    ModuleVersion = '1.0.0'
    
    # ID used to uniquely identify this module
    GUID = 'a7f3e8b1-4c9d-4e21-8f5a-3b2c1d9e6a4f'
    
    # Author of this module
    Author = 'Bahaa Fadam - BahyWay'
    
    # Company or vendor of this module
    CompanyName = 'BahyWay Solutions'
    
    # Copyright statement for this module
    Copyright = '(c) 2025 BahyWay Solutions. All rights reserved.'
    
    # Description of the functionality provided by this module
    Description = 'Enterprise-grade PostgreSQL High Availability management module for BahyWay infrastructure. Provides comprehensive health checks, monitoring, deployment, and alarm detection for PostgreSQL primary-replica clusters with Docker, HAProxy, and Barman backup integration.'
    
    # Minimum version of the PowerShell engine required by this module
    PowerShellVersion = '7.0'
    
    # Supported platforms
    CompatiblePSEditions = @('Core', 'Desktop')
    
    # Functions to export from this module
    FunctionsToExport = @(
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
    
    # Cmdlets to export from this module
    CmdletsToExport = @()
    
    # Variables to export from this module
    VariablesToExport = @()
    
    # Aliases to export from this module
    AliasesToExport = @()
    
    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{
        PSData = @{
            # Tags applied to this module
            Tags = @('PostgreSQL', 'HA', 'Docker', 'Database', 'Monitoring', 'BahyWay', 'AlarmInsight')
            
            # A URL to the license for this module
            LicenseUri = 'https://github.com/bahyway/postgresql-ha/blob/main/LICENSE'
            
            # A URL to the main website for this project
            ProjectUri = 'https://github.com/bahyway/postgresql-ha'
            
            # A URL to an icon representing this module
            IconUri = ''
            
            # Release notes of this module
            ReleaseNotes = @'
Version 1.0.0 (2025-11-22)
- Initial release
- Complete health check system
- Docker environment validation
- PostgreSQL primary/replica monitoring
- HAProxy integration
- Barman backup support
- Comprehensive logging with alarm detection
- Cross-platform support (Windows WSL2 + Linux)
- Ansible deployment integration
- Try-Catch-Finally error handling
- Alarm detection and notification system
'@
        }
    }
    
    # HelpInfo URI of this module
    HelpInfoURI = 'https://github.com/bahyway/postgresql-ha/wiki'
}