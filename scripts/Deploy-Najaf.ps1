# =======================================================
# BAHYWAY ECOSYSTEM - Najaf Cemetery Deployment
# =======================================================
$ErrorActionPreference = "Stop"
$LogFile = "deploy_najaf.log"

# (Include Helper Function Log-Message here... same as above)
function Log-Message { param([string]$Msg, [string]$Col="White") Write-Host $Msg -ForegroundColor $Col }

Log-Message "🚀 Starting Najaf Cemetery Deployment..." "Cyan"

$ContainerName = "bahyway-postgres-primary"
$DbUser = "postgres"
$DbName = "bahyway_najaf" # <--- NEW DATABASE

$SourcePath = "src\Apps\Najaf.Cemetery"
$AkkFile = "$SourcePath\Najaf_Master.akk"
$Compiler = "src\Akkadian.Compiler\Akkadian.Cli\bin\Debug\net8.0\Akkadian.Cli.exe"

# 1. Compile
Log-Message "`n[1/4] Compiling..." "Cyan"
& $Compiler $AkkFile

# 2. Database
Log-Message "`n[2/4] Deploying DB..." "Cyan"
$CheckDb = docker exec $ContainerName psql -U $DbUser -lqt | Select-String $DbName
if (-not $CheckDb) { docker exec $ContainerName psql -U $DbUser -c "CREATE DATABASE $DbName;" }

Get-Content "$SourcePath\Najaf_Master.sql" | docker exec -i $ContainerName psql -U $DbUser -d $DbName

# 3. Code Integration
Log-Message "`n[3/4] Copying Code..." "Cyan"
Copy-Item "$SourcePath\Najaf_Master.cs" -Destination "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Domain\" -Force
# (No actors in this specific file, or add if generated)

# 4. Build
Log-Message "`n[4/4] Verifying Kernel..." "Cyan"
Push-Location "src\BahyWay.Kernel\src\BahyWay.SharedKernel"
dotnet build --nologo --verbosity quiet
Pop-Location

Log-Message "`n🚀 Najaf Deployment Complete." "Magenta"
