#!/bin/bash
set -e

echo "ðŸš€ Initializing PostgreSQL Primary Node..."

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER $POSTGRES_REPLICATION_USER WITH REPLICATION ENCRYPTED PASSWORD '$POSTGRES_REPLICATION_PASSWORD';
    GRANT ALL PRIVILEGES ON DATABASE $POSTGRES_DB TO $POSTGRES_REPLICATION_USER;
    SELECT pg_create_physical_replication_slot('replica_slot');
    CREATE USER barman WITH SUPERUSER ENCRYPTED PASSWORD 'barman123';
    SELECT * FROM pg_stat_replication;
EOSQL

echo "âœ… Primary node initialized successfully!"