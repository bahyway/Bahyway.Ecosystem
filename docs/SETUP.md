# 🚀 BahyWay PostgreSQL HA Cluster - Setup Guide

Complete guide to setting up and deploying the production-grade PostgreSQL HA cluster.

---

## 📋 Prerequisites

### Required Software
- ✅ **Docker** 20.10+ ([Install Docker](https://docs.docker.com/get-docker/))
- ✅ **Docker Compose** 2.0+ (included with Docker Desktop)
- ✅ **Ansible** 2.9+ ([Install Ansible](https://docs.ansible.com/ansible/latest/installation_guide/index.html))
- ✅ **.NET 8 SDK** ([Install .NET](https://dotnet.microsoft.com/download))
- ✅ **PowerShell** 7+ (Windows) or **Bash** (Linux/Mac)

### System Requirements
- **CPU:** 4+ cores recommended
- **RAM:** 8GB minimum, 16GB recommended
- **Disk:** 50GB+ free space
- **OS:** Windows 10/11, Ubuntu 20.04+, macOS 11+

---

## 🎯 Quick Start (5 Minutes)

### Option A: Automated Deployment

```bash
# Navigate to infrastructure directory {#navigate-to-infrastructure-directory  data-source-line="2726"}
cd infrastructure/postgresql-ha

# Run quick start script {#run-quick-start-script  data-source-line="2729"}
./quick-start.sh

# Wait for deployment to complete (~5 minutes) {#wait-for-deployment-to-complete-5-minutes  data-source-line="2732"}
# Access HAProxy stats: http://localhost:7000/stats {#access-haproxy-stats-httplocalhost7000stats  data-source-line="2733"}
``` {data-source-line="2734"}

### Option B: Manual Deployment with Ansible

```bash
# Step 1: Deploy cluster {#step-1-deploy-cluster  data-source-line="2739"}
cd infrastructure/postgresql-ha/ansible
ansible-playbook playbooks/01-setup-cluster.yml

# Step 2: Configure replication {#step-2-configure-replication  data-source-line="2743"}
ansible-playbook playbooks/02-configure-replication.yml

# Step 3: Setup Barman backups {#step-3-setup-barman-backups  data-source-line="2746"}
ansible-playbook playbooks/03-setup-barman.yml

# Step 4: Deploy AlarmInsight migrations {#step-4-deploy-alarminsight-migrations  data-source-line="2749"}
ansible-playbook playbooks/04-deploy-migrations.yml
``` {data-source-line="2751"}

---

## 📊 Architecture Overview
                               ┌─────────────────┐
                               │   HAProxy LB    │
                               │   :5000 (R/W)   │
                               │   :5001 (R/O)   │
                               └────────┬────────┘
                                        │
                    ┌───────────────────┴─────────────────────┐
                    │                                         │
               ┌────▼─────┐                              ┌─────▼────┐
               │ Primary  │ ───── Streaming ────────────▶│ Replica  │
               │   :5432  │       Replication            │   :5433  │
               └────┬─────┘                              └──────────┘
                    │
                    │ WAL Archive
                    │
               ┌────▼─────┐
               │  Barman  │
               │  Backup  │
               └──────────┘

### Components

| Component | Purpose | Port | Access |
|-----------|---------|------|--------|
| **PostgreSQL Primary** | Read-Write operations | 5432 | Direct |
| **PostgreSQL Replica** | Read-Only operations | 5433 | Direct |
| **HAProxy Primary** | Load balanced R/W | 5000 | Recommended |
| **HAProxy Replica** | Load balanced R/O | 5001 | Recommended |
| **HAProxy Stats** | Monitoring dashboard | 7000 | Web UI |
| **ETCD** | Cluster coordination | 2379 | Internal |
| **Barman** | Backup/Recovery | N/A | CLI |

---

## 🔧 Configuration

### Environment Variables

Edit `docker/.env`:

```bash
# Database credentials {#database-credentials  data-source-line="2800"}
POSTGRES_DB=alarminsight
POSTGRES_USER=postgres
POSTGRES_PASSWORD=YourStrongPassword123!

# Replication {#replication  data-source-line="2805"}
REPLICATION_USER=replicator
REPLICATION_PASSWORD=YourReplicatorPass456!

# Backup {#backup  data-source-line="2809"}
BARMAN_USER=barman
BARMAN_PASSWORD=YourBarmanPass789!
``` {data-source-line="2812"}

### Ansible Variables

Edit `ansible/vars/postgresql-config.yml`:

```yaml
postgres:
  database: "alarminsight"
  memory:
    shared_buffers: "256MB"  # Adjust based on RAM
    effective_cache_size: "1GB"
``` {data-source-line="2824"}

---

## ✅ Verification

### Check Cluster Status

```bash
# All containers running {#all-containers-running  data-source-line="2833"}
docker ps

# Verify replication {#verify-replication  data-source-line="2836"}
./docker/scripts/verify-replication.sh

# Check HAProxy stats {#check-haproxy-stats  data-source-line="2839"}
open http://localhost:7000/stats
``` {data-source-line="2841"}

### Expected Output
```
✅ Primary container is running
✅ Primary is in read-write mode
✅ Replica container is running
✅ Replica is in hot standby mode
✅ Replication lag: 0s
```
---

## 🔌 Connection Strings

### For Development

```csharp
// Primary (Read-Write) - Direct
"Host=localhost;Port=5432;Database=alarminsight;Username=postgres;Password=xxx"

// Replica (Read-Only) - Direct
"Host=localhost;Port=5433;Database=alarminsight;Username=postgres;Password=xxx"
``` {data-source-line="2865"}

### For Production (Recommended)

```csharp
// Primary (Read-Write) - via HAProxy
"Host=localhost;Port=5000;Database=alarminsight;Username=postgres;Password=xxx"

// Replica (Read-Only) - via HAProxy
"Host=localhost;Port=5001;Database=alarminsight;Username=postgres;Password=xxx"
``` {data-source-line="2875"}

### AlarmInsight API Configuration

Update `src/AlarmInsight.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AlarmInsight": "Host=localhost;Port=5000;Database=alarminsight;Username=postgres;Password=xxx"
  }
}
``` {data-source-line="2887"}

---

## 🔥 Test Failover

```bash
# Run automated failover test {#run-automated-failover-test  data-source-line="2894"}
./docker/scripts/test-failover.sh

# This will: {#this-will  data-source-line="2897"}
# 1. Stop primary node (simulate failure) {#1-stop-primary-node-simulate-failure  data-source-line="2898"}
# 2. Promote replica to primary {#2-promote-replica-to-primary  data-source-line="2899"}
# 3. Verify writes work on new primary {#3-verify-writes-work-on-new-primary  data-source-line="2900"}
``` {data-source-line="2901"}

---

## 📦 Backup & Restore

See [BACKUP-RESTORE.md](./BACKUP-RESTORE.md) for complete guide.

**Quick commands:**

```bash
# Create backup {#create-backup  data-source-line="2912"}
docker exec bahyway-barman barman backup primary

# List backups {#list-backups  data-source-line="2915"}
docker exec bahyway-barman barman list-backup primary

# Restore (see full guide) {#restore-see-full-guide  data-source-line="2918"}
docker exec bahyway-barman barman recover primary <backup-id> /restore/path
``` {data-source-line="2920"}

---

## 🐛 Troubleshooting

### Containers Not Starting

```bash
# Check logs {#check-logs  data-source-line="2929"}
docker-compose logs postgres-primary
docker-compose logs postgres-replica

# Restart cluster {#restart-cluster  data-source-line="2933"}
docker-compose down
docker-compose up -d
``` {data-source-line="2936"}

### Replication Not Working

```bash
# Check replication status {#check-replication-status  data-source-line="2941"}
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT * FROM pg_stat_replication;"

# Check replica logs {#check-replica-logs  data-source-line="2944"}
docker logs bahyway-postgres-replica
``` {data-source-line="2946"}

### High Replication Lag

```bash
# Check network connectivity {#check-network-connectivity  data-source-line="2951"}
docker exec bahyway-postgres-replica ping postgres-primary

# Check WAL generation rate {#check-wal-generation-rate  data-source-line="2954"}
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_current_wal_lsn();"
``` {data-source-line="2956"}

---

## 🔒 Security Best Practices

1. **Change Default Passwords** - Update all passwords in `.env`
2. **Enable SSL/TLS** - Configure PostgreSQL SSL certificates
3. **Firewall Rules** - Restrict access to PostgreSQL ports
4. **Network Isolation** - Use Docker networks properly
5. **Regular Backups** - Verify Barman backups daily

---

## 📞 Support

- **Documentation:** See `docs/` folder
- **Issues:** Check logs with `docker-compose logs`
- **Community:** [BahyWay GitHub](https://github.com/bahyway)

---

## 🎓 Next Steps

1. ✅ **Deploy BahyWay Projects**
   - AlarmInsight
   - ETLway
   - SmartForesight
   - (5 more projects)

2. ✅ **Monitoring Setup**
   - Prometheus + Grafana
   - Alert notifications
   - Performance metrics

3. ✅ **Production Hardening**
   - SSL/TLS certificates
   - Backup verification
   - Disaster recovery testing

---

**Cluster is ready for all 8 BahyWay projects!** 🚀