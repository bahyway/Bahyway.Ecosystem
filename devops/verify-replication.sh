#!/bin/bash

# ============================================
# BAHYWAY POSTGRESQL REPLICATION VERIFICATION
# Checks replication status and lag
# ============================================

set -e

echo "🔍 REPLICATION STATUS CHECK"
echo "=============================================="

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Function to check if container is running
check_container() {
    if docker ps --format '{{.Names}}' | grep -q "^$1$"; then
        return 0
    else
        return 1
    fi
}

# Check Primary Status
echo ""
echo "${YELLOW}📊 PRIMARY NODE STATUS${NC}"
echo "=============================================="
if check_container "bahyway-postgres-primary"; then
    echo "${GREEN}✅ Primary container is running${NC}"

    # Check if it's in recovery mode (should be false)
    docker exec bahyway-postgres-primary psql -U postgres -t -c "SELECT pg_is_in_recovery();" | xargs | \
    if [ "$(cat)" = "f" ]; then
        echo "${GREEN}✅ Primary is in read-write mode${NC}"
    else
        echo "${RED}❌ Primary is in recovery mode (PROBLEM!)${NC}"
    fi

    # Show replication connections
    echo ""
    echo "Active replication connections:"
    docker exec bahyway-postgres-primary psql -U postgres -c "
        SELECT
            client_addr AS replica_ip,
            application_name,
            state,
            sync_state,
            COALESCE(replay_lag, '0'::interval) AS replay_lag,
            pg_size_pretty(pg_wal_lsn_diff(pg_current_wal_lsn(), replay_lsn)) AS lag_bytes
        FROM pg_stat_replication;
    "

    # Show current WAL position
    echo ""
    echo "Current WAL position:"
    docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_current_wal_lsn();"
else
    echo "${RED}❌ Primary container is not running${NC}"
fi

# Check Replica Status
echo ""
echo "${YELLOW}📊 REPLICA NODE STATUS${NC}"
echo "=============================================="
if check_container "bahyway-postgres-replica"; then
    echo "${GREEN}✅ Replica container is running${NC}"

    # Check if it's in recovery mode (should be true)
    docker exec bahyway-postgres-replica psql -U postgres -t -c "SELECT pg_is_in_recovery();" | xargs | \
    if [ "$(cat)" = "t" ]; then
        echo "${GREEN}✅ Replica is in hot standby mode${NC}"
    else
        echo "${RED}❌ Replica is NOT in recovery mode (PROBLEM!)${NC}"
    fi

    # Show recovery status
    echo ""
    echo "Recovery status:"
    docker exec bahyway-postgres-replica psql -U postgres -c "
        SELECT
            pg_last_wal_receive_lsn() AS receive_lsn,
            pg_last_wal_replay_lsn() AS replay_lsn,
            pg_last_xact_replay_timestamp() AS last_replay_time,
            NOW() - pg_last_xact_replay_timestamp() AS replication_lag;
    "

    # Show received WAL position
    echo ""
    echo "Last received WAL:"
    docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_last_wal_receive_lsn();"
else
    echo "${RED}❌ Replica container is not running${NC}"
fi

# Calculate Replication Lag
echo ""
echo "${YELLOW}📊 REPLICATION LAG${NC}"
echo "=============================================="
if check_container "bahyway-postgres-primary" && check_container "bahyway-postgres-replica"; then
    docker exec bahyway-postgres-primary psql -U postgres -c "
        SELECT
            application_name,
            COALESCE(replay_lag, '0'::interval) AS time_lag,
            pg_size_pretty(pg_wal_lsn_diff(pg_current_wal_lsn(), replay_lsn)) AS byte_lag,
            CASE
                WHEN replay_lag < '10 seconds'::interval THEN '${GREEN}✅ Excellent${NC}'
                WHEN replay_lag < '60 seconds'::interval THEN '${YELLOW}⚠️  Warning${NC}'
                ELSE '${RED}❌ Critical${NC}'
            END AS status
        FROM pg_stat_replication;
    "
fi

# Check HAProxy Status
echo ""
echo "${YELLOW}📊 HAPROXY STATUS${NC}"
echo "=============================================="
if check_container "bahyway-haproxy"; then
    echo "${GREEN}✅ HAProxy container is running${NC}"
    echo "Stats available at: http://localhost:7000/stats"
else
    echo "${RED}❌ HAProxy container is not running${NC}"
fi

# Summary
echo ""
echo "${GREEN}=============================================="
echo "✅ REPLICATION CHECK COMPLETED"
echo "=============================================="
echo ""
echo "Connection endpoints:"
echo "  • Primary (R/W):     localhost:5432 or localhost:5000 (via HAProxy)"
echo "  • Replica (R/O):     localhost:5433 or localhost:5001 (via HAProxy)"
echo "  • HAProxy Stats:     http://localhost:7000/stats"
echo ""