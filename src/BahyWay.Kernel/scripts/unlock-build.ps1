# 1. SETUP LOGGING
$LogFile = "$PWD/unlock_build_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "☠️ KILLING ZOMBIE PROCESSES..." -ForegroundColor Red

# 2. KILL STUCK PROCESSES
# We try to kill dotnet.exe to release file locks (Ignore errors if not running)
Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "Bahyway.KGEditor.UI" -Force -ErrorAction SilentlyContinue

# Wait a moment for Windows to release locks
Start-Sleep -Seconds 2

# 3. FORCE DELETE BIN/OBJ
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"

Write-Host "🧹 Deleting locked folders..."
Remove-Item -Path "$uiPath/bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$uiPath/obj" -Recurse -Force -ErrorAction SilentlyContinue

# 4. ENSURE VERSION OVERRIDE IS STILL THERE
# (Re-applying the .csproj fix just in case the previous script failed mid-write)
$projContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" />
    <PackageReference Include="Avalonia.Desktop" />
    <PackageReference Include="Avalonia.Themes.Fluent" />
    <PackageReference Include="Avalonia.Fonts.Inter" />
    <PackageReference Include="Avalonia.Diagnostics" />
    <!-- MANUAL OVERRIDE to bypass Central Package Management issues -->
    <PackageReference Include="Avalonia.Controls.WebView" VersionOverride="11.0.6" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="Akka.Remote" />
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

# 5. RESTORE & BUILD
Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Check '$LogFile'." -ForegroundColor Red
}

Stop-Transcript
