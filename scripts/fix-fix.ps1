# 1. SETUP PATHS
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"
$buildLog = "$PWD/real_build_log.txt"

Write-Host "🔧 FIXING CPM VERSION CONFLICT..." -ForegroundColor Cyan

# 2. REWRITE .CSPROJ (WITHOUT VERSIONS)
# This lets Directory.Packages.props handle the versions (The correct way)
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
    <!-- Note: No Version="" attributes here. They come from Directory.Packages.props -->
    <PackageReference Include="Avalonia" />
    <PackageReference Include="Avalonia.Desktop" />
    <PackageReference Include="Avalonia.Themes.Fluent" />
    <PackageReference Include="Avalonia.Fonts.Inter" />
    <PackageReference Include="Avalonia.Diagnostics" />
    <PackageReference Include="Avalonia.Controls.WebView" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="Akka.Remote" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\Bahyway.SharedKernel\Bahyway.SharedKernel.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Content Include="Assets\ontoway_engine.html">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
'@
Set-Content -Path $projFile -Value $projContent -Encoding UTF8

# 3. BUILD AND CAPTURE LOG
Write-Host "🔨 Building..."
# We use standard redirection to guarantee we catch the error message
dotnet build $projFile > $buildLog 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED." -ForegroundColor Red
    Write-Host "--- ERROR LOG START ---" -ForegroundColor Gray
    Get-Content $buildLog | Select-Object -Last 20
    Write-Host "--- ERROR LOG END ---" -ForegroundColor Gray
}
