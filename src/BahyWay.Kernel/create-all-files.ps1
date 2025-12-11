# Navigate to postgresql-ha directory
$baseDir = "C:\Users\Bahaa\source\_OTAP\Bahyway_StillInDev\infrastructure\postgresql-ha"
Set-Location $baseDir

Write-Host "🚀 Creating PostgreSQL HA Infrastructure Files..." -ForegroundColor Cyan
Write-Host ""

# Create docker-compose.yml
Write-Host "Creating docker/docker-compose.yml..." -ForegroundColor Yellow
@'
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
'@ | Out-File -FilePath "docker/docker-compose.yml" -Encoding UTF8
Write-Host "✅ docker-compose.yml created" -ForegroundColor Green

# Create .env file
Write-Host "Creating docker/.env..." -ForegroundColor Yellow
@'
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
'@ | Out-File -FilePath "docker/.env" -Encoding UTF8
Write-Host "✅ .env created" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 Files created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "  1. I'll provide more files in next message"
Write-Host "  2. Create config and script files"
Write-Host "  3. Run: cd docker && docker-compose up -d"
Write-Host ""