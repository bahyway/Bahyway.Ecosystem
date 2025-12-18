#!/bin/bash

# Fuzzy Logic Study Package Setup Script
# For Linux and macOS

set -e  # Exit on error

echo "=================================="
echo "Fuzzy Logic Study Package Setup"
echo "=================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check Python
echo "Checking Python installation..."
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version | cut -d' ' -f2)
    echo -e "${GREEN}✓ Python 3 found: $PYTHON_VERSION${NC}"
    PYTHON_CMD="python3"
elif command -v python &> /dev/null; then
    PYTHON_VERSION=$(python --version | cut -d' ' -f2)
    if [[ $PYTHON_VERSION == 3.* ]]; then
        echo -e "${GREEN}✓ Python 3 found: $PYTHON_VERSION${NC}"
        PYTHON_CMD="python"
    else
        echo -e "${RED}✗ Python 3.8+ required. Found Python $PYTHON_VERSION${NC}"
        exit 1
    fi
else
    echo -e "${RED}✗ Python 3 not found. Please install Python 3.8 or later.${NC}"
    exit 1
fi

# Check pip
echo "Checking pip..."
if command -v pip3 &> /dev/null; then
    echo -e "${GREEN}✓ pip3 found${NC}"
    PIP_CMD="pip3"
elif command -v pip &> /dev/null; then
    echo -e "${GREEN}✓ pip found${NC}"
    PIP_CMD="pip"
else
    echo -e "${RED}✗ pip not found. Please install pip.${NC}"
    exit 1
fi

# Check Rust (optional)
echo ""
echo "Checking Rust installation (optional)..."
if command -v cargo &> /dev/null; then
    RUST_VERSION=$(rustc --version | cut -d' ' -f2)
    echo -e "${GREEN}✓ Rust found: $RUST_VERSION${NC}"
    RUST_AVAILABLE=true
else
    echo -e "${YELLOW}! Rust not found. Rust projects will not be available.${NC}"
    echo "  Install from: https://rustup.rs/"
    RUST_AVAILABLE=false
fi

# Create Python virtual environment
echo ""
echo "Setting up Python environment..."
cd python-fuzzy-logic

if [ -d "venv" ]; then
    echo -e "${YELLOW}! Virtual environment already exists. Skipping creation.${NC}"
else
    echo "Creating virtual environment..."
    $PYTHON_CMD -m venv venv
    echo -e "${GREEN}✓ Virtual environment created${NC}"
fi

# Activate virtual environment
echo "Activating virtual environment..."
source venv/bin/activate

# Upgrade pip
echo "Upgrading pip..."
$PIP_CMD install --upgrade pip

# Install Python dependencies
echo "Installing Python packages..."
if [ -f "requirements.txt" ]; then
    $PIP_CMD install -r requirements.txt
    echo -e "${GREEN}✓ Python packages installed${NC}"
else
    echo -e "${YELLOW}! requirements.txt not found. Installing manually...${NC}"
    $PIP_CMD install numpy scikit-fuzzy matplotlib jupyter ipykernel
fi

# Create necessary directories
echo ""
echo "Creating project directories..."
mkdir -p src
mkdir -p examples
mkdir -p notebooks
mkdir -p tools
mkdir -p .vscode

echo -e "${GREEN}✓ Directories created${NC}"

cd ..

# Setup Rust project
if [ "$RUST_AVAILABLE" = true ]; then
    echo ""
    echo "Setting up Rust environment..."
    cd rust-fuzzy-logic

    if [ -f "Cargo.toml" ]; then
        echo "Building Rust project..."
        cargo build
        echo -e "${GREEN}✓ Rust project built successfully${NC}"
    else
        echo -e "${YELLOW}! Cargo.toml not found. Skipping Rust setup.${NC}"
    fi

    cd ..
fi

# Summary
echo ""
echo "=================================="
echo "Setup Complete! 🎉"
echo "=================================="
echo ""
echo "Next steps:"
echo ""
echo "1. For Python:"
echo "   cd python-fuzzy-logic"
echo "   source venv/bin/activate"
echo "   python src/main.py"
echo ""

if [ "$RUST_AVAILABLE" = true ]; then
    echo "2. For Rust:"
    echo "   cd rust-fuzzy-logic"
    echo "   cargo run"
    echo ""
fi

echo "3. Read the documentation:"
echo "   - README.md for overview"
echo "   - STUDY_GUIDE.md for learning path"
echo "   - CHEATSHEET.md for quick reference"
echo ""
echo "4. Install VSCode extensions:"
echo "   - Python (ms-python.python)"
echo "   - Jupyter (ms-toolsai.jupyter)"
if [ "$RUST_AVAILABLE" = true ]; then
    echo "   - rust-analyzer (rust-lang.rust-analyzer)"
fi
echo ""
echo "Happy learning! 🧠✨"
