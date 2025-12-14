# =======================================================
# BAHYWAY ECOSYSTEM - WPDWay Deployment Automation (v2)
# =======================================================
$ErrorActionPreference = "Stop"
$LogFile = "deploy.log"

# --- HELPER FUNCTION: Logging ---
function Log-Message {
    param ( [string]$Message, [string]$Color = "White" )
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogEntry = "[$Timestamp] $Message"
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $LogFile -Value $LogEntry
}

# Clear old log
if (Test-Path $LogFile) { Remove-Item $LogFile }
Log-Message "🚀 Starting WPDWay Deployment..." "Cyan"

# --- CONFIGURATION ---
$ContainerName = "bahyway-postgres-primary"
$DbUser = "postgres"
$DbName = "bahyway_ontoway"

# Paths (Relative to Repo Root)
$SqlSource = "src\Apps\WPDWay.Logic\WPDWay_Optimized.sql"
$ModelSource = "src\Apps\WPDWay.Logic\WPDWay_Optimized.cs"
$ActorSource = "src\Apps\WPDWay.Logic\WPDWay_Optimized.Actors.cs"

$KernelDomainPath = "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Domain"
$KernelActorsPath = "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Actors"

# --- PHASE 0: DATABASE CHECK ---
Log-Message "`n[0/3] 🔍 Checking Database Existence..." "Cyan"
$CheckDb = docker exec $ContainerName psql -U $DbUser -lqt | Select-String $DbName

if (-not $CheckDb) {
    Log-Message "⚠️ Database '$DbName' does not exist. Creating it now..." "Yellow"
    docker exec $ContainerName psql -U $DbUser -c "CREATE DATABASE $DbName;"
    if ($LASTEXITCODE -eq 0) { Log-Message "✅ Database Created." "Green" }
    else { Log-Message "❌ Failed to create database." "Red"; exit 1 }
} else {
    Log-Message "✅ Database '$DbName' exists." "Green"
}

# --- PHASE 1: DATABASE DEPLOYMENT ---
Log-Message "`n[1/3] 🛢️  Deploying WPDWay Database Schema..." "Cyan"

if (Test-Path $SqlSource) {
    # Capture Standard Error separately to log it
    $SqlError = Get-Content $SqlSource | docker exec -i $ContainerName psql -U $DbUser -d $DbName 2>&1

    if ($LASTEXITCODE -eq 0) {
        Log-Message "✅ Database Schema Applied Successfully." "Green"
    } else {
        Log-Message "❌ Database Deployment Failed. Details:" "Red"
        Log-Message "$SqlError" "Red"
        exit 1
    }
} else {
    Log-Message "❌ SQL Source file not found: $SqlSource" "Red"
    exit 1
}

# --- PHASE 2: CODE INTEGRATION ---
Log-Message "`n[2/3] 🧬 Integrating C# Logic into Shared Kernel..." "Cyan"

New-Item -ItemType Directory -Force -Path $KernelDomainPath | Out-Null
New-Item -ItemType Directory -Force -Path $KernelActorsPath | Out-Null

if (Test-Path $ModelSource) {
    Copy-Item -Path $ModelSource -Destination $KernelDomainPath -Force
    Log-Message "   -> Copied Domain Models" "Gray"
}

if (Test-Path $ActorSource) {
    Copy-Item -Path $ActorSource -Destination $KernelActorsPath -Force
    Log-Message "   -> Copied Actor Logic" "Gray"
}

Log-Message "✅ Code Integration Complete." "Green"

# --- PHASE 3: VERIFICATION BUILD ---
Log-Message "`n[3/3] 🔨 Verifying Build (BahyWay.SharedKernel)..." "Cyan"

Push-Location "src\BahyWay.Kernel\src\BahyWay.SharedKernel"
try {
    $BuildOutput = dotnet build --nologo 2>&1
    Log-Message "✅ Build Succeeded." "Green"
} catch {
    Log-Message "❌ Build Failed. Output:" "Red"
    Log-Message "$BuildOutput" "Red"
    Pop-Location
    exit 1
}
Pop-Location

Log-Message "`n🚀 WPDWay Deployment Finished Successfully." "Magenta"
