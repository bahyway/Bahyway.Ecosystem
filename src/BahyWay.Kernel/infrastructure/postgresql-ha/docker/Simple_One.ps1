# Navigate to directory
cd C:\Users\Bahaa\source\_OTAP\Bahyway_StillInDev\infrastructure\postgresql-ha\docker

# 1. Create replication user
Write-Host "Creating replication user..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres <<< "DROP USER IF EXISTS replicator;"
docker exec bahyway-postgres-primary psql -U postgres <<< "CREATE USER replicator WITH REPLICATION PASSWORD 'replicator123';"

# 2. Create replication slot
Write-Host "Creating replication slot..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres <<< "SELECT pg_create_physical_replication_slot('replica_slot');"

# 3. Get subnet
Write-Host "Getting Docker subnet..." -ForegroundColor Yellow
$subnet = (docker network inspect bahyway-network | ConvertFrom-Json)[0].IPAM.Config[0].Subnet
Write-Host "Subnet: $subnet" -ForegroundColor Cyan

# 4. Add pg_hba.conf entries
Write-Host "Adding pg_hba.conf entries..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary bash -c "echo 'host replication replicator $subnet trust' >> /var/lib/postgresql/data/pg_hba.conf"
docker exec bahyway-postgres-primary bash -c "echo 'host replication all $subnet trust' >> /var/lib/postgresql/data/pg_hba.conf"

# 5. Reload config
Write-Host "Reloading config..." -ForegroundColor Yellow
docker exec bahyway-postgres-primary psql -U postgres <<< "SELECT pg_reload_conf();"

# 6. Restart replica
Write-Host "Restarting replica..." -ForegroundColor Yellow
docker stop bahyway-postgres-replica
docker rm bahyway-postgres-replica
docker volume rm bahyway-replica-data

docker-compose -f docker-compose-complete.yml up -d postgres-replica

# 7. Wait and check
Write-Host "Waiting 30 seconds..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "`nChecking status..." -ForegroundColor Cyan
docker ps --filter "name=bahyway-postgres"

Write-Host "`nReplication status:" -ForegroundColor Cyan
docker exec bahyway-postgres-primary psql -U postgres <<< "SELECT * FROM pg_stat_replication;"

Write-Host "`nDone!" -ForegroundColor Green
