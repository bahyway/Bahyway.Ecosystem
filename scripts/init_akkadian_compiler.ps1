# scripts/init_akkadian_compiler.ps1

Write-Host "🚀 Adding Akkadian Compiler to Bahyway Ecosystem..." -ForegroundColor Cyan

# 1. Ensure we are in the src folder
if (Test-Path "src") {
    Set-Location "src"
} else {
    # If user is already inside src or it doesn't exist structurally
    Write-Warning "Ensure you are running this from the Solution Root."
}

# 2. Check for Solution File
if (-not (Test-Path "../Bahyway.sln") -and -not (Test-Path "Bahyway.sln")) {
    Write-Host "⚠️ No Solution found. Creating Bahyway.sln..." -ForegroundColor Yellow
    dotnet new sln -n "Bahyway"
}

# 3. Create SharedKernel (Where the Compiler Lives)
if (-not (Test-Path "Bahyway.SharedKernel")) {
    Write-Host "🔨 Creating Bahyway.SharedKernel..." -ForegroundColor Gray
    dotnet new classlib -n "Bahyway.SharedKernel"
    dotnet sln add "Bahyway.SharedKernel/Bahyway.SharedKernel.csproj"
} else {
    Write-Host "⏩ Bahyway.SharedKernel already exists. Skipping creation." -ForegroundColor DarkGray
}

# 4. Create Orchestrator (The Akka Host)
if (-not (Test-Path "Bahyway.Orchestrator")) {
    Write-Host "🔨 Creating Bahyway.Orchestrator..." -ForegroundColor Gray
    dotnet new console -n "Bahyway.Orchestrator"
    dotnet sln add "Bahyway.Orchestrator/Bahyway.Orchestrator.csproj"
} else {
    Write-Host "⏩ Bahyway.Orchestrator already exists. Skipping creation." -ForegroundColor DarkGray
}

# 5. Create Tests Project
if (-not (Test-Path "Bahyway.Tests")) {
    Write-Host "🔨 Creating Bahyway.Tests..." -ForegroundColor Gray
    dotnet new xunit -n "Bahyway.Tests"
    dotnet sln add "Bahyway.Tests/Bahyway.Tests.csproj"
}

# 6. Add Project References (Link them together)
Write-Host "🔗 Linking Projects..." -ForegroundColor Gray
dotnet add "Bahyway.Orchestrator/Bahyway.Orchestrator.csproj" reference "Bahyway.SharedKernel/Bahyway.SharedKernel.csproj"
dotnet add "Bahyway.Tests/Bahyway.Tests.csproj" reference "Bahyway.SharedKernel/Bahyway.SharedKernel.csproj"

# If KGEditor exists, link it too
if (Test-Path "Bahyway.KGEditor") {
    dotnet add "Bahyway.KGEditor/Bahyway.KGEditor.csproj" reference "Bahyway.SharedKernel/Bahyway.SharedKernel.csproj"
}

# 7. Install Nuget Packages (The Tech Stack)
Write-Host "📦 Installing NuGet Packages..." -ForegroundColor Yellow

# SharedKernel (Compiler & DB & Vectors)
dotnet add "Bahyway.SharedKernel" package Npgsql --version 8.0.0
dotnet add "Bahyway.SharedKernel" package Pgvector --version 0.2.0
dotnet add "Bahyway.SharedKernel" package Akka --version 1.5.15
dotnet add "Bahyway.SharedKernel" package Microsoft.CodeAnalysis.CSharp --version 4.8.0 # For Roslyn

# Orchestrator (Akka Host)
dotnet add "Bahyway.Orchestrator" package Akka.Remote --version 1.5.15

Write-Host "✅ Akkadian Compiler components added successfully!" -ForegroundColor Green
