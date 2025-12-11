cd C:\Users\Bahaa\source\_OTAP\Bahyway_StillInDev\infrastructure\postgresql-ha\docker

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  POSTGRESQL HA - COMPLETE REPLICATION FIX                ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Step 1: Get Docker network subnet
Write-Host "`n[1/7] Finding Docker network subnet..." -ForegroundColor Yellow
$networkInfo = docker network inspect bahyway-network | ConvertFrom-Json
$subnet = $networkInfo[0].IPAM.Config[0].Subnet
Write-Host "   Subnet: $subnet" -ForegroundColor Green

# Step 2: Create replication user
Write-Host "`n[2/7] Creating replication user..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres -c "DROP USER IF EXISTS replicator;" 2>$null
docker exec bahyway-postgres-primary psql -U postgres -c "CREATE USER replicator WITH REPLICATION PASSWORD 'replicator123';"
Write-Host "   ✅ User created" -ForegroundColor Green

# Step 3: Create replication slot
Write-Host "`n[3/7] Creating replication slot..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_drop_replication_slot('replica_slot');" 2>$null
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_create_physical_replication_slot('replica_slot');"
Write-Host "   ✅ Slot created" -ForegroundColor Green

# Step 4: Configure pg_hba.conf
Write-Host "`n[4/7] Configuring pg_hba.conf for replication..." -ForegroundColor Yellow

# Create a temporary script file to avoid PowerShell escaping issues
$hbaScript = @"
#!/bin/bash
# Backup pg_hba.conf
cp /var/lib/postgresql/data/pg_hba.conf /var/lib/postgresql/data/pg_hba.conf.backup

# Remove old replication entries
sed -i '/replication/d' /var/lib/postgresql/data/pg_hba.conf

# Add new replication entries
echo '' >> /var/lib/postgresql/data/pg_hba.conf
echo '# Replication connections for BahyWay HA' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    replication     replicator      $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    replication     all             $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    all             all             $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf
"@

# Write script to temp file
$hbaScript | Out-File -FilePath "setup-hba.sh" -Encoding ASCII -NoNewline

# Copy script to container
docker cp setup-hba.sh bahyway-postgres-primary:/tmp/setup-hba.sh

# Execute script in container
docker exec bahyway-postgres-primary bash /tmp/setup-hba.sh

# Clean up
Remove-Item setup-hba.sh

Write-Host "   ✅ pg_hba.conf updated" -ForegroundColor Green

# Step 5: Reload PostgreSQL configuration
Write-Host "`n[5/7] Reloading PostgreSQL configuration..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_reload_conf();"
Write-Host "   ✅ Configuration reloaded" -ForegroundColor Green

# Step 6: Verify configuration
Write-Host "`n[6/7] Verifying configuration..." -ForegroundColor Yellow
Write-Host "`nReplication entries in pg_hba.conf:" -ForegroundColor Cyan
docker exec bahyway-postgres-primary cat /var/lib/postgresql/data/pg_hba.conf | Select-String "replication"

# Step 7: Clean and restart replica
Write-Host "`n[7/7] Restarting replica..." -ForegroundColor Yellow

docker stop bahyway-postgres-replica 2>$null
docker rm bahyway-postgres-replica 2>$null
docker volume rm bahyway-replica-data 2>$null

docker-compose -f docker-compose-complete.yml up -d postgres-replica

Write-Host "`nWaiting 30 seconds for replica initialization..." -ForegroundColor Yellow
for ($i = 30; $i -gt 0; $i--) {
    Write-Host -NoNewline "`r   $i seconds remaining... "
    Start-Sleep -Seconds 1
}
Write-Host ""

# Verification
Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  VERIFICATION                                            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Host "`n📦 Container Status:" -ForegroundColor Yellow
docker ps --filter "name=bahyway-postgres"

Write-Host "`n🔄 Replication Status:" -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT client_addr, state, sync_state FROM pg_stat_replication;" 2>$null

Write-Host "`n📖 Replica Recovery Status:" -ForegroundColor Yellow
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();" 2>$null

Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  SETUP COMPLETE                                          ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
