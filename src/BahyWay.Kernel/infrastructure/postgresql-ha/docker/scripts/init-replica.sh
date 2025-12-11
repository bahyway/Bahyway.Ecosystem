#!/bin/bash
set -e

echo "ðŸš€ Initializing PostgreSQL Replica Node..."

until pg_isready -h $PRIMARY_HOST -U postgres; do
  echo "â³ Waiting for primary node to be ready..."
  sleep 2
done

echo "âœ… Primary node is ready!"

pg_ctl -D "$PGDATA" stop || true
rm -rf "$PGDATA"/*

echo "ðŸ“¦ Creating base backup from primary..."
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

echo "âœ… Replica initialized successfully!"