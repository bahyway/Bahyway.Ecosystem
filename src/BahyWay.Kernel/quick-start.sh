#!/bin/bash

# ============================================
# BAHYWAY POSTGRESQL HA CLUSTER
# Quick Start Deployment Script
# ============================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Banner
echo "${BLUE}"
cat << "EOF"
╔════════════════════════════════════════════════════════════╗
║                                                            ║
║   ██████╗  █████╗ ██╗  ██╗██╗   ██╗██╗    ██╗ █████╗ ██╗ ║
║   ██╔══██╗██╔══██╗██║  ██║╚██╗ ██╔╝██║    ██║██╔══██╗╚██╗║
║   ██████╔╝███████║███████║ ╚████╔╝ ██║ █╗ ██║███████║ ╚██║
║   ██╔══██╗██╔══██║██╔══██║  ╚██╔╝  ██║███╗██║██╔══██║ ██╔╝║
║   ██████╔╝██║  ██║██║  ██║   ██║   ╚███╔███╔╝██║  ██║██╔╝ ║
║   ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝    ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝  ║
║                                                            ║
║         PostgreSQL HA Cluster - Quick Start               ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
EOF
echo "${NC}"

echo ""
echo "${GREEN}🚀 BahyWay PostgreSQL HA Cluster Deployment${NC}"
echo "${BLUE}============================================${NC}"
echo ""

# Check prerequisites
echo "${YELLOW}📋 Checking prerequisites...${NC}"

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "${RED}❌ Docker is not installed${NC}"
    echo "Install from: https://docs.docker.com/get-docker/"
    exit 1
fi
echo "${GREEN}✅ Docker installed${NC}"

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "${RED}❌ Docker Compose is not installed${NC}"
    echo "Install from: https://docs.docker.com/compose/install/"
    exit 1
fi
echo "${GREEN}✅ Docker Compose installed${NC}"

# Check Ansible
if ! command -v ansible-playbook &> /dev/null; then
    echo "${YELLOW}⚠️  Ansible not installed (optional - will use Docker Compose only)${NC}"
    USE_ANSIBLE=false
else
    echo "${GREEN}✅ Ansible installed${NC}"
    USE_ANSIBLE=true
fi

echo ""

# Deployment options
echo "${BLUE}📦 Deployment Options:${NC}"
echo "  1) Full automated deployment (Ansible + Docker Compose)"
echo "  2) Docker Compose only (manual configuration)"
echo "  3) Exit"
echo ""
read -p "Select option [1-3]: " DEPLOY_OPTION

case $DEPLOY_OPTION in
    1)
        if [ "$USE_ANSIBLE" = false ]; then
            echo "${RED}❌ Ansible is required for option 1${NC}"
            exit 1
        fi

        echo ""
        echo "${GREEN}🚀 Starting full automated deployment...${NC}"
        echo ""

        # Run Ansible playbooks
        cd ansible

        echo "${BLUE}Step 1/4: Deploying cluster...${NC}"
        ansible-playbook playbooks/01-setup-cluster.yml

        echo ""
        echo "${BLUE}Step 2/4: Configuring replication...${NC}"
        ansible-playbook playbooks/02-configure-replication.yml

        echo ""
        echo "${BLUE}Step 3/4: Setting up Barman backups...${NC}"
        ansible-playbook playbooks/03-setup-barman.yml

        echo ""
        echo "${BLUE}Step 4/4: Deploying migrations...${NC}"
        read -p "Deploy AlarmInsight database migrations? (y/n): " DEPLOY_MIGRATIONS
        if [[ $DEPLOY_MIGRATIONS =~ ^[Yy]$ ]]; then
            ansible-playbook playbooks/04-deploy-migrations.yml
        else
            echo "${YELLOW}⏭️  Skipping migrations${NC}"
        fi

        cd ..
        ;;

    2)
        echo ""
        echo "${GREEN}🚀 Starting Docker Compose deployment...${NC}"
        echo ""

        cd docker

        # Pull images
        echo "${BLUE}📥 Pulling Docker images...${NC}"
        docker-compose pull

        # Start cluster
        echo "${BLUE}🚀 Starting cluster...${NC}"
        docker-compose up -d

        # Wait for health checks
        echo "${BLUE}⏳ Waiting for services to be healthy...${NC}"
        sleep 30

        cd ..
        ;;

    3)
        echo "${YELLOW}👋 Exiting...${NC}"
        exit 0
        ;;

    *)
        echo "${RED}❌ Invalid option${NC}"
        exit 1
        ;;
esac

# Verify deployment
echo ""
echo "${BLUE}🔍 Verifying deployment...${NC}"
echo ""

# Check containers
echo "${YELLOW}Checking containers...${NC}"
docker ps --filter "name=bahyway" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""

# Verify replication
if [ -x "docker/scripts/verify-replication.sh" ]; then
    echo "${YELLOW}Checking replication status...${NC}"
    ./docker/scripts/verify-replication.sh
else
    echo "${YELLOW}⚠️  Replication verification script not found${NC}"
fi

# Success message
echo ""
echo "${GREEN}═══════════════════════════════════════════════════════════${NC}"
echo "${GREEN}✅ DEPLOYMENT COMPLETE!${NC}"
echo "${GREEN}═══════════════════════════════════════════════════════════${NC}"
echo ""
echo "${BLUE}📊 Cluster Information:${NC}"
echo ""
echo "  ${GREEN}Connection Endpoints:${NC}"
echo "    • Primary (R/W):     localhost:5432 or localhost:5000 (HAProxy)"
echo "    • Replica (R/O):     localhost:5433 or localhost:5001 (HAProxy)"
echo "    • HAProxy Stats:     http://localhost:7000/stats"
echo ""
echo "  ${GREEN}Management Commands:${NC}"
echo "    • Verify replication: ./docker/scripts/verify-replication.sh"
echo "    • Test failover:      ./docker/scripts/test-failover.sh"
echo "    • Create backup:      docker exec bahyway-barman barman backup primary"
echo "    • View logs:          docker-compose -f docker/docker-compose.yml logs -f"
echo ""
echo "  ${GREEN}Connection String (for AlarmInsight):${NC}"
echo "    Host=localhost;Port=5000;Database=alarminsight;Username=postgres;Password=yourpassword"
echo ""
echo "  ${GREEN}Documentation:${NC}"
echo "    • Setup Guide:        docs/SETUP.md"
echo "    • Failover Guide:     docs/FAILOVER.md"
echo "    • Backup Guide:       docs/BACKUP-RESTORE.md"
echo ""
echo "${BLUE}🎯 Next Steps:${NC}"
echo "  1. Update your connection strings to use port 5000 (HAProxy)"
echo "  2. Run your AlarmInsight API: cd ../../src/AlarmInsight.API && dotnet run"
echo "  3. Access Swagger UI: https://localhost:5001"
echo "  4. Test failover: ./docker/scripts/test-failover.sh"
echo ""
echo "${GREEN}🚀 Your production-grade PostgreSQL HA cluster is ready!${NC}"
echo ""