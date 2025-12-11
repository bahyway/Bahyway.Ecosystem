#!/bin/bash
set -e

echo "ðŸ” REPLICATION STATUS CHECK"
echo "=============================================="

if docker ps --format '{{.Names}}' | grep -q "^bahyway-postgres-primary$"; then
    echo ""
    echo "âœ… Primary container is running"
    docker exec bahyway-postgres-primary psql -U postgres -c "SELECT client_addr, state, sync_state, replay_lag FROM pg_stat_replication;"
else
    echo "âŒ Primary container is not running"
fi

if docker ps --format '{{.Names}}' | grep -q "^bahyway-postgres-replica$"; then
    echo ""
    echo "âœ… Replica container is running"
    docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"
else
    echo "âŒ Replica container is not running"
fi

echo ""
echo "âœ… Verification complete"