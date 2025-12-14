# =======================================================
# BAHYWAY ECOSYSTEM - beeMDM Deployment Automation
# =======================================================
$ErrorActionPreference = "Stop"
$LogFile = "deploy_beemdm.log"

function Log-Message {
    param ( [string]$Message, [string]$Color = "White" )
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $LogFile -Value "[$Timestamp] $Message"
}

if (Test-Path $LogFile) { Remove-Item $LogFile }
Log-Message "🚀 Starting beeMDM Deployment..." "Cyan"

# --- CONFIGURATION ---
$ContainerName = "bahyway-postgres-primary"
$DbUser = "postgres"
$DbName = "bahyway_beemdm" # <--- NEW DATABASE

# Paths
$SourcePath = "src\Apps\beeMDM.Logic"
$AkkFile = "$SourcePath\HiveMind.akk"
$Compiler = "src\Akkadian.Compiler\Akkadian.Cli\bin\Debug\net8.0\Akkadian.Cli.exe"

$KernelDomainPath = "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Domain"
$KernelActorsPath = "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Actors"

# --- PHASE 1: COMPILATION (Regenerate artifacts) ---
Log-Message "`n[1/4] ⚙️  Compiling Akkadian DSL..." "Cyan"
& $Compiler $AkkFile
if ($LASTEXITCODE -eq 0) { Log-Message "✅ Compilation Successful." "Green" }
else { Log-Message "❌ Compilation Failed." "Red"; exit 1 }

# --- PHASE 2: DATABASE CHECK & DEPLOY ---
Log-Message "`n[2/4] 🛢️  Checking Database..." "Cyan"
$CheckDb = docker exec $ContainerName psql -U $DbUser -lqt | Select-String $DbName

if (-not $CheckDb) {
    Log-Message "⚠️ Database '$DbName' does not exist. Creating it now..." "Yellow"
    docker exec $ContainerName psql -U $DbUser -c "CREATE DATABASE $DbName;"
}

Log-Message "   -> Deploying Schema..." "Cyan"
$SqlFile = "$SourcePath\HiveMind.sql"
$SqlError = Get-Content $SqlFile | docker exec -i $ContainerName psql -U $DbUser -d $DbName 2>&1

if ($LASTEXITCODE -eq 0) { Log-Message "✅ Database Deployed." "Green" }
else { Log-Message "❌ Database Deployment Failed:`n$SqlError" "Red"; exit 1 }

# --- PHASE 3: CODE INTEGRATION ---
Log-Message "`n[3/4] 🧬 Integrating Code..." "Cyan"
Copy-Item "$SourcePath\HiveMind.cs" -Destination $KernelDomainPath -Force
Copy-Item "$SourcePath\HiveMind.Actors.cs" -Destination $KernelActorsPath -Force
Log-Message "✅ Code Copied to Kernel." "Green"

# --- PHASE 4: VERIFICATION BUILD ---
Log-Message "`n[4/4] 🔨 Verifying Kernel Build..." "Cyan"
Push-Location "src\BahyWay.Kernel\src\BahyWay.SharedKernel"
dotnet build --nologo --verbosity quiet
Pop-Location
Log-Message "`n🚀 beeMDM Deployment Finished Successfully." "Magenta"
