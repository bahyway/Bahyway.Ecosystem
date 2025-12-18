# 🔥 BahyWay PostgreSQL HA - Failover Guide

Complete guide to handling PostgreSQL failover scenarios.

---

## 🎯 Failover Overview

**What is Failover?**

Failover is the process of automatically switching from a failed primary database to a healthy replica, ensuring zero downtime for your applications.

### Failover Scenarios

1. **Planned Maintenance** - Scheduled downtime for upgrades
2. **Hardware Failure** - Physical server crash
3. **Network Partition** - Loss of network connectivity
4. **Database Corruption** - Data integrity issues

---

## 🔄 Automatic Failover (Future: with Patroni)

**Note:** Current setup supports **manual failover**. For automatic failover, see Patroni integration guide.

---

## 🛠️ Manual Failover Process

### Scenario: Primary Node Failure

#### Step 1: Detect Failure

```bash
# Check primary status {#check-primary-status  data-source-line="3042"}
docker exec bahyway-postgres-primary pg_isready -U postgres

# If returns error: Primary is down {#if-returns-error-primary-is-down  data-source-line="3045"}
``` {data-source-line="3046"}

#### Step 2: Verify Replica Health

```bash
# Check replica status {#check-replica-status  data-source-line="3051"}
docker exec bahyway-postgres-replica pg_isready -U postgres

# Check replication lag (should be minimal) {#check-replication-lag-should-be-minimal  data-source-line="3054"}
docker exec bahyway-postgres-replica psql -U postgres -c "
  SELECT NOW() - pg_last_xact_replay_timestamp() AS replication_lag;
"
``` {data-source-line="3058"}

#### Step 3: Promote Replica to Primary

```bash
# Promote replica {#promote-replica  data-source-line="3063"}
docker exec bahyway-postgres-replica pg_ctl promote -D /var/lib/postgresql/data/pgdata

# Wait 5-10 seconds for promotion {#wait-5-10-seconds-for-promotion  data-source-line="3066"}
sleep 10

# Verify it's now a primary (should return 'f') {#verify-its-now-a-primary-should-return-f  data-source-line="3069"}
docker exec bahyway-postgres-replica psql -U postgres -t -c "SELECT pg_is_in_recovery();"
``` {data-source-line="3071"}

#### Step 4: Update Application Connection Strings

**Option A: Update appsettings.json**

```bash
# Update AlarmInsight API {#update-alarminsight-api  data-source-line="3078"}
cd src/AlarmInsight.API
# Change port from 5432 to 5433 {#change-port-from-5432-to-5433  data-source-line="3080"}
# Or use HAProxy (port 5000) which handles this automatically {#or-use-haproxy-port-5000-which-handles-this-automatically  data-source-line="3081"}
``` {data-source-line="3082"}

**Option B: Use HAProxy (Automatic)**

If using HAProxy (port 5000), it will automatically route to the new primary. No changes needed!

#### Step 5: Verify Application Connectivity

```bash
# Test write operation {#test-write-operation  data-source-line="3091"}
docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -c "
  CREATE TABLE failover_test (id SERIAL, test_time TIMESTAMP DEFAULT NOW());
  INSERT INTO failover_test DEFAULT VALUES;
  SELECT * FROM failover_test;
"
``` {data-source-line="3097"}

#### Step 6: Handle Old Primary

**When old primary comes back online:**

```bash
# Option A: Rejoin as new replica (manual reconfiguration needed) {#option-a-rejoin-as-new-replica-manual-reconfiguration-needed  data-source-line="3104"}
# Option B: Rebuild from scratch using pg_basebackup {#option-b-rebuild-from-scratch-using-pg_basebackup  data-source-line="3105"}
# Option C: Keep offline and restore from Barman backup later {#option-c-keep-offline-and-restore-from-barman-backup-later  data-source-line="3106"}
``` {data-source-line="3107"}

---

## 🧪 Automated Failover Test

Run the complete failover test:

```bash
cd infrastructure/postgresql-ha/docker/scripts
./test-failover.sh
``` {data-source-line="3118"}

**What it does:**

1. ✅ Checks current cluster status
2. ✅ Verifies replication lag
3. ✅ Stops primary (simulates crash)
4. ✅ Promotes replica to primary
5. ✅ Tests write operations on new primary
6. ✅ Offers to restart old primary as new replica

---

## 📊 Monitoring Failover

### Key Metrics to Monitor

```bash
# Replication lag (should be < 10s) {#replication-lag-should-be--10s  data-source-line="3136"}
docker exec bahyway-postgres-primary psql -U postgres -c "
  SELECT
    application_name,
    COALESCE(replay_lag, '0'::interval) AS lag
  FROM pg_stat_replication;
"

# WAL position difference {#wal-position-difference  data-source-line="3144"}
docker exec bahyway-postgres-primary psql -U postgres -c "
  SELECT
    pg_size_pretty(pg_wal_lsn_diff(pg_current_wal_lsn(), replay_lsn)) AS lag_bytes
  FROM pg_stat_replication;
"

# Last transaction replay time {#last-transaction-replay-time  data-source-line="3151"}
docker exec bahyway-postgres-replica psql -U postgres -c "
  SELECT pg_last_xact_replay_timestamp() AS last_replay;
"
``` {data-source-line="3155"}

---

## ⚠️ Failover Checklist

### Before Failover

- [ ] Verify replica is healthy and in sync
- [ ] Check replication lag < 10 seconds
- [ ] Backup current state with Barman
- [ ] Notify team of planned failover
- [ ] Prepare rollback plan

### During Failover

- [ ] Stop applications (if planned)
- [ ] Promote replica to primary
- [ ] Verify promotion successful
- [ ] Update connection strings (if needed)
- [ ] Test write operations

### After Failover

- [ ] Monitor new primary performance
- [ ] Verify application connectivity
- [ ] Create post-failover backup
- [ ] Update documentation
- [ ] Plan for old primary recovery

---

## 🔄 Rebuilding Old Primary as New Replica

After failover, rebuild the old primary:

```bash
# Step 1: Stop old primary {#step-1-stop-old-primary  data-source-line="3192"}
docker stop bahyway-postgres-primary

# Step 2: Clear old data {#step-2-clear-old-data  data-source-line="3195"}
docker exec bahyway-postgres-primary rm -rf /var/lib/postgresql/data/pgdata/*

# Step 3: Create base backup from NEW primary (old replica) {#step-3-create-base-backup-from-new-primary-old-replica  data-source-line="3198"}
docker exec bahyway-postgres-primary bash -c "
  PGPASSWORD=replicator123 pg_basebackup \
    -h postgres-replica \
    -U replicator \
    -D /var/lib/postgresql/data/pgdata \
    -P -R
"

# Step 4: Create standby.signal {#step-4-create-standbysignal  data-source-line="3207"}
docker exec bahyway-postgres-primary touch /var/lib/postgresql/data/pgdata/standby.signal

# Step 5: Start as new replica {#step-5-start-as-new-replica  data-source-line="3210"}
docker start bahyway-postgres-primary

# Step 6: Verify replication {#step-6-verify-replication  data-source-line="3213"}
./scripts/verify-replication.sh
``` {data-source-line="3215"}

---

## 🚨 Troubleshooting Failover

### Issue: Replica Won't Promote

**Symptoms:** `pg_ctl promote` returns error

**Solution:**
```bash
# Check if standby.signal exists {#check-if-standbysignal-exists  data-source-line="3227"}
docker exec bahyway-postgres-replica ls /var/lib/postgresql/data/pgdata/standby.signal

# Check recovery status {#check-recovery-status  data-source-line="3230"}
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

# Force promotion {#force-promotion  data-source-line="3233"}
docker exec bahyway-postgres-replica pg_ctl promote -D /var/lib/postgresql/data/pgdata -w
``` {data-source-line="3235"}

### Issue: Split-Brain Scenario

**Symptoms:** Both primary and replica think they're primary

**Solution:**
```bash
# DANGER: Only keep ONE as primary! {#danger-only-keep-one-as-primary  data-source-line="3243"}

# Check both nodes {#check-both-nodes  data-source-line="3245"}
docker exec bahyway-postgres-primary psql -U postgres -t -c "SELECT pg_is_in_recovery();"
docker exec bahyway-postgres-replica psql -U postgres -t -c "SELECT pg_is_in_recovery();"

# Choose which to keep as primary (usually newest) {#choose-which-to-keep-as-primary-usually-newest  data-source-line="3249"}
# Demote the other by recreating it from base backup {#demote-the-other-by-recreating-it-from-base-backup  data-source-line="3250"}
``` {data-source-line="3251"}

### Issue: High Replication Lag After Failover

**Symptoms:** New replica has high lag

**Solution:**
```bash
# Check network connectivity {#check-network-connectivity-1  data-source-line="3259"}
docker network inspect bahyway-network

# Check WAL sender processes {#check-wal-sender-processes  data-source-line="3262"}
docker exec <new-primary> psql -U postgres -c "SELECT * FROM pg_stat_replication;"

# Increase wal_keep_size if needed {#increase-wal_keep_size-if-needed  data-source-line="3265"}
docker exec <new-primary> psql -U postgres -c "ALTER SYSTEM SET wal_keep_size = '2GB';"
docker exec <new-primary> psql -U postgres -c "SELECT pg_reload_conf();"
``` {data-source-line="3268"}

---

## 📈 Failover Performance Metrics

### Target Metrics

- **RTO (Recovery Time Objective):** < 60 seconds
- **RPO (Recovery Point Objective):** < 10 seconds of data loss
- **Failover Detection Time:** < 30 seconds
- **Promotion Time:** < 30 seconds

### Measuring Actual Performance

```bash
# Run failover test and measure {#run-failover-test-and-measure  data-source-line="3284"}
time ./scripts/test-failover.sh

# Target: Complete failover in < 60 seconds {#target-complete-failover-in--60-seconds  data-source-line="3287"}
``` {data-source-line="3288"}

---

## 🎓 Best Practices

1. **Regular Testing** - Test failover monthly
2. **Monitor Replication Lag** - Keep < 10 seconds
3. **Automate Monitoring** - Set up alerts for lag > 30s
4. **Document Procedures** - Keep runbooks updated
5. **Practice Recovery** - Train team on failover procedures
6. **Use HAProxy** - Automatic routing to healthy primary
7. **Backup Before Failover** - Always create backup first

---

## 🔗 Related Documentation

- [SETUP.md](./SETUP.md) - Initial cluster setup
- [BACKUP-RESTORE.md](./BACKUP-RESTORE.md) - Backup and recovery
- [PostgreSQL Replication Docs](https://www.postgresql.org/docs/16/warm-standby.html)

---

**Practice failover regularly to ensure zero downtime!** 🚀