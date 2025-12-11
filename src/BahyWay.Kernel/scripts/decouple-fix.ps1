# 1. SETUP LOGGING
$LogFile = "$PWD/decouple_fix_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "✂️ DECOUPLING UI FROM CENTRAL MANAGEMENT..." -ForegroundColor Cyan

# 2. DEFINE PATHS
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"

# 3. REWRITE .CSPROJ (With CPM Disabled)
# We add <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
# And we explicitly define versions here.
$projContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <!-- CRITICAL FIX: Disable Central Management for this project -->
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Explicit Versions (Self-Contained) -->
    <PackageReference Include="Avalonia" Version="11.0.6" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.6" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.6" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.6" />
    <PackageReference Include="Avalonia.Diagnostics" Version="11.0.6" />
    <PackageReference Include="Avalonia.Controls.WebView" Version="11.0.6" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="Akka.Remote" Version="1.5.15" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Bahyway.SharedKernel\Bahyway.SharedKernel.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="Assets\ontoway_engine.html">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
'@
Set-Content -Path $projFile -Value $projContent -Encoding UTF8
Write-Host "✅ Project file rewritten to allow local versions."

# 4. KILL & CLEAN
Write-Host "☠️ Killing lingering processes..."
Stop-Process -Name "Bahyway.KGEditor.UI" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "Avalonia.Designer.HostApp" -Force -ErrorAction SilentlyContinue

Write-Host "🧹 Deep Cleaning..."
Remove-Item -Path "$uiPath/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$uiPath/obj" -Recurse -Force -ErrorAction SilentlyContinue

# 5. RESTORE & BUILD (Capture Errors)
Write-Host "📦 Restoring..."
$restoreOut = dotnet restore $projFile 2>&1 | Out-String
Write-Host $restoreOut

if ($LASTEXITCODE -eq 0) {
    Write-Host "🔨 Building..."
    $buildOut = dotnet build $projFile 2>&1 | Out-String
    Write-Host $buildOut

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
        dotnet run --project $projFile
    }
    else {
        Write-Host "❌ BUILD FAILED." -ForegroundColor Red
    }
}
else {
    Write-Host "❌ RESTORE FAILED." -ForegroundColor Red
}

Stop-Transcript
