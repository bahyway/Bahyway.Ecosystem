#!/bin/bash
set -e

echo "ðŸ”¥ FAILOVER TEST"
echo "=============================================="

echo ""
echo "Step 1: Checking current status..."
docker exec bahyway-postgres-primary psql -U postgres -c "SELECT pg_is_in_recovery();" || echo "Primary is down"
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

echo ""
echo "Step 2: Stopping primary (simulate failure)..."
docker stop bahyway-postgres-primary
sleep 3

echo ""
echo "Step 3: Promoting replica to primary..."
docker exec bahyway-postgres-replica pg_ctl promote -D /var/lib/postgresql/data/pgdata
sleep 5

echo ""
echo "Step 4: Verifying promotion..."
docker exec bahyway-postgres-replica psql -U postgres -c "SELECT pg_is_in_recovery();"

echo ""
echo "âœ… Failover test complete!"