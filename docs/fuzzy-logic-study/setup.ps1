# Fuzzy Logic Study Package Setup Script
# For Windows PowerShell

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Fuzzy Logic Study Package Setup" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Check execution policy
$executionPolicy = Get-ExecutionPolicy
if ($executionPolicy -eq "Restricted") {
    Write-Host "! Execution policy is Restricted. You may need to run:" -ForegroundColor Yellow
    Write-Host "  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass" -ForegroundColor Yellow
    Write-Host ""
}

# Check Python
Write-Host "Checking Python installation..."
try {
    $pythonVersion = python --version 2>&1
    if ($pythonVersion -match "Python 3\.(\d+)") {
        Write-Host "✓ Python 3 found: $pythonVersion" -ForegroundColor Green
        $pythonCmd = "python"
    } else {
        Write-Host "✗ Python 3.8+ required. Found: $pythonVersion" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Python 3 not found. Please install Python 3.8 or later." -ForegroundColor Red
    Write-Host "  Download from: https://www.python.org/downloads/" -ForegroundColor Yellow
    exit 1
}

# Check pip
Write-Host "Checking pip..."
try {
    $pipVersion = pip --version 2>&1
    Write-Host "✓ pip found" -ForegroundColor Green
    $pipCmd = "pip"
} catch {
    Write-Host "✗ pip not found. Please install pip." -ForegroundColor Red
    exit 1
}

# Check Rust (optional)
Write-Host ""
Write-Host "Checking Rust installation (optional)..."
try {
    $rustVersion = rustc --version 2>&1
    Write-Host "✓ Rust found: $rustVersion" -ForegroundColor Green
    $rustAvailable = $true
} catch {
    Write-Host "! Rust not found. Rust projects will not be available." -ForegroundColor Yellow
    Write-Host "  Install from: https://rustup.rs/" -ForegroundColor Yellow
    $rustAvailable = $false
}

# Create Python virtual environment
Write-Host ""
Write-Host "Setting up Python environment..."
Set-Location python-fuzzy-logic

if (Test-Path "venv") {
    Write-Host "! Virtual environment already exists. Skipping creation." -ForegroundColor Yellow
} else {
    Write-Host "Creating virtual environment..."
    & $pythonCmd -m venv venv
    Write-Host "✓ Virtual environment created" -ForegroundColor Green
}

# Activate virtual environment
Write-Host "Activating virtual environment..."
& .\venv\Scripts\Activate.ps1

# Upgrade pip
Write-Host "Upgrading pip..."
& $pipCmd install --upgrade pip

# Install Python dependencies
Write-Host "Installing Python packages..."
if (Test-Path "requirements.txt") {
    & $pipCmd install -r requirements.txt
    Write-Host "✓ Python packages installed" -ForegroundColor Green
} else {
    Write-Host "! requirements.txt not found. Installing manually..." -ForegroundColor Yellow
    & $pipCmd install numpy scikit-fuzzy matplotlib jupyter ipykernel
}

# Create necessary directories
Write-Host ""
Write-Host "Creating project directories..."
$directories = @("src", "examples", "notebooks", "tools", ".vscode")
foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}
Write-Host "✓ Directories created" -ForegroundColor Green

Set-Location ..

# Setup Rust project
if ($rustAvailable) {
    Write-Host ""
    Write-Host "Setting up Rust environment..."
    Set-Location rust-fuzzy-logic

    if (Test-Path "Cargo.toml") {
        Write-Host "Building Rust project..."
        cargo build
        Write-Host "✓ Rust project built successfully" -ForegroundColor Green
    } else {
        Write-Host "! Cargo.toml not found. Skipping Rust setup." -ForegroundColor Yellow
    }

    Set-Location ..
}

# Summary
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Setup Complete! 🎉" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:"
Write-Host ""
Write-Host "1. For Python:"
Write-Host "   cd python-fuzzy-logic"
Write-Host "   .\venv\Scripts\Activate.ps1"
Write-Host "   python src\main.py"
Write-Host ""

if ($rustAvailable) {
    Write-Host "2. For Rust:"
    Write-Host "   cd rust-fuzzy-logic"
    Write-Host "   cargo run"
    Write-Host ""
}

Write-Host "3. Read the documentation:"
Write-Host "   - README.md for overview"
Write-Host "   - STUDY_GUIDE.md for learning path"
Write-Host "   - CHEATSHEET.md for quick reference"
Write-Host ""
Write-Host "4. Install VSCode extensions:"
Write-Host "   - Python (ms-python.python)"
Write-Host "   - Jupyter (ms-toolsai.jupyter)"
if ($rustAvailable) {
    Write-Host "   - rust-analyzer (rust-lang.rust-analyzer)"
}
Write-Host ""
Write-Host "Happy learning! 🧠✨"
