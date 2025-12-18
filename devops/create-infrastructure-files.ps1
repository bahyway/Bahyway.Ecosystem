# ============================================
# BAHYWAY POSTGRESQL HA - FILE CREATOR
# Creates all 11 essential infrastructure files
# ============================================

$ErrorActionPreference = "Stop"

# Get current directory
$baseDir = Get-Location

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  BAHYWAY POSTGRESQL HA - INFRASTRUCTURE FILE CREATOR" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ============================================
# FILE 1: docker-compose.yml
# ============================================
Write-Host "📄 Creating docker/docker-compose.yml..." -ForegroundColor Yellow

$dockerCompose = @'
version: '3.8'

services:
  etcd:
    image: quay.io/coreos/etcd:v3.5.11
    container_name: bahyway-etcd
    hostname: etcd
    environment:
      ETCD_NAME: etcd
      ETCD_LISTEN_CLIENT_URLS: http://0.0.0.0:2379
      ETCD_ADVERTISE_CLIENT_URLS: http://etcd:2379
      ETCD_LISTEN_PEER_URLS: http://0.0.0.0:2380
      ETCD_INITIAL_ADVERTISE_PEER_URLS: http://etcd:2380
      ETCD_INITIAL_CLUSTER: etcd=http://etcd:2380
      ETCD_INITIAL_CLUSTER_STATE: new
      ETCD_INITIAL_CLUSTER_TOKEN: bahyway-etcd-cluster
    networks:
      - bahyway-network
    volumes:
      - etcd-data:/etcd-data
    ports:
      - "2379:2379"
      - "2380:2380"

  postgres-primary:
    image: postgres:16
    container_name: bahyway-postgres-primary
    hostname: postgres-primary
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-alarminsight}
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      POSTGRES_REPLICATION_USER: ${REPLICATION_USER:-replicator}
      POSTGRES_REPLICATION_PASSWORD: ${REPLICATION_PASSWORD:-replicator123}
      PGDATA: /var/lib/postgresql/data/pgdata
    volumes:
      - postgres-primary-data:/var/lib/postgresql/data
      - ./scripts/init-primary.sh:/docker-entrypoint-initdb.d/01-init-primary.sh:ro
      - ./config/postgresql-primary.conf:/etc/postgresql/postgresql.conf:ro
      - ./config/pg_hba.conf:/etc/postgresql/pg_hba.conf:ro
    networks:
      - bahyway-network
    ports:
      - "5432:5432"
    command: postgres -c config_file=/etc/postgresql/postgresql.conf -c hba_file=/etc/postgresql/pg_hba.conf
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  postgres-replica:
    image: postgres:16
    container_name: bahyway-postgres-replica
    hostname: postgres-replica
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-alarminsight}
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      PGDATA: /var/lib/postgresql/data/pgdata
      PRIMARY_HOST: postgres-primary
      REPLICATION_USER: ${REPLICATION_USER:-replicator}
      REPLICATION_PASSWORD: ${REPLICATION_PASSWORD:-replicator123}
    volumes:
      - postgres-replica-data:/var/lib/postgresql/data
      - ./scripts/init-replica.sh:/docker-entrypoint-initdb.d/01-init-replica.sh:ro
      - ./config/postgresql-replica.conf:/etc/postgresql/postgresql.conf:ro
    networks:
      - bahyway-network
    ports:
      - "5433:5432"
    depends_on:
      postgres-primary:
        condition: service_healthy
    command: postgres -c config_file=/etc/postgresql/postgresql.conf

  barman:
    image: postgres:16
    container_name: bahyway-barman
    hostname: barman
    environment:
      POSTGRES_PRIMARY_HOST: postgres-primary
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      BARMAN_BACKUP_USER: ${BARMAN_USER:-barman}
      BARMAN_BACKUP_PASSWORD: ${BARMAN_PASSWORD:-barman123}
    volumes:
      - barman-data:/var/lib/barman
      - barman-wal:/var/lib/barman/wal
      - ./scripts/init-barman.sh:/docker-entrypoint-initdb.d/01-init-barman.sh:ro
    networks:
      - bahyway-network
    depends_on:
      postgres-primary:
        condition: service_healthy
    command: tail -f /dev/null

  haproxy:
    image: haproxy:2.9
    container_name: bahyway-haproxy
    hostname: haproxy
    volumes:
      - ./config/haproxy.cfg:/usr/local/etc/haproxy/haproxy.cfg:ro
    networks:
      - bahyway-network
    ports:
      - "5000:5000"
      - "5001:5001"
      - "7000:7000"
    depends_on:
      - postgres-primary
      - postgres-replica

networks:
  bahyway-network:
    driver: bridge
    name: bahyway-network

volumes:
  etcd-data:
    name: bahyway-etcd-data
  postgres-primary-data:
    name: bahyway-postgres-primary-data
  postgres-replica-data:
    name: bahyway-postgres-replica-data
  barman-data:
    name: bahyway-barman-data
  barman-wal:
    name: bahyway-barman-wal
'@

$dockerCompose | Out-File -FilePath "docker/docker-compose.yml" -Encoding UTF8 -NoNewline
Write-Host "   ✅ docker-compose.yml created" -ForegroundColor Green

# ============================================
# FILE 2: .env
# ============================================
Write-Host "📄 Creating docker/.env..." -ForegroundColor Yellow

$envFile = @'
POSTGRES_DB=alarminsight
POSTGRES_USER=postgres
POSTGRES_PASSWORD=StrongPassword123!

REPLICATION_USER=replicator
REPLICATION_PASSWORD=ReplicatorPass456!

BARMAN_USER=barman
BARMAN_PASSWORD=BarmanPass789!

POSTGRES_PRIMARY_PORT=5432
POSTGRES_REPLICA_PORT=5433
HAPROXY_PRIMARY_PORT=5000
HAPROXY_REPLICA_PORT=5001
HAPROXY_STATS_PORT=7000
'@

$envFile | Out-File -FilePath "docker/.env" -Encoding UTF8 -NoNewline
Write-Host "   ✅ .env created" -ForegroundColor Green

# ============================================
# FILE 3: postgresql-primary.conf
# ============================================
Write-Host "📄 Creating docker/config/postgresql-primary.conf..." -ForegroundColor Yellow

$primaryConf = @'
# Connection Settings
listen_addresses = '*'
port = 5432
max_connections = 200
superuser_reserved_connections = 3

# Memory Settings
shared_buffers = 256MB
effective_cache_size = 1GB
maintenance_work_mem = 64MB
work_mem = 4MB

# WAL Settings
wal_level = replica
wal_log_hints = on
max_wal_senders = 10
max_replication_slots = 10
wal_keep_size = 1GB
hot_standby = on

# Archiving
archive_mode = on
archive_command = 'test ! -f /var/lib/postgresql/wal_archive/%f && cp %p /var/lib/postgresql/wal_archive/%f'
archive_timeout = 300

# Checkpoint Settings
checkpoint_timeout = 15min
checkpoint_completion_target = 0.9
max_wal_size = 2GB
min_wal_size = 1GB

# Query Tuning
random_page_cost = 1.1
effective_io_concurrency = 200

# Logging
logging_collector = on
log_directory = 'log'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
log_rotation_age = 1d
log_rotation_size = 100MB
log_line_prefix = '%m [%p] %q%u@%d '
log_timezone = 'UTC'

# Monitoring
shared_preload_libraries = 'pg_stat_statements'
track_activities = on
track_counts = on
track_io_timing = on
track_functions = all

# Locale
datestyle = 'iso, mdy'
timezone = 'UTC'
lc_messages = 'en_US.utf8'
lc_monetary = 'en_US.utf8'
lc_numeric = 'en_US.utf8'
lc_time = 'en_US.utf8'
default_text_search_config = 'pg_catalog.english'
'@

$primaryConf | Out-File -FilePath "docker/config/postgresql-primary.conf" -Encoding UTF8 -NoNewline
Write-Host "   ✅ postgresql-primary.conf created" -ForegroundColor Green

# ============================================
# FILE 4: postgresql-replica.conf
# ============================================
Write-Host "📄 Creating docker/config/postgresql-replica.conf..." -ForegroundColor Yellow

$replicaConf = @'
# Connection Settings
listen_addresses = '*'
port = 5432
max_connections = 200

# Memory Settings
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 4MB

# Hot Standby Settings
hot_standby = on
hot_standby_feedback = on
max_standby_streaming_delay = 30s
wal_receiver_status_interval = 10s
wal_retrieve_retry_interval = 5s

# Primary Connection
primary_conninfo = 'host=postgres-primary port=5432 user=replicator password=replicator123 application_name=replica1'
primary_slot_name = 'replica_slot'

# Recovery Settings
restore_command = 'cp /var/lib/postgresql/wal_archive/%f %p'
recovery_target_timeline = 'latest'

# Logging
logging_collector = on
log_directory = 'log'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
log_line_prefix = '%m [%p] %q%u@%d '
log_timezone = 'UTC'

# Monitoring
shared_preload_libraries = 'pg_stat_statements'
track_activities = on
track_counts = on

# Locale
datestyle = 'iso, mdy'
timezone = 'UTC'
lc_messages = 'en_US.utf8'
default_text_search_config = 'pg_catalog.english'
'@

$replicaConf | Out-File -FilePath "docker/config/postgresql-replica.conf" -Encoding UTF8 -NoNewline
Write-Host "   ✅ postgresql-replica.conf created" -ForegroundColor Green

# ============================================
# FILE 5: pg_hba.conf
# ============================================
Write-Host "📄 Creating docker/config/pg_hba.conf..." -ForegroundColor Yellow

$pgHba = @'
# TYPE  DATABASE        USER            ADDRESS                 METHOD
local   all             all                                     trust
host    all             all             127.0.0.1/32            scram-sha-256
host    all             all             0.0.0.0/0               scram-sha-256
host    all             all             ::1/128                 scram-sha-256
host    replication     replicator      postgres-replica/32     scram-sha-256
host    replication     replicator      0.0.0.0/0               scram-sha-256
host    all             barman          barman/32               scram-sha-256
host    all             barman          0.0.0.0/0               scram-sha-256
host    all             all             172.16.0.0/12           scram-sha-256
host    replication     replicator      172.16.0.0/12           scram-sha-256
host    replication     all             172.16.0.0/12           scram-sha-256
'@

$pgHba | Out-File -FilePath "docker/config/pg_hba.conf" -Encoding UTF8 -NoNewline
Write-Host "   ✅ pg_hba.conf created" -ForegroundColor Green

# ============================================
# FILE 6: haproxy.cfg
# ============================================
Write-Host "📄 Creating docker/config/haproxy.cfg..." -ForegroundColor Yellow

$haproxy = @'
global
    maxconn 4096
    log stdout format raw local0 info
    user haproxy
    group haproxy

defaults
    log     global
    mode    tcp
    option  tcplog
    option  dontlognull
    retries 3
    timeout connect 5000ms
    timeout client  50000ms
    timeout server  50000ms

frontend postgres_primary_frontend
    bind *:5000
    mode tcp
    default_backend postgres_primary_backend

backend postgres_primary_backend
    mode tcp
    option httpchk
    http-check expect status 200
    default-server inter 3s fall 3 rise 2
    server primary postgres-primary:5432 check port 5432

frontend postgres_replica_frontend
    bind *:5001
    mode tcp
    default_backend postgres_replica_backend

backend postgres_replica_backend
    mode tcp
    option httpchk
    http-check expect status 200
    default-server inter 3s fall 3 rise 2
    server replica postgres-replica:5432 check port 5432

listen stats
    bind *:7000
    mode http
    stats enable
    stats uri /stats
    stats refresh 30s
    stats show-legends
    stats show-node
    stats admin if TRUE
'@

$haproxy | Out-File -FilePath "docker/config/haproxy.cfg" -Encoding UTF8 -NoNewline
Write-Host "   ✅ haproxy.cfg created" -ForegroundColor Green

# ============================================
# FILE 7: init-primary.sh
# ============================================
Write-Host "📄 Creating docker/scripts/init-primary.sh..." -ForegroundColor Yellow

$initPrimary = @'
#!/bin/bash
set -e

echo "🚀 Initializing PostgreSQL Primary Node..."

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER $POSTGRES_REPLICATION_USER WITH REPLICATION ENCRYPTED PASSWORD '$POSTGRES_REPLICATION_PASSWORD';
    GRANT ALL PRIVILEGES ON DATABASE $POSTGRES_DB TO $POSTGRES_REPLICATION_USER;
    SELECT pg_create_physical_replication_slot('replica_slot');
    CREATE USER barman WITH SUPERUSER ENCRYPTED PASSWORD 'barman123';
    SELECT * FROM pg_stat_replication;
EOSQL

echo "✅ Primary node initialized successfully!"
'@

$initPrimary | Out-File -FilePath "docker/scripts/init-primary.sh" -Encoding UTF8 -NoNewline
Write-Host "   ✅ init-primary.sh created" -ForegroundColor Green

# ============================================
# FILE 8: init-replica.sh
# ============================================
Write-Host "📄 Creating docker/scripts/init-replica.sh..." -ForegroundColor Yellow

$initReplica = @'
#!/bin/bash
set -e

echo "🚀 Initializing PostgreSQL Replica Node..."

until pg_isready -h $PRIMARY_HOST -U postgres; do
  echo "⏳ Waiting for primary node to be ready..."
  sleep 2
done

echo "✅ Primary node is ready!"

pg_ctl -D "$PGDATA" stop || true
rm -rf "$PGDATA"/*

echo "📦 Creating base backup from primary..."
PGPASSWORD=$REPLICATION_PASSWORD pg_basebackup \
    -h $PRIMARY_HOST \
    -U $REPLICATION_USER \
    -D "$PGDATA" \
    -P \
    -Xs \
    -R

touch "$PGDATA/standby.signal"
chmod 700 "$PGDATA"
chown -R postgres:postgres "$PGDATA"

echo "✅ Replica initialized successfully!"
'@

$initReplica | Out-File -FilePath "docker/scripts/init-replica.sh" -Encoding UTF8 -NoNewline
Write-Host "   ✅ init-replica.sh created" -ForegroundColor Green

# ============================================
# FILE 9: init-barman.sh
# ============================================
Write-Host "📄 Creating docker/scripts/init-barman.sh..." -ForegroundColor Yellow

$initBarman = @'
#!/bin/bash
set -e

echo "🚀 Initializing Barman Backup Manager..."

apt-get update -qq
apt-get install -y -qq barman barman-cli postgresql-client-16 > /dev/null 2>&1

mkdir -p /var/lib/barman/{incoming,errors,streaming}
mkdir -p /var/log/barman
mkdir -p /etc/barman.d

cat > /etc/barman.conf <<EOF
[barman]
barman_user = barman
configuration_files_directory = /etc/barman.d
barman_home = /var/lib/barman
log_file = /var/log/barman/barman.log
log_level = INFO
compression = gzip
minimum_redundancy = 1
retention_policy = RECOVERY WINDOW OF 7 DAYS
EOF

cat > /etc/barman.d/primary.conf <<EOF
[primary]
description = "BahyWay PostgreSQL Primary"
conninfo = host=postgres-primary port=5432 user=barman dbname=alarminsight password=barman123
streaming_conninfo = host=postgres-primary port=5432 user=barman dbname=alarminsight password=barman123
backup_method = postgres
streaming_archiver = on
slot_name = barman_slot
path_prefix = "/usr/lib/postgresql/16/bin"
EOF

export PGPASSWORD="barman123"
psql -h postgres-primary -U barman -d alarminsight -c "SELECT pg_create_physical_replication_slot('barman_slot');" || echo "Slot already exists"

echo "✅ Barman initialized successfully!"
'@

$initBarman | Out-File -FilePath "docker/scripts/init-barman.sh" -Encoding UTF8 -NoNewline
Write-Host "   ✅ init-barman.sh created" -ForegroundColor Green

# ============================================
# FILE 10: verify-replication.sh
# ============================================
Write-Host "📄 Creating docker/scripts/verify-replication.sh..." -ForegroundColor Yellow

$verifyReplication = @'
#!/bin/bash
set -e

echo "🔍 REPLICATION STATUS CHECK"
echo "=============================================="

if docker ps --format '{{.Names}}' | grep -q "^bahyway-postgres-primary$"; then
    echo ""
    echo "✅ Primary container is running"
    docker exec bahyway-postgres-primary psql -U postgres -c "SELECT client_addr, state, sync_state, replay_lag FROM pg_stat_replication;"
else
    echo "❌ Primary container is not running"
fi

if docker ps --format '{{.Names}}' | grep -q "^bahyway-postgres-replica$"; then
    echo ""
    echo "✅ Replica container is running"
    docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"
else
    echo "❌ Replica container is not running"
fi

echo ""
echo "✅ Verification complete"
'@

$verifyReplication | Out-File -FilePath "docker/scripts/verify-replication.sh" -Encoding UTF8 -NoNewline
Write-Host "   ✅ verify-replication.sh created" -ForegroundColor Green

# ============================================
# FILE 11: test-failover.sh
# ============================================
Write-Host "📄 Creating docker/scripts/test-failover.sh..." -ForegroundColor Yellow

$testFailover = @'
#!/bin/bash
set -e

echo "🔥 FAILOVER TEST"
echo "=============================================="

echo ""
echo "Step 1: Checking current status..."
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_is_in_recovery();" || echo "Primary is down"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

echo ""
echo "Step 2: Stopping primary (simulate failure)..."
docker stop bahyway-postgres-primary
sleep 3

echo ""
echo "Step 3: Promoting replica to primary..."
docker exec bahyway-postgres-replica pg_ctl promote -D /var/lib/postgresql/data/pgdata
sleep 5

echo ""
echo "Step 4: Verifying promotion..."
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

echo ""
echo "✅ Failover test complete!"
'@

$testFailover | Out-File -FilePath "docker/scripts/test-failover.sh" -Encoding UTF8 -NoNewline
Write-Host "   ✅ test-failover.sh created" -ForegroundColor Green

# ============================================
# SUCCESS MESSAGE
# ============================================
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ ALL 11 FILES CREATED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📂 Files created:" -ForegroundColor Cyan
Write-Host "   1. docker/docker-compose.yml"
Write-Host "   2. docker/.env"
Write-Host "   3. docker/config/postgresql-primary.conf"
Write-Host "   4. docker/config/postgresql-replica.conf"
Write-Host "   5. docker/config/pg_hba.conf"
Write-Host "   6. docker/config/haproxy.cfg"
Write-Host "   7. docker/scripts/init-primary.sh"
Write-Host "   8. docker/scripts/init-replica.sh"
Write-Host "   9. docker/scripts/init-barman.sh"
Write-Host "  10. docker/scripts/verify-replication.sh"
Write-Host "  11. docker/scripts/test-failover.sh"
Write-Host ""
Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. cd docker"
Write-Host "   2. docker-compose pull"
Write-Host "   3. docker-compose up -d"
Write-Host "   4. Wait 30 seconds"
Write-Host "   5. docker exec bahyway-postgres-primary psql -U postgres -c 'SELECT * FROM pg_stat_replication;'"
Write-Host ""
Write-Host "🎉 Your PostgreSQL HA cluster is ready to deploy!" -ForegroundColor Green
Write-Host ""