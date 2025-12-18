# 🐛 Troubleshooting & FAQ Guide

Common issues and solutions for the Fuzzy Logic Study Package.

## 📋 Table of Contents

- [Python Issues](#python-issues)
- [Rust Issues](#rust-issues)
- [VSCode Issues](#vscode-issues)
- [Platform-Specific Issues](#platform-specific-issues)
- [FAQ](#faq)

---

## Python Issues

### Issue: `ModuleNotFoundError: No module named 'skfuzzy'`

**Solution:**
```bash
# Make sure virtual environment is activated
source venv/bin/activate  # Mac/Linux
venv\Scripts\activate     # Windows

# Install dependencies
pip install -r requirements.txt

# If still failing, install directly
pip install scikit-fuzzy numpy matplotlib
```

### Issue: Virtual Environment Not Activating

**Windows PowerShell:**
```powershell
# Enable script execution
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned

# Then activate
.\venv\Scripts\Activate.ps1
```

**Mac/Linux:**
```bash
# Make sure the script is executable
chmod +x venv/bin/activate
source venv/bin/activate
```

### Issue: `matplotlib` Display Errors

**Linux (WSL/Headless):**
```bash
# Install tkinter
sudo apt-get install python3-tk

# Or use non-interactive backend
export MPLBACKEND=Agg
```

**Mac:**
```bash
# Install tkinter via brew
brew install python-tk
```

### Issue: Jupyter Notebook Won't Start

**Solution:**
```bash
# Install/reinstall jupyter
pip install --upgrade jupyter notebook ipykernel

# Register kernel
python -m ipykernel install --user --name=fuzzy_env

# Start notebook
jupyter notebook
```

---

## Rust Issues

### Issue: `cargo: command not found`

**Solution:**
```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Restart terminal or source the environment
source $HOME/.cargo/env
```

### Issue: Compilation Errors with `plotters`

**Solution:**
```toml
# In Cargo.toml, try different version
[dependencies]
plotters = "0.3.5"  # Specific stable version
```

Or remove plotting temporarily:
```bash
# Comment out plotters in Cargo.toml
# Remove visualization code from examples
```

### Issue: `rust-analyzer` Not Working in VSCode

**Solution:**
1. Install rust-analyzer extension from VSCode marketplace
2. Restart VSCode
3. Run command palette (Ctrl+Shift+P): "rust-analyzer: Restart Server"
4. Check Rust installation: `rustc --version`

### Issue: Slow Compilation Times

**Solution:**
```toml
# Add to Cargo.toml for faster debug builds
[profile.dev]
opt-level = 1  # Some optimization without full compile time
```

Or use `cargo check` instead of `cargo build` during development:
```bash
cargo check  # Fast syntax/type checking
```

---

## VSCode Issues

### Issue: Python Extension Not Detecting Virtual Environment

**Solution:**
1. Press `Ctrl+Shift+P` (Cmd+Shift+P on Mac)
2. Type "Python: Select Interpreter"
3. Choose the interpreter from `./venv/` directory
4. Restart VSCode if needed

### Issue: Code Runner Not Working

**Solution:**
Add to `.vscode/settings.json`:
```json
{
  "code-runner.executorMap": {
    "python": "python -u",
    "rust": "cargo run"
  },
  "code-runner.clearPreviousOutput": true
}
```

### Issue: IntelliSense/Autocomplete Not Working

**Python:**
```bash
# Reinstall language server
pip install --upgrade pylance
```

**Rust:**
```bash
# Update rust-analyzer
rustup update
# Restart VSCode
```

### Issue: Terminal Not Using Virtual Environment

**Solution:**
Add to `.vscode/settings.json`:
```json
{
  "python.terminal.activateEnvironment": true,
  "python.defaultInterpreterPath": "${workspaceFolder}/venv/bin/python"
}
```

---

## Platform-Specific Issues

### Windows

#### Issue: Long Path Errors

**Solution:**
```powershell
# Enable long paths (Run as Administrator)
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force

# Or move project closer to drive root
# e.g., C:\fuzzy-logic instead of C:\Users\...\Documents\...\fuzzy-logic
```

#### Issue: Line Ending Warnings (CRLF vs LF)

**Solution:**
```bash
# Configure Git for Windows
git config --global core.autocrlf true

# Or in VSCode settings.json
{
  "files.eol": "\n"
}
```

### Mac (Apple Silicon M1/M2)

#### Issue: Rust Build Errors on ARM

**Solution:**
```bash
# Make sure you have ARM version of Rust
rustup show

# If needed, reinstall for ARM
rustup self uninstall
# Then reinstall from https://rustup.rs
```

#### Issue: Python Library Compatibility

**Solution:**
```bash
# Use Rosetta or native ARM Python
# Check Python architecture
python -c "import platform; print(platform.machine())"

# Should show 'arm64' for native
```

### Linux

#### Issue: Permission Denied for Scripts

**Solution:**
```bash
chmod +x setup.sh
chmod +x venv/bin/activate
```

#### Issue: Missing Development Libraries

**Solution:**
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install python3-dev python3-pip python3-venv build-essential

# Fedora/RHEL
sudo dnf install python3-devel python3-pip gcc
```

---

## FAQ

### Q: Which language should I start with?

**A:**
- **New to fuzzy logic?** Start with Python - easier syntax, better visualization
- **Know fuzzy logic?** Try Rust for performance and type safety
- **Want both?** Use Python for learning, Rust for implementing production systems

### Q: Can I use this on VSCode Web/Codespaces?

**A:**
Yes! Python works perfectly. Rust may require additional setup:
```bash
# In codespace/container
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
source $HOME/.cargo/env
```

### Q: The visualizations aren't showing up

**A:**
```python
# Add this at the end of your script
import matplotlib.pyplot as plt
plt.show()  # Make sure to call this!

# Or for non-interactive environments
plt.savefig('output.png')
```

### Q: How do I add more membership functions?

**Python:**
```python
from skfuzzy import membership as mf

# scikit-fuzzy provides:
# - trimf (triangular)
# - trapmf (trapezoidal)
# - gaussmf (Gaussian)
# - gbellmf (Generalized bell)
# - sigmf (Sigmoid)

# Use them in fuzzy_utils.py
```

**Rust:**
```rust
// Add to src/membership.rs
impl MembershipFunction {
    pub fn sigmoid(x: f64, a: f64, c: f64) -> f64 {
        1.0 / (1.0 + (-a * (x - c)).exp())
    }
}
```

### Q: Can I export my fuzzy system for production use?

**A:**
Yes! For Python, use `pickle`:
```python
import pickle
# Save
with open('fuzzy_system.pkl', 'wb') as f:
    pickle.dump(fuzzy_system, f)
```

For Rust, compile as library and use in other projects:
```bash
cargo build --release
# Library will be in target/release/
```

### Q: How do I benchmark performance?

**A:**
Use the included tool:
```bash
cd python-fuzzy-logic
python tools/performance_benchmark.py
```

### Q: Are there more examples available?

**A:**
Create your own! Common applications:
- **Temperature control** - Already included
- **Tipping system** - Already included
- **Traffic light control** - Exercise for you
- **Image processing** - Edge detection with fuzzy logic
- **Decision systems** - Medical diagnosis, loan approval

### Q: My code is slow in Python

**A:**
1. Profile first: `python -m cProfile your_script.py`
2. Use NumPy vectorized operations
3. Consider implementing bottlenecks in Rust
4. Use PyPy instead of CPython for compute-heavy code

### Q: How do I contribute or report bugs?

**A:**
This is a learning project! Feel free to:
1. Modify any files for your learning
2. Add your own examples
3. Improve documentation
4. Share your improvements with others

### Q: Can I use this for my research/thesis?

**A:**
Yes! This package is educational. Just make sure to:
- Cite any papers/books you reference
- Validate your fuzzy system implementations
- Test thoroughly before using in academic work

---

## 🆘 Still Having Issues?

### Debug Checklist

1. **Check Python version:** `python --version` (Need 3.8+)
2. **Check Rust version:** `rustc --version` (Need 1.70+)
3. **Verify virtual environment:** `which python` (Should show venv path)
4. **Check installed packages:** `pip list` or `cargo tree`
5. **Try clean reinstall:**
   ```bash
   # Python
   rm -rf venv
   python -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt

   # Rust
   cargo clean
   cargo build
   ```

### Get Help

- Read the documentation in `README.md` and `STUDY_GUIDE.md`
- Check VSCode output panel for error details
- Search for error messages online
- Review the code comments for hints

### Common Error Patterns

| Error Message | Quick Fix |
|---------------|-----------|
| `No module named...` | Install with pip, check venv |
| `cargo: command not found` | Install Rust, restart terminal |
| `Permission denied` | Use `chmod +x` on Linux/Mac |
| `Cannot find binary...` | Run `cargo build` first |
| Display not showing | Check matplotlib backend |

---

**Remember:** Most issues are environment-related. When in doubt, try a clean install! 🔄
