#!/bin/bash

# ============================================
# BAHYWAY POSTGRESQL FAILOVER TEST
# Simulates primary failure and promotes replica
# ============================================

set -e

echo "🔥 FAILOVER TEST - Simulating Primary Failure"
echo "=============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Step 1: Check current cluster status
echo ""
echo "${YELLOW}📊 Step 1: Checking current cluster status...${NC}"
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_is_in_recovery();" || echo "Primary is down"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

# Step 2: Check replication lag
echo ""
echo "${YELLOW}📊 Step 2: Checking replication lag...${NC}"
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT client_addr, state, sync_state, replay_lag FROM pg_stat_replication;" || echo "Primary is down"

# Step 3: Stop primary (simulate failure)
echo ""
echo "${RED}💥 Step 3: Stopping primary node (simulating failure)...${NC}"
docker stop bahyway-postgres-primary
sleep 3

# Step 4: Verify primary is down
echo ""
echo "${YELLOW}📊 Step 4: Verifying primary is down...${NC}"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

# Step 5: Promote replica to primary
echo ""
echo "${GREEN}⬆️  Step 5: Promoting replica to primary...${NC}"
docker exec bahyway-postgres-replica pg_ctl promote -D /var/lib/postgresql/data/pgdata

# Wait for promotion to complete
sleep 5

# Step 6: Verify new primary
echo ""
echo "${YELLOW}📊 Step 6: Verifying promotion...${NC}"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_current_wal_lsn();"

# Step 7: Test write to new primary
echo ""
echo "${GREEN}✍️  Step 7: Testing write to new primary...${NC}"
docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -c "CREATE TABLE IF NOT EXISTS failover_test (id SERIAL PRIMARY KEY, test_time TIMESTAMP DEFAULT NOW());"
docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -c "INSERT INTO failover_test DEFAULT VALUES;"
docker exec bahyway-postgres-replica psql -U postgres -d alarminsight -c "SELECT * FROM failover_test;"

# Step 8: Restart old primary as new replica (optional)
echo ""
echo "${BLUE}🔄 Step 8: Restarting old primary as new replica (optional)...${NC}"
read -p "Do you want to restart the old primary as a new replica? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
    docker start bahyway-postgres-primary
    echo "${GREEN}✅ Old primary restarted. Manual reconfiguration needed.${NC}"
else
    echo "${YELLOW}⏭️  Skipped restarting old primary.${NC}"
fi

echo ""
echo "${GREEN}=============================================="
echo "✅ FAILOVER TEST COMPLETED"
echo "=============================================="
echo ""
echo "Summary:"
echo "  • Old primary: STOPPED (simulated failure)"
echo "  • Old replica: PROMOTED to primary"
echo "  • New primary: ACCEPTING WRITES"
echo ""
echo "Connection strings:"
echo "  • New Primary: postgres-replica:5432"
echo "  • HAProxy Primary: localhost:5000"
echo ""