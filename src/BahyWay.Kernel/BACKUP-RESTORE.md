# 💾 BahyWay PostgreSQL HA - Backup & Restore Guide

Complete guide to backup strategies and disaster recovery using Barman.

---

## 🎯 Backup Strategy Overview

### Backup Types

| Type | Frequency | Retention | Purpose |
|------|-----------|-----------|---------|
| **Full Backup** | Daily at 2 AM | 7 days | Complete database snapshot |
| **WAL Archive** | Continuous | 7 days | Point-in-Time Recovery (PITR) |
| **Pre-Migration** | Before each migration | 30 days | Rollback safety |
| **Weekly Archive** | Sunday 2 AM | 4 weeks | Long-term storage |

### 3-2-1 Backup Rule

✅ **3** copies of data
✅ **2** different storage types
✅ **1** off-site backup

---

## 📦 Barman Backup Manager

### Architecture
┌─────────────┐
│ Primary │
│ PostgreSQL │
└──────┬──────┘
│ WAL Streaming
│ Base Backups
▼
┌─────────────┐
│ Barman │
│ Server │
├─────────────┤
│ • Backups │
│ • WAL Files │
│ • Metadata │
└─────────────┘

---

## 🔧 Barman Setup Verification

### Check Barman Status

```bash
# Check Barman is configured {#check-barman-is-configured  data-source-line="3376"}
docker exec bahyway-barman barman check primary

# Expected output: {#expected-output-1  data-source-line="3379"}
# ✅ PostgreSQL: OK {#-postgresql-ok  data-source-line="3380"}
# ✅ is_superuser: OK {#-is_superuser-ok  data-source-line="3381"}
# ✅ WAL archive: OK {#-wal-archive-ok  data-source-line="3382"}
# ✅ backup: OK {#-backup-ok  data-source-line="3383"}
``` {data-source-line="3384"}

### View Barman Configuration

```bash
# Show configuration {#show-configuration  data-source-line="3389"}
docker exec bahyway-barman barman show-server primary
``` {data-source-line="3391"}

---

## 💾 Creating Backups

### Manual Full Backup

```bash
# Create immediate backup {#create-immediate-backup  data-source-line="3400"}
docker exec bahyway-barman barman backup primary

# Create backup with description {#create-backup-with-description  data-source-line="3403"}
docker exec bahyway-barman barman backup --name="pre-migration-$(date +%Y%m%d)" primary
``` {data-source-line="3405"}

### List All Backups

```bash
# List backups {#list-backups-1  data-source-line="3410"}
docker exec bahyway-barman barman list-backup primary

# Example output: {#example-output  data-source-line="3413"}
# primary 20250120T020000 - Sun Jan 20 02:00:00 2025 - Size: 245.3 MiB - WAL Size: 112.5 MiB {#primary-20250120t020000---sun-jan-20-020000-2025---size-2453-mib---wal-size-1125-mib  data-source-line="3414"}
``` {data-source-line="3415"}

### Show Backup Details

```bash
# Show specific backup info {#show-specific-backup-info  data-source-line="3420"}
docker exec bahyway-barman barman show-backup primary <backup-id>

# Show latest backup {#show-latest-backup  data-source-line="3423"}
docker exec bahyway-barman barman show-backup primary latest
``` {data-source-line="3425"}

---

## 🔄 Restore Operations

### Full Restore to New Location

```bash
# Step 1: Create restore directory {#step-1-create-restore-directory  data-source-line="3434"}
docker exec bahyway-barman mkdir -p /restore/alarminsight

# Step 2: Restore backup {#step-2-restore-backup  data-source-line="3437"}
docker exec bahyway-barman barman recover primary latest /restore/alarminsight

# Step 3: Verify restored files {#step-3-verify-restored-files  data-source-line="3440"}
docker exec bahyway-barman ls -lh /restore/alarminsight/
``` {data-source-line="3442"}

### Point-in-Time Recovery (PITR)

```bash
# Restore to specific timestamp {#restore-to-specific-timestamp  data-source-line="3447"}
docker exec bahyway-barman barman recover primary latest /restore/pitr \
  --target-time="2025-01-20 14:30:00" \
  --target-action=promote

# Restore to specific transaction ID {#restore-to-specific-transaction-id  data-source-line="3452"}
docker exec bahyway-barman barman recover primary latest /restore/pitr \
  --target-xid=12345678 \
  --target-action=promote
``` {data-source-line="3456"}

### Restore to Running Cluster

**⚠️ DANGER: This replaces your current data!**

```bash
# Step 1: STOP ALL APPLICATIONS {#step-1-stop-all-applications  data-source-line="3463"}
# Step 2: Stop PostgreSQL {#step-2-stop-postgresql  data-source-line="3464"}
docker stop bahyway-postgres-primary

# Step 3: Backup current data (just in case) {#step-3-backup-current-data-just-in-case  data-source-line="3467"}
docker run --rm -v bahyway-postgres-primary-data:/data -v $(pwd):/backup \
  alpine tar czf /backup/emergency-backup-$(date +%Y%m%d_%H%M%S).tar.gz /data

# Step 4: Clear current data {#step-4-clear-current-data  data-source-line="3471"}
docker volume rm bahyway-postgres-primary-data
docker volume create bahyway-postgres-primary-data

# Step 5: Restore from Barman {#step-5-restore-from-barman  data-source-line="3475"}
docker exec bahyway-barman barman recover primary latest /restore/production
docker cp $(docker inspect -f '{{.Mounts}}' bahyway-barman | grep /restore/production):/restore/production/* \
  /var/lib/docker/volumes/bahyway-postgres-primary-data/_data/

# Step 6: Start PostgreSQL {#step-6-start-postgresql  data-source-line="3480"}
docker start bahyway-postgres-primary

# Step 7: Verify {#step-7-verify  data-source-line="3483"}
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_postmaster_start_time();"
``` {data-source-line="3485"}

---

## 🧪 Testing Backups

### Test Restore Procedure (Safe)

```bash
# Create test restore {#create-test-restore  data-source-line="3494"}
docker exec bahyway-barman barman recover primary latest /tmp/test-restore

# Verify files {#verify-files  data-source-line="3497"}
docker exec bahyway-barman ls -lh /tmp/test-restore/

# Check for required files {#check-for-required-files  data-source-line="3500"}
docker exec bahyway-barman bash -c "
  [ -f /tmp/test-restore/PG_VERSION ] && echo '✅ PG_VERSION exists' || echo '❌ Missing PG_VERSION'
  [ -f /tmp/test-restore/postgresql.conf ] && echo '✅ Config exists' || echo '❌ Missing config'
  [ -d /tmp/test-restore/base ] && echo '✅ Data dir exists' || echo '❌ Missing data dir'
"

# Cleanup {#cleanup  data-source-line="3507"}
docker exec bahyway-barman rm -rf /tmp/test-restore
``` {data-source-line="3509"}

### Monthly Restore Drill

**Run this monthly to verify backups:**

```bash
#!/bin/bash
# monthly-restore-drill.sh {#monthly-restore-drillsh  data-source-line="3517"}

echo "🧪 Starting Monthly Restore Drill"
echo "================================="

# 1. List recent backups {#1-list-recent-backups  data-source-line="3522"}
echo "📦 Recent backups:"
docker exec bahyway-barman barman list-backup primary | tail -3

# 2. Select latest backup {#2-select-latest-backup  data-source-line="3526"}
BACKUP_ID=$(docker exec bahyway-barman barman list-backup primary | head -2 | tail -1 | awk '{print $2}')

# 3. Restore to temp location {#3-restore-to-temp-location  data-source-line="3529"}
echo "🔄 Restoring backup: $BACKUP_ID"
docker exec bahyway-barman barman recover primary $BACKUP_ID /tmp/restore-drill-$(date +%Y%m%d)

# 4. Verify restore {#4-verify-restore  data-source-line="3533"}
if [ $? -eq 0 ]; then
  echo "✅ Restore drill PASSED"
else
  echo "❌ Restore drill FAILED - CHECK BACKUPS!"
  exit 1
fi

# 5. Cleanup {#5-cleanup  data-source-line="3541"}
docker exec bahyway-barman rm -rf /tmp/restore-drill-*

echo "================================="
echo "✅ Monthly Restore Drill Complete"
``` {data-source-line="3546"}

---

## 📊 Backup Monitoring

### Check Backup Status

```bash
# Check last backup time {#check-last-backup-time  data-source-line="3555"}
docker exec bahyway-barman barman list-backup primary | head -2

# Check backup size {#check-backup-size  data-source-line="3558"}
docker exec bahyway-barman du -sh /var/lib/barman/primary/base/*

# Check WAL archive size {#check-wal-archive-size  data-source-line="3561"}
docker exec bahyway-barman du -sh /var/lib/barman/primary/wals
``` {data-source-line="3563"}

### Backup Health Checks

```bash
# Run all checks {#run-all-checks  data-source-line="3568"}
docker exec bahyway-barman barman check primary

# Check specific component {#check-specific-component  data-source-line="3571"}
docker exec bahyway-barman barman check primary --nagios
``` {data-source-line="3573"}

---

## 🔄 Backup Retention Policy

### Current Policy

```bash
retention_policy = RECOVERY WINDOW OF 7 DAYS
``` {data-source-line="3583"}

**What this means:**
- Backups older than 7 days are automatically deleted
- Maintains backups for the past week
- WAL files required for PITR within window are kept

### Change Retention Policy

```bash
# Edit Barman config {#edit-barman-config  data-source-line="3593"}
docker exec bahyway-barman bash -c "
cat >> /etc/barman.d/primary.conf <<EOF
retention_policy = RECOVERY WINDOW OF 30 DAYS
EOF
"

# Reload configuration {#reload-configuration  data-source-line="3600"}
docker exec bahyway-barman barman cron
``` {data-source-line="3602"}

### Manual Cleanup

```bash
# Show obsolete backups {#show-obsolete-backups  data-source-line="3607"}
docker exec bahyway-barman barman list-backup primary --obsolete

# Delete obsolete backups {#delete-obsolete-backups  data-source-line="3610"}
docker exec bahyway-barman barman delete primary <backup-id>

# Delete all obsolete {#delete-all-obsolete  data-source-line="3613"}
docker exec bahyway-barman barman delete primary oldest
``` {data-source-line="3615"}

---

## 💡 Backup Best Practices

### 1. Regular Schedule

```bash
# Cron job already configured (runs at 2 AM daily) {#cron-job-already-configured-runs-at-2-am-daily  data-source-line="3624"}
docker exec bahyway-barman crontab -l

# Manually trigger if needed {#manually-trigger-if-needed  data-source-line="3627"}
docker exec bahyway-barman barman cron
``` {data-source-line="3629"}

### 2. Pre-Migration Backups

**Always backup before migrations:**

```bash
# Before running migrations {#before-running-migrations  data-source-line="3636"}
docker exec bahyway-barman barman backup --name="pre-migration-alarminsight-$(date +%Y%m%d_%H%M%S)" primary
``` {data-source-line="3638"}

### 3. Verify Backups

```bash
# Test restore monthly {#test-restore-monthly  data-source-line="3643"}
./monthly-restore-drill.sh

# Verify backup integrity {#verify-backup-integrity  data-source-line="3646"}
docker exec bahyway-barman barman check primary
``` {data-source-line="3648"}

### 4. Off-Site Backups

```bash
# Export backup to external storage {#export-backup-to-external-storage  data-source-line="3653"}
docker cp bahyway-barman:/var/lib/barman/primary/base/<backup-id> \
  /mnt/external-backup/

# Or rsync to remote server {#or-rsync-to-remote-server  data-source-line="3657"}
docker exec bahyway-barman rsync -avz /var/lib/barman/primary/ \
  remote-server:/backups/bahyway/
``` {data-source-line="3660"}

---

## 🚨 Disaster Recovery Scenarios

### Scenario 1: Accidental Table Drop

```bash
# Find backup before the drop {#find-backup-before-the-drop  data-source-line="3669"}
docker exec bahyway-barman barman list-backup primary

# Restore to point before drop {#restore-to-point-before-drop  data-source-line="3672"}
docker exec bahyway-barman barman recover primary <backup-id> /restore/recovery \
  --target-time="2025-01-20 14:00:00"

# Extract just the table data {#extract-just-the-table-data  data-source-line="3676"}
# (requires manual SQL export/import) {#requires-manual-sql-exportimport  data-source-line="3677"}
``` {data-source-line="3678"}

### Scenario 2: Database Corruption

```bash
# Stop corrupted database {#stop-corrupted-database  data-source-line="3683"}
docker stop bahyway-postgres-primary

# Restore from latest good backup {#restore-from-latest-good-backup  data-source-line="3686"}
docker exec bahyway-barman barman recover primary latest /restore/production

# Replace corrupted data {#replace-corrupted-data  data-source-line="3689"}
# Start database {#start-database  data-source-line="3690"}
``` {data-source-line="3691"}

### Scenario 3: Complete Cluster Loss

```bash
# 1. Rebuild infrastructure {#1-rebuild-infrastructure  data-source-line="3696"}
cd infrastructure/postgresql-ha/ansible
ansible-playbook playbooks/01-setup-cluster.yml

# 2. Restore from Barman {#2-restore-from-barman  data-source-line="3700"}
docker exec bahyway-barman barman list-backup primary
docker exec bahyway-barman barman recover primary latest /restore/disaster

# 3. Copy to new primary {#3-copy-to-new-primary  data-source-line="3704"}
# 4. Rebuild replica from new primary {#4-rebuild-replica-from-new-primary  data-source-line="3705"}
``` {data-source-line="3706"}

---

## 📈 Backup Performance

### Monitor Backup Duration

```bash
# Check last backup duration {#check-last-backup-duration  data-source-line="3715"}
docker exec bahyway-barman barman show-backup primary latest | grep "Time"
``` {data-source-line="3717"}

### Optimize Backup Performance

```bash
# Use compression {#use-compression  data-source-line="3722"}
compression = gzip

# Parallel WAL transfer {#parallel-wal-transfer  data-source-line="3725"}
parallel_jobs = 4

# Incremental backups {#incremental-backups  data-source-line="3728"}
backup_method = postgres
``` {data-source-line="3730"}

---

## 🔗 Related Documentation

- [SETUP.md](./SETUP.md) - Initial cluster setup
- [FAILOVER.md](./FAILOVER.md) - Failover procedures
- [Barman Documentation](https://pgbarman.org/documentation/)

---

**Test your backups regularly - Untested backups are useless backups!** 🚀