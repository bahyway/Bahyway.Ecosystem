# 1. SETUP LOGGING
$LogFile = "$PWD/override_fix.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔧 APPLYING MANUAL VERSION OVERRIDE..." -ForegroundColor Cyan

# Define Path
$projFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

# 2. FORCE OVERRIDE IN .CSPROJ
# We explicitly set VersionOverride, which tells NuGet "Ignore the central file, use THIS version."
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

    <!-- THE FIX: VersionOverride forces NuGet to work -->
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

# 3. NUKE CACHES
Write-Host "🧹 Nuking local caches..."
dotnet nuget locals all --clear > $null 2>&1
Remove-Item -Path "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/bin" -Recurse -Force -ErrorAction SilentlyContinue

# 4. RESTORE & BUILD
Write-Host "📦 Restoring..."
dotnet restore $projFile

Write-Host "🔨 Building..."
$buildOutput = dotnet build $projFile 2>&1 | Out-String
Write-Host $buildOutput

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED." -ForegroundColor Red
}

Stop-Transcript
