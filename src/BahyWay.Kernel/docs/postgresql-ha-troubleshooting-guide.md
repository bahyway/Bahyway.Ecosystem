# PostgreSQL HA Module Setup & Troubleshooting Guide

**BahyWay Solutions - PostgreSQL High Availability Implementation**
**Date**: November 25, 2025
**Author**: BahyWay Engineering Team
**Module**: BahyWay.PostgreSQLHA PowerShell Module

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Environment Setup](#environment-setup)
3. [Initial Problem](#initial-problem)
4. [Issues Encountered & Solutions](#issues-encountered--solutions)
   - [Issue 1: Only 6 of 33 Functions Exported](#issue-1-only-6-of-33-functions-exported)
   - [Issue 2: Docker Daemon Connection Issues](#issue-2-docker-daemon-connection-issues)
   - [Issue 3: Replica Permission Errors](#issue-3-replica-permission-errors)
   - [Issue 4: Root Execution Not Permitted](#issue-4-root-execution-not-permitted)
5. [Final Solution](#final-solution)
6. [Testing & Verification](#testing--verification)
7. [Best Practices](#best-practices)
8. [Appendix](#appendix)

---

## Executive Summary

This document details the complete troubleshooting process for setting up the BahyWay PostgreSQL High Availability (HA) module with streaming replication. The implementation successfully established a primary-replica PostgreSQL cluster with zero replication lag and full operational status.

**Key Achievements:**
- ✅ Fixed PowerShell module to export all 33 functions
- ✅ Resolved Docker connectivity issues on Windows
- ✅ Fixed PostgreSQL replica permission problems
- ✅ Established streaming replication with zero lag
- ✅ All cluster health checks passing

**Technology Stack:**
- PostgreSQL 16
- Docker Desktop on Windows
- PowerShell 7.5.4
- Docker Compose 3.8
- WSL2 (Windows Subsystem for Linux)

---

## Environment Setup

### System Information
- **Platform**: Windows with Docker Desktop
- **PowerShell Version**: 7.5.4
- **Docker Version**: 27.2.0
- **PostgreSQL Version**: 16 (Official Docker Image)
- **Repository**: BahyWay/BahyWay
- **Branch**: `claude/set-default-bahyway-013aMB3GgxwwZ3ahVFqnb8CV`

### Project Structure
```
BahyWay/
├── src/
│   └── AlarmInsight.Infrastructure/
│       └── PowerShellModules/
│           └── BahyWay.PostgreSQLHA/
│               ├── BahyWay.PostgreSQLHA.psd1  # Module manifest
│               └── BahyWay.PostgreSQLHA.psm1  # Module implementation
└── infrastructure/
    └── postgresql-ha/
        └── docker/
            └── docker-compose-complete.yml    # Docker Compose config
```

### Module Functions Overview
The BahyWay.PostgreSQLHA module provides 33 functions organized in 7 categories:

| Category | Functions | Purpose |
|----------|-----------|---------|
| Deployment | 6 | Initialize, deploy, start/stop clusters |
| Health Checks | 9 | Verify Docker, PostgreSQL, network, storage |
| Monitoring | 5 | Replication status, lag, connections, size |
| Maintenance | 3 | Restart nodes, failover, backups |
| Alarms | 3 | Send, get, clear health alarms |
| Configuration | 4 | Manage cluster configuration |
| Logging | 3 | Module logs management |

---

## Initial Problem

### Symptom
When importing the BahyWay.PostgreSQLHA PowerShell module, only **6 out of 33 functions** were being exported:

```powershell
PS> Get-Command -Module BahyWay.PostgreSQLHA | Measure-Object
Count: 6  # Expected: 33
```

**Functions Actually Exported:**
1. Get-ClusterHealth
2. Test-DockerEnvironment
3. Test-PostgreSQLPrimary
4. Test-PostgreSQLReplica
5. Test-PostgreSQLReplication
6. Test-StorageSpace

**Missing Functions:** 27 functions including critical deployment and replication management functions.

---

## Issues Encountered & Solutions

### Issue 1: Only 6 of 33 Functions Exported

#### Root Cause
The user was on the **wrong Git branch**:
- Current branch: `claude/add-postgresql-replication-module-01CCBksmeqMKt7eWtCNGAs2e`
- Correct branch: `claude/set-default-bahyway-013aMB3GgxwwZ3ahVFqnb8CV`

Additionally, the `.psm1` file had an incorrect `Export-ModuleMember` statement:
```powershell
# WRONG - This was overriding the manifest
Export-ModuleMember -Function * -Alias *
```

#### Solution Steps

**Step 1: Switch to Correct Branch**
```powershell
# Check current branch
git branch --show-current

# Switch to correct branch
git checkout claude/set-default-bahyway-013aMB3GgxwwZ3ahVFqnb8CV

# Pull latest changes
git pull
```

**Step 2: Update Module Export Statement**

Changed from wildcard export to explicit function list:

**File**: `src/AlarmInsight.Infrastructure/PowerShellModules/BahyWay.PostgreSQLHA/BahyWay.PostgreSQLHA.psm1`

```powershell
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
```

**Step 3: Verify Fix**
```powershell
# Reimport module
Remove-Module BahyWay.PostgreSQLHA -Force -ErrorAction SilentlyContinue
Import-Module "C:\Users\Bahaa\source\repos\BahyWay\BahyWay\src\AlarmInsight.Infrastructure\PowerShellModules\BahyWay.PostgreSQLHA" -Force

# Verify all 33 functions are exported
Get-Command -Module BahyWay.PostgreSQLHA | Measure-Object
# Result: Count: 33 ✅
```

**Commit Message:**
```
commit 62b7965
Add explicit Export-ModuleMember for all 33 public functions

Instead of relying solely on the manifest's FunctionsToExport, explicitly
list all 33 public functions in Export-ModuleMember. This ensures PowerShell
properly exports all functions regardless of module caching issues.
```

---

### Issue 2: Docker Daemon Connection Issues

#### Symptom
```powershell
PS> Test-DockerEnvironment
[ERROR] Docker daemon not running

PS> docker ps
Get "http://192.168.178.48:2375/v1.47/containers/json": dial tcp 192.168.178.48:2375: i/o timeout
```

#### Root Cause
The `DOCKER_HOST` environment variable was set to a remote/non-existent Docker daemon address (`tcp://192.168.178.48:2375`), but Docker Desktop was running locally via named pipe.

#### Solution
```powershell
# Check current DOCKER_HOST setting
$env:DOCKER_HOST
# Output: tcp://192.168.178.48:2375

# Unset to use Docker Desktop's default connection
$env:DOCKER_HOST = $null

# Verify Docker works now
docker ps
# Output: Lists running containers ✅
```

**Explanation:** Docker Desktop on Windows uses a named pipe (`npipe:////./pipe/docker_engine`) by default. Setting `DOCKER_HOST` to a TCP address causes Docker CLI to attempt remote connections instead.

---

### Issue 3: Replica Permission Errors

#### Symptom
Replica container stuck in restart loop with error:
```
FATAL: data directory "/var/lib/postgresql/data" has invalid permissions
DETAIL: Permissions should be u=rwx (0700) or u=rwx,g=rx (0750).
```

#### Root Cause
The replica data volume was created with incorrect ownership/permissions, preventing PostgreSQL from starting.

#### Solution
**Recreate replica volume and use Initialize-PostgreSQLHA:**

```powershell
# Stop and remove replica
docker stop bahyway-postgres-replica
docker rm bahyway-postgres-replica

# Remove corrupted volume
docker volume rm bahyway-replica-data

# Use module function to properly initialize
Initialize-PostgreSQLHA -PrimaryContainer "bahyway-postgres-primary" -ReplicaContainer "bahyway-postgres-replica"
```

The `Initialize-PostgreSQLHA` function handles:
- Creating replication user
- Creating replication slot
- Configuring pg_hba.conf
- Setting correct permissions (chmod 0700)
- Restarting replica with proper configuration

---

### Issue 4: Root Execution Not Permitted

#### Symptom
Even after fixing permissions, replica continued restarting with:
```
"root" execution of the PostgreSQL server is not permitted.
The server must be started under an unprivileged user ID to prevent
possible system security compromise.
```

#### Root Cause
The docker-compose entrypoint script was running PostgreSQL as the root user. The script executed:
```bash
exec postgres  # This runs as current user (root)
```

PostgreSQL explicitly forbids running as root for security reasons.

#### Solution

**Modified docker-compose-complete.yml:**

**File**: `infrastructure/postgresql-ha/docker/docker-compose-complete.yml`

Changed the replica service entrypoint command:

```yaml
postgres-replica:
  # ... other configuration ...
  entrypoint: /bin/bash
  command:
    - -c
    - |
      set -e
      echo "Waiting for primary..."
      until pg_isready -h postgres-primary -U postgres; do sleep 2; done

      if [ ! -f /var/lib/postgresql/data/PG_VERSION ]; then
        echo "Creating base backup..."
        rm -rf /var/lib/postgresql/data/*
        PGPASSWORD=replicator123 pg_basebackup -h postgres-primary -U replicator -D /var/lib/postgresql/data -Fp -Xs -P -R

        echo "Fixing permissions..."
        chmod 0700 /var/lib/postgresql/data
        chown -R postgres:postgres /var/lib/postgresql/data
      fi

      echo "Starting replica..."
      exec gosu postgres postgres  # ← CHANGED FROM: exec postgres
```

**Key Change:**
```bash
# Before (WRONG - runs as root):
exec postgres

# After (CORRECT - switches to postgres user):
exec gosu postgres postgres
```

**Explanation:**
- `gosu` is a lightweight alternative to `sudo` for Docker containers
- Included in official PostgreSQL Docker images
- Allows the script to run as root for chmod/chown operations
- Then switches to postgres user before starting PostgreSQL server

**Commit Message:**
```
commit 96cad7e
Fix PostgreSQL replica 'root execution not permitted' error

Changed the replica entrypoint script to use 'gosu postgres postgres' instead
of 'exec postgres' to ensure the PostgreSQL server runs as the postgres user
instead of root.

The entrypoint script runs as root to perform chmod/chown operations on the
data directory, but then needs to switch to the postgres user before starting
the PostgreSQL server. Using gosu ensures this user switch happens correctly.

Fixes the issue where replica container was stuck in restart loop with error:
"root execution of the PostgreSQL server is not permitted"
```

**Verification Steps:**
```powershell
# Pull changes
git pull

# Navigate to docker directory
cd "infrastructure\postgresql-ha\docker"

# Recreate replica with fixed configuration
docker stop bahyway-postgres-replica
docker rm bahyway-postgres-replica
docker volume rm bahyway-replica-data
docker-compose -f docker-compose-complete.yml up -d postgres-replica

# Watch logs - should start successfully
docker logs bahyway-postgres-replica --follow
```

**Expected Output:**
```
Waiting for primary...
postgres-primary:5432 - accepting connections
Creating base backup...
30757/30757 kB (100%), 1/1 tablespace
Fixing permissions...
Starting replica...
[PostgreSQL startup logs]
database system is ready to accept read-only connections
started streaming WAL from primary at 0/9000000 on timeline 1 ✅
```

---

## Final Solution

### Complete Working Configuration

**docker-compose-complete.yml** (postgres-replica service):
```yaml
postgres-replica:
  image: postgres:16
  container_name: bahyway-postgres-replica
  hostname: postgres-replica
  restart: unless-stopped
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
  ports:
    - "5434:5432"
  networks:
    - bahyway-network
  volumes:
    - replica-data:/var/lib/postgresql/data
  depends_on:
    postgres-primary:
      condition: service_healthy
  entrypoint: /bin/bash
  command:
    - -c
    - |
      set -e
      echo "Waiting for primary..."
      until pg_isready -h postgres-primary -U postgres; do sleep 2; done

      if [ ! -f /var/lib/postgresql/data/PG_VERSION ]; then
        echo "Creating base backup..."
        rm -rf /var/lib/postgresql/data/*
        PGPASSWORD=replicator123 pg_basebackup -h postgres-primary -U replicator -D /var/lib/postgresql/data -Fp -Xs -P -R

        echo "Fixing permissions..."
        chmod 0700 /var/lib/postgresql/data
        chown -R postgres:postgres /var/lib/postgresql/data
      fi

      echo "Starting replica..."
      exec gosu postgres postgres
```

### Deployment Commands

**Complete setup from scratch:**
```powershell
# 1. Ensure correct Git branch
cd "C:\Users\Bahaa\source\repos\BahyWay\BahyWay"
git checkout claude/set-default-bahyway-013aMB3GgxwwZ3ahVFqnb8CV
git pull

# 2. Clear Docker HOST environment variable
$env:DOCKER_HOST = $null

# 3. Import PowerShell module
Import-Module "src\AlarmInsight.Infrastructure\PowerShellModules\BahyWay.PostgreSQLHA" -Force

# 4. Verify all 33 functions are available
Get-Command -Module BahyWay.PostgreSQLHA | Measure-Object

# 5. Navigate to docker directory
cd "infrastructure\postgresql-ha\docker"

# 6. Stop and clean up any existing containers
docker-compose -f docker-compose-complete.yml down
docker volume rm bahyway-primary-data bahyway-replica-data -ErrorAction SilentlyContinue

# 7. Start the cluster
docker-compose -f docker-compose-complete.yml up -d

# 8. Wait for containers to initialize (60 seconds)
Start-Sleep -Seconds 60

# 9. Initialize replication
Initialize-PostgreSQLHA -PrimaryContainer "bahyway-postgres-primary" -ReplicaContainer "bahyway-postgres-replica"
```

---

## Testing & Verification

### Health Check Results

**Test Replication Status:**
```powershell
PS> Test-PostgreSQLReplication -PrimaryContainer "bahyway-postgres-primary" -ReplicaContainer "bahyway-postgres-replica"

IsHealthy          : True ✅
ReplicationActive  : True ✅
ReplicaConnected   : True ✅
StreamingState     : streaming ✅
SyncState          : async
WriteLag           : 00:00:00 ✅
FlushLag           : 00:00:00 ✅
ReplayLag          : 00:00:00 ✅
ReplicationSlots   : {replica_slot|physical|f}
Issues             : {}
```

**Comprehensive Cluster Health:**
```powershell
PS> Get-ClusterHealth

ALL SYSTEMS OPERATIONAL ✅

Component Status:
  Docker:       ✅
  Primary:      ✅
  Replica:      ✅
  Replication:  ✅
  Storage:      ✅

Metrics:
  Replication Lag:      s (seconds)
  Active Connections:   2
  Available Storage:    72GB
  Database Size:        7361 kB

AllIssues: {} (No issues detected)
```

**Replication Lag:**
```powershell
PS> Get-ReplicationLag -PrimaryContainer "bahyway-postgres-primary"

WriteLag     : 00:00:00
FlushLag     : 00:00:00
ReplayLag    : 00:00:00
```

### Container Status Verification

**Docker PS Output:**
```
CONTAINER ID   IMAGE         COMMAND                  CREATED         STATUS                   PORTS                    NAMES
f28ab27fbf9d   postgres:16   "docker-entrypoint.s…"   3 days ago      Up 2 days (healthy)      0.0.0.0:5432->5432/tcp   bahyway-postgres-primary
98a8002e3b3b   postgres:16   "/bin/bash -c 'set -…"   2 minutes ago   Up 2 minutes             0.0.0.0:5434->5432/tcp   bahyway-postgres-replica
```

**Key Indicators:**
- ✅ Primary: Status "Up 2 days (healthy)"
- ✅ Replica: Status "Up 2 minutes" (no restart loop)
- ✅ Both containers on bahyway-network
- ✅ Ports exposed correctly (5432 for primary, 5434 for replica)

### Replication Verification

**Check streaming replication is active:**
```powershell
# On primary - check for replication connections
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT * FROM pg_stat_replication;"

# Expected output:
 application_name | client_addr  | state     | sync_state | write_lag | flush_lag | replay_lag
------------------+--------------+-----------+------------+-----------+-----------+------------
 walreceiver      | 172.20.0.3   | streaming | async      | 00:00:00  | 00:00:00  | 00:00:00
```

**Test data replication:**
```powershell
# Create test table on primary
docker exec bahyway-postgres-primary psql -U postgres -d alarminsight -c "CREATE TABLE test_replication (id SERIAL PRIMARY KEY, data TEXT, created_at TIMESTAMP DEFAULT NOW());"

docker exec bahyway-postgres-primary psql -U postgres -d alarminsight -c "INSERT INTO test_replication (data) VALUES ('Test data from primary');"

# Wait 2 seconds for replication
Start-Sleep -Seconds 2

# Verify data appears on replica
docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -c "SELECT * FROM test_replication;"

# Expected output shows replicated data ✅
 id |         data            |         created_at
----+-------------------------+----------------------------
  1 | Test data from primary  | 2025-11-25 03:45:00.123456
```

---

## Best Practices

### 1. Module Import Best Practices

**Always clear module cache before importing:**
```powershell
Remove-Module BahyWay.PostgreSQLHA -Force -ErrorAction SilentlyContinue
Import-Module "path\to\BahyWay.PostgreSQLHA" -Force
```

**Verify function count after import:**
```powershell
$functionCount = (Get-Command -Module BahyWay.PostgreSQLHA).Count
if ($functionCount -ne 33) {
    Write-Error "Expected 33 functions, but got $functionCount"
}
```

### 2. Docker Environment Configuration

**Set Docker HOST appropriately:**
```powershell
# For Docker Desktop on Windows (default):
$env:DOCKER_HOST = $null

# For remote Docker daemon:
$env:DOCKER_HOST = "tcp://remote-host:2375"

# For Docker in WSL2:
$env:DOCKER_HOST = "unix:///var/run/docker.sock"
```

**Verify Docker connectivity before operations:**
```powershell
if (-not (docker ps 2>$null)) {
    throw "Docker is not accessible. Check DOCKER_HOST and Docker Desktop status."
}
```

### 3. PostgreSQL HA Deployment

**Always use Initialize-PostgreSQLHA for replication setup:**
```powershell
# DON'T manually configure replication
# DO use the module function:
Initialize-PostgreSQLHA -Verbose
```

**Check health after deployment:**
```powershell
# Wait for initialization
Start-Sleep -Seconds 60

# Verify cluster health
$health = Get-ClusterHealth
if (-not $health.IsHealthy) {
    Write-Warning "Cluster has issues:"
    $health.AllIssues | ForEach-Object { Write-Warning "  - $_" }
}
```

### 4. Docker Compose Best Practices

**Always use gosu for user switching in entrypoints:**
```yaml
command:
  - -c
  - |
    # Run privileged operations as root
    chown -R postgres:postgres /data

    # Switch to postgres user before starting server
    exec gosu postgres postgres
```

**Never run PostgreSQL as root:**
```bash
# WRONG ❌
exec postgres

# CORRECT ✅
exec gosu postgres postgres
```

### 5. Volume Management

**Clean up volumes when reinitializing:**
```powershell
# Stop containers first
docker-compose down

# Remove volumes
docker volume rm bahyway-primary-data bahyway-replica-data

# Recreate from scratch
docker-compose up -d
```

### 6. Troubleshooting Workflow

**Standard troubleshooting steps:**
```powershell
# 1. Check Docker
docker ps
docker logs <container-name>

# 2. Check module
Get-Command -Module BahyWay.PostgreSQLHA | Measure-Object

# 3. Check specific component
Test-DockerEnvironment
Test-PostgreSQLPrimary
Test-PostgreSQLReplica

# 4. Check replication
Test-PostgreSQLReplication

# 5. Check overall health
Get-ClusterHealth
```

---

## Appendix

### A. Git Commands Reference

```powershell
# Check current branch
git branch --show-current

# Switch branches
git checkout <branch-name>

# Pull latest changes
git pull origin <branch-name>

# View recent commits
git log --oneline -5

# Check file in specific commit
git show <commit-hash>:<file-path>

# Commit changes
git add <files>
git commit -m "commit message"
git push -u origin <branch-name>
```

### B. Docker Commands Reference

```powershell
# List containers
docker ps -a

# View logs
docker logs <container-name> --follow
docker logs <container-name> --tail 50

# Execute command in container
docker exec <container-name> <command>

# Inspect container
docker inspect <container-name>

# Container stats
docker stats <container-name>

# Network inspect
docker network inspect <network-name>

# Volume management
docker volume ls
docker volume inspect <volume-name>
docker volume rm <volume-name>

# Docker Compose
docker-compose -f <file> up -d
docker-compose -f <file> down
docker-compose -f <file> logs <service>
```

### C. PostgreSQL Commands Reference

```powershell
# Connect to database
docker exec -it <container-name> psql -U postgres -d <database>

# Check replication status
docker exec <container-name> psql -U postgres -c "SELECT * FROM pg_stat_replication;"

# Check replica status
docker exec <container-name> psql -U postgres -c "SELECT pg_is_in_recovery();"

# Check replication lag
docker exec <container-name> psql -U postgres -c "SELECT EXTRACT(EPOCH FROM (NOW() - pg_last_xact_replay_timestamp())) AS lag_seconds;"

# Check replication slots
docker exec <container-name> psql -U postgres -c "SELECT * FROM pg_replication_slots;"

# Check database size
docker exec <container-name> psql -U postgres -c "SELECT pg_size_pretty(pg_database_size('alarminsight'));"
```

### D. Module Functions Quick Reference

#### Deployment Functions
- `Initialize-PostgreSQLHA` - Initialize HA cluster with replication
- `Deploy-PostgreSQLCluster` - Deploy complete cluster
- `Remove-PostgreSQLCluster` - Remove cluster
- `Start-PostgreSQLCluster` - Start cluster services
- `Stop-PostgreSQLCluster` - Stop cluster services
- `Start-PostgreSQLReplication` - Start/restart replication

#### Health Check Functions
- `Test-DockerEnvironment` - Verify Docker is running
- `Test-PostgreSQLPrimary` - Check primary node health
- `Test-PostgreSQLReplica` - Check replica node health
- `Test-PostgreSQLReplication` - Check replication status
- `Test-StorageSpace` - Verify adequate storage
- `Test-NetworkConnectivity` - Test network between nodes
- `Test-HAProxyHealth` - Check HAProxy status
- `Test-BarmanBackup` - Verify Barman backup system
- `Get-ClusterHealth` - Comprehensive health check

#### Monitoring Functions
- `Get-ReplicationStatus` - Detailed replication metrics
- `Get-ReplicationLag` - Check replication lag
- `Get-DatabaseSize` - Get database size
- `Get-ConnectionCount` - Count active connections
- `Watch-ClusterHealth` - Continuous health monitoring

#### Maintenance Functions
- `Restart-PostgreSQLNode` - Restart a node
- `Invoke-FailoverToReplica` - Perform failover
- `Invoke-BaseBackup` - Create base backup

#### Alarm Functions
- `Send-HealthAlarm` - Send health alarm
- `Get-HealthAlarms` - Retrieve alarms
- `Clear-HealthAlarms` - Clear alarm history

#### Configuration Functions
- `Get-ClusterConfiguration` - Get current config
- `Set-ClusterConfiguration` - Update config
- `Export-ClusterConfiguration` - Export config to file
- `Import-ClusterConfiguration` - Import config from file

#### Log Functions
- `Get-ModuleLog` - Retrieve module logs
- `Clear-ModuleLog` - Clear log history
- `Export-ModuleLogs` - Export logs to file

### E. Common Error Messages & Solutions

| Error Message | Cause | Solution |
|---------------|-------|----------|
| "Docker daemon is not running" | DOCKER_HOST misconfigured or Docker Desktop not running | Set `$env:DOCKER_HOST = $null` and start Docker Desktop |
| "data directory has invalid permissions" | Volume permissions incorrect | Recreate volume and use `Initialize-PostgreSQLHA` |
| "root execution not permitted" | PostgreSQL running as root | Use `exec gosu postgres postgres` in entrypoint |
| "Only 6 functions exported" | Wrong branch or module not updated | Switch to correct branch and pull latest changes |
| "No replication connections" | Replication not configured | Run `Initialize-PostgreSQLHA` |
| "pg_basebackup: error: connection failed" | Replication user not created | Run `Initialize-PostgreSQLHA` to create replication user |

### F. Related Documentation

- [PostgreSQL Replication Documentation](https://www.postgresql.org/docs/16/high-availability.html)
- [Docker Compose File Reference](https://docs.docker.com/compose/compose-file/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [PowerShell Module Manifests](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-25 | BahyWay Engineering | Initial documentation of PostgreSQL HA setup and troubleshooting |

---

## License

Copyright (c) 2025 BahyWay Solutions. All rights reserved.

This document is proprietary and confidential. Unauthorized copying, distribution, or use of this document, via any medium, is strictly prohibited.

---

**End of Document**
