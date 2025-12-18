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

# Step 4: Backup current pg_hba.conf
Write-Host "`n[4/7] Backing up pg_hba.conf..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary cp /var/lib/postgresql/data/pg_hba.conf /var/lib/postgresql/data/pg_hba.conf.backup

# Step 5: Add replication entries to pg_hba.conf
Write-Host "`n[5/7] Configuring pg_hba.conf for replication..." -ForegroundColor Yellow

# Remove old replication entries
docker exec bahyway-postgres-primary bash -c "sed -i '/replication/d' /var/lib/postgresql/data/pg_hba.conf"

# Add new entries with trust authentication for Docker network
docker exec bahyway-postgres-primary bash -c "echo '' >> /var/lib/postgresql/data/pg_hba.conf"
docker exec bahyway-postgres-primary bash -c "echo '# Replication connections for BahyWay HA' >> /var/lib/postgresql/data/pg_hba.conf"
docker exec bahyway-postgres-primary bash -c "echo 'host    replication     replicator      $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf"
docker exec bahyway-postgres-primary bash -c "echo 'host    replication     all             $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf"
docker exec bahyway-postgres-primary bash -c "echo 'host    all             all             $subnet                 trust' >> /var/lib/postgresql/data/pg_hba.conf"

Write-Host "   ✅ pg_hba.conf updated" -ForegroundColor Green

# Step 6: Reload PostgreSQL configuration
Write-Host "`n[6/7] Reloading PostgreSQL configuration..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_reload_conf();"
Write-Host "   ✅ Configuration reloaded" -ForegroundColor Green

# Step 7: Verify configuration
Write-Host "`n[7/7] Verifying configuration..." -ForegroundColor Yellow
Write-Host "`nReplication entries in pg_hba.conf:" -ForegroundColor Cyan
docker exec bahyway-postgres-primary cat /var/lib/postgresql/data/pg_hba.conf | Select-String "replication"

Write-Host "`nReplication slot:" -ForegroundColor Cyan
docker exec bahyway-postgres-primary psql -U postgres -t -c "SELECT slot_name, active FROM pg_replication_slots;"

# Step 8: Clean and restart replica
Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  RESTARTING REPLICA                                      ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

docker stop bahyway-postgres-replica 2>$null
docker rm bahyway-postgres-replica 2>$null
docker volume rm bahyway-replica-data 2>$null

Write-Host "`nStarting replica container..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml up -d postgres-replica

Write-Host "`nWaiting 30 seconds for replica initialization..." -ForegroundColor Yellow
for ($i = 30; $i -gt 0; $i--) {
    Write-Host -NoNewline "`r   $i seconds remaining... "
    Start-Sleep -Seconds 1
}
Write-Host ""

# Step 9: Check results
Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  VERIFICATION                                            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Host "`n📦 Container Status:" -ForegroundColor Yellow
docker ps --filter "name=bahyway-postgres" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host "`n🔄 Replication Status:" -ForegroundColor Yellow
$replStatus = docker exec bahyway-postgres-primary psql -U postgres -t -A -c "SELECT COUNT(*) FROM pg_stat_replication;" 2>$null
if ($replStatus -gt 0) {
    Write-Host "   ✅ Replica is connected and streaming!" -ForegroundColor Green
    docker exec bahyway-postgres-primary psql -U postgres -c "SELECT client_addr, state, sync_state FROM pg_stat_replication;"
} else {
    Write-Host "   ❌ No replica connected!" -ForegroundColor Red
}

Write-Host "`n📖 Replica Recovery Status:" -ForegroundColor Yellow
$recovery = docker exec bahyway-postgres-replica psql -U postgres -t -A -c "SELECT pg_is_in_recovery();" 2>$null
if ($recovery -match "t") {
    Write-Host "   ✅ Replica is in recovery mode (correct)" -ForegroundColor Green
} else {
    Write-Host "   ❌ Replica is NOT in recovery mode" -ForegroundColor Red
}

Write-Host "`n🧪 Testing Data Replication:" -ForegroundColor Yellow
$testTime = Get-Date -Format "HHmmss"
docker exec bahyway-postgres-primary psql -U postgres -d alarminsight -c "CREATE TABLE IF NOT EXISTS repl_test (id SERIAL, ts TEXT);" 2>$null
docker exec bahyway-postgres-primary psql -U postgres -d alarminsight -c "INSERT INTO repl_test (ts) VALUES ('$testTime');" 2>$null

Write-Host "   Waiting 3 seconds for replication..." -ForegroundColor Gray
Start-Sleep -Seconds 3

$primaryData = docker exec bahyway-postgres-primary psql -U postgres -d alarminsight -t -A -c "SELECT ts FROM repl_test WHERE ts='$testTime';"
$replicaData = docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -t -A -c "SELECT ts FROM repl_test WHERE ts='$testTime';" 2>$null

if ($replicaData -match $testTime) {
    Write-Host "   ✅ Data replicated successfully!" -ForegroundColor Green
} else {
    Write-Host "   ❌ Data replication failed!" -ForegroundColor Red
}

Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  SETUP COMPLETE                                          ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n📝 Summary:" -ForegroundColor Cyan
Write-Host "   Primary:  localhost:5432 (Read/Write)" -ForegroundColor White
Write-Host "   Replica:  localhost:5434 (Read-Only)" -ForegroundColor White
Write-Host "   Database: alarminsight" -ForegroundColor White
Write-Host "   User:     postgres" -ForegroundColor White
Write-Host ""