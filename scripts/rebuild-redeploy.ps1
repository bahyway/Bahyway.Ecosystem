# 1. Kill any stuck processes
Stop-Process -Name "Akkadian.Cli" -Force -ErrorAction SilentlyContinue

# 2. Delete the BAD file from the Kernel manually
Remove-Item "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Actors\WPDWay_Optimized.Actors.cs" -Force -ErrorAction SilentlyContinue
Write-Host "🗑️ Deleted old broken file." -ForegroundColor Yellow

# 3. Clean and Rebuild the Compiler
dotnet clean "src\Akkadian.Compiler\Akkadian.Cli\Akkadian.Cli.csproj"
dotnet build "src\Akkadian.Compiler\Akkadian.Cli\Akkadian.Cli.csproj"

# 4. Run the WPDWay Deployment to generate the NEW file
.\src\devops\Deploy-WPDWay.ps1

# 5. Verify the fix
$NewFileContent = Get-Content "src\BahyWay.Kernel\src\BahyWay.SharedKernel\Actors\WPDWay_Optimized.Actors.cs"
if ($NewFileContent -match "Vector<int>.Zero") {
    Write-Host "✅ FIX VERIFIED: The new file uses Vector<int>." -ForegroundColor Green
} else {
    Write-Host "❌ ERROR: The file still has the old code! Check ActorGenerator.cs" -ForegroundColor Red
    exit
}

# 6. Now Run Najaf
Write-Host "🚀 Running Najaf Deployment..." -ForegroundColor Cyan
.\src\devops\Deploy-Najaf.ps1
