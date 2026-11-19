# TechNova Installation Guide

Complete step-by-step guide to install and run TechNova locally.

---

## Table of Contents

1. [System Requirements](#system-requirements)
2. [Prerequisites Installation](#prerequisites-installation)
3. [Clone Repository](#clone-repository)
4. [Configuration](#configuration)
5. [Database Setup](#database-setup)
6. [Running the Application](#running-the-application)
7. [Troubleshooting](#troubleshooting)
8. [Post-Installation](#post-installation)

---

## System Requirements

### Minimum Requirements
- **Operating System**: Windows 10+, macOS 10.15+, Ubuntu 20.04+
- **RAM**: 4GB
- **Disk Space**: 2GB free
- **Internet**: Required for NuGet packages and external services

### Recommended Requirements
- **Operating System**: Windows 11+, macOS 12+, Ubuntu 22.04 LTS
- **RAM**: 8GB+
- **Disk Space**: 5GB SSD
- **CPU**: Quad-core processor
- **GPU**: Not required

---

## Prerequisites Installation

### 1. Install .NET 9 SDK

#### Windows
```bash
# Download from Microsoft
https://dotnet.microsoft.com/en-us/download/dotnet/9.0

# Or use Chocolatey (if installed)
choco install dotnet-sdk-9.0
```

#### macOS
```bash
# Using Homebrew
brew install dotnet

# Or download directly
https://dotnet.microsoft.com/en-us/download/dotnet/9.0
```

#### Linux (Ubuntu)
```bash
# Add Microsoft package repository
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version latest

# Or using apt (Ubuntu 22.04+)
sudo apt-get update
sudo apt-get install dotnet-sdk-9.0
```

**Verify Installation**:
```bash
dotnet --version
# Should output: 9.0.0 or higher
```

---

### 2. Install MySQL 8.0+

#### Windows
1. Download MySQL Community Server: https://dev.mysql.com/downloads/mysql/
2. Run the installer and follow setup wizard
3. Choose "Developer Default" setup type
4. Configure MySQL Server as Windows Service
5. Set root password (e.g., `root`)
6. Complete installation

#### macOS
```bash
# Using Homebrew
brew install mysql

# Start MySQL
brew services start mysql

# Run initial setup
mysql_secure_installation
```

#### Linux (Ubuntu)
```bash
# Install MySQL
sudo apt-get update
sudo apt-get install mysql-server

# Run initial setup
sudo mysql_secure_installation

# Start service
sudo systemctl start mysql
sudo systemctl enable mysql
```

**Verify Installation**:
```bash
mysql --version
# Should output: mysql  Ver 8.0.x for ...
```

---

### 3. Install Visual Studio or VS Code

#### Option A: Visual Studio 2022 (Recommended)
1. Download: https://visualstudio.microsoft.com/vs/
2. Choose "ASP.NET and web development" workload
3. Include ".NET 9 runtime"
4. Complete installation

#### Option B: Visual Studio Code
1. Download: https://code.visualstudio.com/
2. Install extensions:
   - C# Dev Kit
   - C# Extensions
   - ASP.NET Core Test Explorer
   - Markdown Preview Enhanced

#### Option C: JetBrains Rider
1. Download: https://www.jetbrains.com/rider/
2. Configure .NET SDK 9.0
3. Install and activate license

---

### 4. Install Git

#### Windows
```bash
# Download from
https://git-scm.com/download/win

# Or use Chocolatey
choco install git
```

#### macOS
```bash
# Using Homebrew
brew install git
```

#### Linux (Ubuntu)
```bash
sudo apt-get install git
```

**Verify Installation**:
```bash
git --version
# Should output: git version 2.x.x or higher
```

---

## Clone Repository

### 1. Create Project Directory
```bash
mkdir ~/Projects
cd ~/Projects
```

### 2. Clone the Repository
```bash
git clone https://github.com/yourusername/TechNova.git
cd TechNova
```

### 3. Verify Repository Structure
```bash
# Windows PowerShell
dir

# macOS/Linux
ls -la

# Should show:
# - TechNova/
# - README.md
# - CONTRIBUTING.md
# - CHANGELOG.md
# - .gitignore
```

---

## Configuration

### 1. Open Project in IDE

#### Visual Studio
1. Open Visual Studio
2. File → Open → Project/Solution
3. Select `TechNova.slnx`
4. Wait for project to load and restore packages

#### VS Code
```bash
cd TechNova
code .
```

### 2. Update Connection String

**File**: `TechNova/TechNova/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=technovadb;User=root;Password=your_password;Port=3306"
  },
  "Smtp": {
	"Host": "smtp.gmail.com",
	"Port": 587,
	"FromName": "TechNova",
	"Username": "your-email@gmail.com",
	"Password": "your-app-password"
  },
  "Stripe": {
	"PublishableKey": "pk_test_YOUR_KEY_HERE",
	"SecretKey": "sk_test_YOUR_KEY_HERE"
  }
}
```

### 3. Configure Email (Optional)

To enable email notifications:

1. **Enable Gmail App Password**:
   - Go to Google Account: https://myaccount.google.com/
   - Security → 2-Step Verification (enable if not done)
   - Security → App passwords
   - Select "Mail" and "Windows Computer"
   - Copy the generated password

2. **Update `appsettings.Development.json`**:
```json
{
  "Smtp": {
	"Host": "smtp.gmail.com",
	"Port": 587,
	"FromName": "TechNova",
	"Username": "your-email@gmail.com",
	"Password": "your-16-character-app-password"
  }
}
```

### 4. Configure Stripe (Optional)

1. Create account: https://dashboard.stripe.com/register
2. Get API keys from Dashboard → API keys
3. Update `appsettings.Development.json`:

```json
{
  "Stripe": {
	"PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY",
	"SecretKey": "sk_test_YOUR_SECRET_KEY"
  }
}
```

---

## Database Setup

### 1. Create Database

**Option A: Using MySQL Command Line**
```bash
mysql -u root -p

# Enter password when prompted, then:
CREATE DATABASE technovadb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EXIT;
```

**Option B: Using MySQL Workbench**
1. Open MySQL Workbench
2. Create Connection (if not exists)
3. Right-click → New Schema
4. Name: `technovadb`
5. Character Set: `utf8mb4`
6. Collation: `utf8mb4_unicode_ci`
7. Apply

### 2. Apply Entity Framework Migrations

```bash
# Navigate to project directory
cd TechNova/TechNova

# Apply migrations
dotnet ef database update

# Output should show:
# Applying migration 20261002181345_InitialCreate...
# Applying migration 20261102082928_AddCheckoutDetails...
# Applying migration 20261119151733_AddSliders...
# Done
```

### 3. Verify Database Creation

**Using MySQL Command Line**:
```bash
mysql -u root -p technovadb

# Check tables
SHOW TABLES;

# Should display all TechNova tables
EXIT;
```

---

## Running the Application

### Method 1: Using Visual Studio (Windows)

1. Open `TechNova.slnx` in Visual Studio
2. Set `TechNova` as startup project
3. Press **F5** or click "Start Debugging"
4. Application launches at `https://localhost:7101`

### Method 2: Using VS Code

```bash
cd TechNova/TechNova
dotnet run
```

Navigate to: `https://localhost:7101`

### Method 3: Using Command Line

```bash
cd TechNova/TechNova

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run
```

### Method 4: Debug Mode

```bash
cd TechNova/TechNova
dotnet run --configuration Debug
```

---

## Troubleshooting

### Issue: "Database connection failed"

**Solution**:
```bash
# Verify MySQL is running
# Windows
Get-Service | Where-Object {$_.Name -like "*MySQL*"}

# macOS
brew services list

# Linux
systemctl status mysql

# Restart if needed
# Windows: Services → MySQL80 → Start
# macOS: brew services restart mysql
# Linux: sudo systemctl restart mysql
```

### Issue: "Connection string error"

**Check**:
1. Username is correct (`root`)
2. Password matches your MySQL password
3. Database name is `technovadb`
4. Port is 3306 (default)
5. No URL-encoded special characters

### Issue: "Port 7101 already in use"

**Solution**:
```bash
# Windows PowerShell - Find process using port
netstat -ano | findstr :7101

# Kill process
taskkill /PID <process_id> /F

# Or use different port
dotnet run --urls "https://localhost:7102"
```

### Issue: "Entity Framework migrations failed"

**Solution**:
```bash
# Check migration status
dotnet ef migrations list

# Remove last migration if needed
dotnet ef migrations remove

# Try update again
dotnet ef database update
```

### Issue: ".NET 9 not found"

**Solution**:
```bash
# Verify installation
dotnet --list-sdks

# If not showing .NET 9, reinstall SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0

# Set global.json to use .NET 9
dotnet new globaljson --sdk-version 9.0.0
```

### Issue: "NuGet package restore failed"

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# If still failing, check internet connection
# and try with verbose output
dotnet restore --verbosity diagnostic
```

---

## Post-Installation

### 1. Create Admin Account

1. Navigate to `https://localhost:7101/Account/Register`
2. Create account with email: `admin@technova.com`
3. Password: `Admin@123456` (for demo purposes)
4. Manually assign admin role to user:

```bash
mysql -u root -p technovadb

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES ('admin-role-id', 'Admin', 'ADMIN', NULL);

# Exit and run application
EXIT;
```

### 2. Explore Features

- **Homepage**: `https://localhost:7101/`
- **Products**: `https://localhost:7101/Products`
- **Admin Panel**: `https://localhost:7101/Admin` (requires admin login)
- **Account**: `https://localhost:7101/Account/Login`

### 3. Load Demo Data

Demo products and categories are automatically loaded from seeds during initial migration.

### 4. Enable Hot Reload (Optional)

```bash
# Install Hot Reload tool
dotnet tool install -g Microsoft.dotnet-hmr

# Run with hot reload
dotnet run --environment Development
```

### 5. Set Up IDE Debugging

#### Visual Studio
- Debug → Windows → Breakpoints
- Set breakpoints in code
- Press F5 to debug

#### VS Code
1. Install C# Dev Kit extension
2. Create `.vscode/launch.json`:
```json
{
	"version": "0.2.0",
	"configurations": [
		{
			"name": ".NET Core Launch",
			"type": "coreclr",
			"request": "launch",
			"preLaunchTask": "build",
			"program": "${workspaceFolder}/TechNova/TechNova/bin/Debug/net9.0/TechNova.dll",
			"args": [],
			"cwd": "${workspaceFolder}/TechNova/TechNova",
			"stopAtEntry": false,
			"console": "internalConsole"
		}
	]
}
```

---

## Verification Checklist

After installation, verify:

- [ ] .NET 9 SDK installed: `dotnet --version`
- [ ] MySQL running: `mysql --version`
- [ ] Git repository cloned
- [ ] `appsettings.Development.json` configured
- [ ] Database created and migrated
- [ ] Admin account created
- [ ] Application runs without errors
- [ ] Can navigate to homepage
- [ ] Database connection working
- [ ] Email configured (optional)
- [ ] Stripe configured (optional)

---

## Next Steps

1. **Read Documentation**: See [README.md](README.md)
2. **Explore Codebase**: Check project structure
3. **Run Tests**: `dotnet test`
4. **Create Feature Branch**: `git checkout -b feature/my-feature`
5. **Start Contributing**: See [CONTRIBUTING.md](CONTRIBUTING.md)

---

## Getting Help

- **Documentation**: Check [README.md](README.md)
- **Issues**: [GitHub Issues](https://github.com/yourusername/TechNova/issues)
- **Q&A**: [GitHub Discussions](https://github.com/yourusername/TechNova/discussions)
- **Email**: support@technova.com

---

<div align="center">

**Installation Complete! 🎉**

Ready to start developing? See [CONTRIBUTING.md](CONTRIBUTING.md)

[⬆ Back to Top](#technova-installation-guide)

</div>
