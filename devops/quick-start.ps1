# ============================================
# BAHYWAY POSTGRESQL HA CLUSTER
# Quick Start Deployment Script (PowerShell)
# ============================================

# Requires PowerShell 7+
#Requires -Version 7.0

# Set error action
$ErrorActionPreference = "Stop"

# Colors
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

# Banner
Write-Host @"

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
║                  (Windows PowerShell)                      ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

Write-Host ""
Write-Host "🚀 BahyWay PostgreSQL HA Cluster Deployment" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Blue
Write-Host ""

# Check prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

# Check Docker
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker installed: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not installed" -ForegroundColor Red
    Write-Host "Install from: https://docs.docker.com/desktop/install/windows-install/" -ForegroundColor Yellow
    exit 1
}

# Check Docker Compose
try {
    $composeVersion = docker-compose --version
    Write-Host "✅ Docker Compose installed: $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose is not installed" -ForegroundColor Red
    exit 1
}

# Check Ansible (optional)
try {
    $ansibleVersion = ansible-playbook --version 2>$null
    Write-Host "✅ Ansible installed" -ForegroundColor Green
    $UseAnsible = $true
} catch {
    Write-Host "⚠️  Ansible not installed (optional - will use Docker Compose only)" -ForegroundColor Yellow
    $UseAnsible = $false
}

Write-Host ""

# Deployment options
Write-Host "📦 Deployment Options:" -ForegroundColor Blue
Write-Host "  1) Docker Compose deployment (Recommended for Windows)"
Write-Host "  2) Full automated deployment (Requires Ansible + WSL)"
Write-Host "  3) Exit"
Write-Host ""
$DeployOption = Read-Host "Select option [1-3]"

switch ($DeployOption) {
    "1" {
        Write-Host ""
        Write-Host "🚀 Starting Docker Compose deployment..." -ForegroundColor Green
        Write-Host ""

        Set-Location docker

        # Pull images
        Write-Host "📥 Pulling Docker images..." -ForegroundColor Blue
        docker-compose pull

        # Start cluster
        Write-Host "🚀 Starting cluster..." -ForegroundColor Blue
        docker-compose up -d

        # Wait for health checks
        Write-Host "⏳ Waiting for services to be healthy..." -ForegroundColor Blue
        Start-Sleep -Seconds 30

        Set-Location ..
    }

    "2" {
        if (-not $UseAnsible) {
            Write-Host "❌ Ansible is required for option 2" -ForegroundColor Red
            Write-Host "Please install WSL and Ansible, or use option 1" -ForegroundColor Yellow
            exit 1
        }

        Write-Host ""
        Write-Host "🚀 Starting full automated deployment..." -ForegroundColor Green
        Write-Host ""

        Set-Location ansible

        Write-Host "Step 1/4: Deploying cluster..." -ForegroundColor Blue
        ansible-playbook playbooks/01-setup-cluster.yml

        Write-Host ""
        Write-Host "Step 2/4: Configuring replication..." -ForegroundColor Blue
        ansible-playbook playbooks/02-configure-replication.yml

        Write-Host ""
        Write-Host "Step 3/4: Setting up Barman backups..." -ForegroundColor Blue
        ansible-playbook playbooks/03-setup-barman.yml

        Write-Host ""
        Write-Host "Step 4/4: Deploying migrations..." -ForegroundColor Blue
        $DeployMigrations = Read-Host "Deploy AlarmInsight database migrations? (y/n)"
        if ($DeployMigrations -match "^[Yy]$") {
            ansible-playbook playbooks/04-deploy-migrations.yml
        } else {
            Write-Host "⏭️  Skipping migrations" -ForegroundColor Yellow
        }

        Set-Location ..
    }

    "3" {
        Write-Host "👋 Exiting..." -ForegroundColor Yellow
        exit 0
    }

    default {
        Write-Host "❌ Invalid option" -ForegroundColor Red
        exit 1
    }
}

# Verify deployment
Write-Host ""
Write-Host "🔍 Verifying deployment..." -ForegroundColor Blue
Write-Host ""

# Check containers
Write-Host "Checking containers..." -ForegroundColor Yellow
docker ps --filter "name=bahyway" --format "table {{.Names}}`t{{.Status}}`t{{.Ports}}"

Write-Host ""

# Verify replication (if script exists)
$verifyScript = "docker/scripts/verify-replication.sh"
if (Test-Path $verifyScript) {
    Write-Host "Checking replication status..." -ForegroundColor Yellow
    # Run in Git Bash if available, otherwise skip
    if (Get-Command bash -ErrorAction SilentlyContinue) {
        bash $verifyScript
    } else {
        Write-Host "⚠️  Skipping replication check (bash not found)" -ForegroundColor Yellow
        Write-Host "To verify manually: docker exec bahyway-postgres-primary psql -U postgres -c 'SELECT * FROM pg_stat_replication;'" -ForegroundColor Yellow
    }
}

# Success message
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "✅ DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Cluster Information:" -ForegroundColor Blue
Write-Host ""
Write-Host "  Connection Endpoints:" -ForegroundColor Green
Write-Host "    • Primary (R/W):     localhost:5432 or localhost:5000 (HAProxy)"
Write-Host "    • Replica (R/O):     localhost:5433 or localhost:5001 (HAProxy)"


<p data-source-line="4280" class="empty-line final-line end-of-document" style="margin:0;"></p>
