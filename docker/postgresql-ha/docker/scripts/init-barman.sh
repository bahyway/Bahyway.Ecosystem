#!/bin/bash
set -e

echo "ðŸš€ Initializing Barman Backup Manager..."

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

echo "âœ… Barman initialized successfully!"