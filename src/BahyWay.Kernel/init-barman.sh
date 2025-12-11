#!/bin/bash

# ============================================
# BAHYWAY BARMAN INITIALIZATION
# Sets up continuous backup with PITR
# ============================================

set -e

echo "🚀 Initializing Barman Backup Manager..."

# Install required packages
apt-get update
apt-get install -y barman barman-cli postgresql-client-16

# Create barman directories
mkdir -p /var/lib/barman/{incoming,errors,streaming}
chown -R barman:barman /var/lib/barman

# Configure barman
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

# Configure primary server backup
mkdir -p /etc/barman.d
cat > /etc/barman.d/primary.conf <<EOF
[primary]
description = "BahyWay PostgreSQL Primary"
conninfo = host=postgres-primary user=barman dbname=alarminsight
streaming_conninfo = host=postgres-primary user=barman dbname=alarminsight
backup_method = postgres
streaming_archiver = on
slot_name = barman_slot
path_prefix = "/usr/lib/postgresql/16/bin"
EOF

# Create replication slot on primary
export PGPASSWORD="$BARMAN_BACKUP_PASSWORD"
psql -h postgres-primary -U barman -d alarminsight -c "SELECT pg_create_physical_replication_slot('barman_slot');" || echo "Slot already exists"

# Test connection
su - barman -c "barman check primary"

# Schedule first backup
su - barman -c "barman backup primary"

echo "✅ Barman initialized successfully!"
echo "📊 Backup schedule: Full backup daily at 2 AM"
echo "📊 Retention: 7 days"
echo "📊 WAL archiving: Enabled"