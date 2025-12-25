# ============================================================
# SCRIPT: Setup_Python_Env.ps1
# PURPOSE: bootstraps the Python environment for BahyWay
# ============================================================

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ReqFile = "$ScriptDir\requirements.txt"
$VenvDir = "$ScriptDir\.venv"
$PythonExe = "python"

# Import BahyWay Logging (If available, otherwise simple write-host)
Try { . "$ScriptDir\..\devops\BahyWay_Logging.ps1" } Catch { }

Write-Host ">>> INITIALIZING BAHYWAY PYTHON ENVIRONMENT..." -ForegroundColor Cyan

Try {
    # 1. Check if Python exists
    $pyVersion = & $PythonExe --version 2>&1
    if ($LASTEXITCODE -ne 0) {
        Throw "Python is not installed or not in PATH."
    }
    Write-Host "✓ Found $pyVersion" -ForegroundColor Green

    # 2. Create Virtual Environment (If not exists)
    if (-not (Test-Path $VenvDir)) {
        Write-Host "Creating Virtual Environment (.venv)..." -ForegroundColor Yellow
        & $PythonExe -m venv $VenvDir
        if ($LASTEXITCODE -ne 0) { Throw "Failed to create venv." }
    } else {
        Write-Host "✓ Virtual Environment exists." -ForegroundColor Green
    }

    # 3. Define path to the VENV pip and python
    $VenvPython = "$VenvDir\Scripts\python.exe"
    $VenvPip = "$VenvDir\Scripts\pip.exe"

    # 4. Upgrade Pip
    Write-Host "Upgrading Pip..." -ForegroundColor Gray
    & $VenvPython -m pip install --upgrade pip | Out-Null

    # 5. Install Requirements
    if (Test-Path $ReqFile) {
        Write-Host "Installing Libraries from requirements.txt..." -ForegroundColor Cyan
        & $VenvPip install -r $ReqFile
        if ($LASTEXITCODE -ne 0) { Throw "Failed to install requirements." }
        Write-Host "✓ All Python Dependencies Installed Successfully." -ForegroundColor Green
    } else {
        Throw "requirements.txt not found at $ReqFile"
    }

}
Catch {
    Write-Host "❌ FATAL ERROR: $_" -ForegroundColor Red
    Exit 1
}
Finally {
    Write-Host ">>> SETUP FINISHED." -ForegroundColor Cyan
}