<#
.SYNOPSIS
    Creates a standard Clean Architecture solution for the Bahyway Ecosystem.
.DESCRIPTION
    Generates Domain, Application, Infrastructure, and Orchestrator projects,
    links them together, and ensures .NET 8 compatibility.
.PARAMETER Name
    The name of the new project (e.g., "ETLWay", "Akkadian").
#>
param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Name
)

# --- Configuration ---
$RootFolder = "src/$Name.Core"
$SlnName = "$Name.Core"
$Framework = "net8.0" # Force LTS version to avoid Preview errors

Write-Host "🚀 Initializing Bahyway Project: $Name..." -ForegroundColor Cyan

# 1. Create Directory Structure
if (!(Test-Path $RootFolder)) {
    New-Item -ItemType Directory -Force -Path $RootFolder | Out-Null
}
Set-Location $RootFolder

# 2. Create the "Shield" (Prevent NU1008 Error)
# This ensures this specific solution ignores the parent repo's strict versioning rules
$shieldContent = @"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
"@
Set-Content -Path "Directory.Packages.props" -Value $shieldContent
Set-Content -Path "Directory.Build.props" -Value $shieldContent
Write-Host "✅ CPM Shield Created (Build Errors Prevented)" -ForegroundColor Green

# 3. Create Solution
dotnet new sln -n $SlnName
Write-Host "✅ Solution Created" -ForegroundColor Green

# 4. Create Projects (Forcing .NET 8)
# Domain
dotnet new classlib -n "$Name.Domain" -f $Framework
# Application
dotnet new classlib -n "$Name.Application" -f $Framework
# Infrastructure
dotnet new classlib -n "$Name.Infrastructure" -f $Framework
# Orchestrator (Worker Service)
dotnet new worker -n "$Name.Orchestrator" -f $Framework

Write-Host "✅ Projects Created (Framework: $Framework)" -ForegroundColor Green

# 5. Add to Solution
dotnet sln "$SlnName.sln" add "$Name.Domain/$Name.Domain.csproj"
dotnet sln "$SlnName.sln" add "$Name.Application/$Name.Application.csproj"
dotnet sln "$SlnName.sln" add "$Name.Infrastructure/$Name.Infrastructure.csproj"
dotnet sln "$SlnName.sln" add "$Name.Orchestrator/$Name.Orchestrator.csproj"

# 6. Link References (Clean Architecture)
# App -> Domain
dotnet add "$Name.Application/$Name.Application.csproj" reference "$Name.Domain/$Name.Domain.csproj"

# Infra -> App & Domain
dotnet add "$Name.Infrastructure/$Name.Infrastructure.csproj" reference "$Name.Application/$Name.Application.csproj"
dotnet add "$Name.Infrastructure/$Name.Infrastructure.csproj" reference "$Name.Domain/$Name.Domain.csproj"

# Orchestrator -> Infra & App
dotnet add "$Name.Orchestrator/$Name.Orchestrator.csproj" reference "$Name.Infrastructure/$Name.Infrastructure.csproj"
dotnet add "$Name.Orchestrator/$Name.Orchestrator.csproj" reference "$Name.Application/$Name.Application.csproj"

Write-Host "✅ References Linked" -ForegroundColor Green

# 7. Final Build Check
dotnet build

Write-Host "------------------------------------------------" -ForegroundColor Cyan
Write-Host "🎉 Project '$Name' is ready!" -ForegroundColor Cyan
Write-Host "📂 Location: src/$Name.Core" -ForegroundColor Cyan
Write-Host "------------------------------------------------"

# Return to root
Set-Location ../..