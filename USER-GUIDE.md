# Certificate Password Recovery Tool - User Guide

## Overview
This repository contains two versions of a certificate password recovery tool that can brute-force passwords for .pfx and .p12 certificate files.

## Project Status ✅
- **Framework**: .NET 8.0 (Latest)
- **Compatibility**: Windows 10 and Windows 11
- **Build Status**: Ready to use (both projects compile successfully)
- **Code Quality**: Modernized with C# 10+ standards

## Available Tools

### 1. CertificatePasswordRecovery (Original Improved)
- **Location**: `CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe`
- **Features**: Single-threaded brute force with modern .NET 8 improvements
- **Best for**: Simple password recovery tasks

### 2. CertificatePasswordRecovery2 (Multi-threaded) ⭐ **Recommended**
- **Location**: `CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe`
- **Features**: Multi-threaded, ~9-10x faster performance
- **Best for**: Complex password recovery tasks (recommended for most users)

## How to Use

### Quick Start (Recommended)
1. Double-click **`Quick-Launch.bat`**
2. Choose version 2 (multi-threaded) for best performance
3. The application will open with a user-friendly interface

### Build and Development
1. Double-click **`Build-and-Run.bat`** for full build options
2. Choose option 1 to build both projects
3. Use options 2-5 to run specific versions

### Manual Execution
You can also run the executables directly:
- `CertificatePasswordRecovery\bin\Release\net8.0-windows\CertificatePasswordRecovery.exe`
- `CertificatePasswordRecovery2\bin\Release\net8.0-windows\CertificatePasswordRecovery2.exe`

## Application Features

### User Interface
- **Certificate Path**: Browse and select your .pfx, .p12, or .cer file (file browser now filters to show only certificate files)
- **Starting String**: Optional known prefix for faster recovery
- **Prefix/Suffix**: Add known parts of the password
- **Max Generated Characters**: Set password length limit
- **Symbol Sequence**: Customize character set for brute force
- **Logging**: Track progress and results

### Logging Options
- **Off**: No logging
- **Success Only**: Log only when password is found
- **Every 10,000 + Success**: Log progress every 10,000 attempts
- **Everything**: Log all attempts (generates large files)

## Requirements
- **Operating System**: Windows 10 or Windows 11
- **.NET Runtime**: .NET 8.0 Desktop Runtime (usually pre-installed)
- **RAM**: Minimum 4GB recommended for multi-threaded version
- **CPU**: Multi-core processor recommended for version 2

## Performance Notes
- **Version 2 is significantly faster** due to multi-threading
- Password recovery time depends on:
  - Password complexity
  - Password length
  - Starting string accuracy
  - CPU performance
- **Warning**: Brute force password recovery can be very time-consuming

## Troubleshooting

### "Executable not found" Error
- Run `Build-and-Run.bat` and select option 1 to build the projects
- Ensure .NET 8.0 SDK is installed

### Application Won't Start
- Ensure .NET 8.0 Desktop Runtime is installed
- Check Windows compatibility (Windows 10/11 required)
- Run as administrator if needed

### Log File Access Issues ✅ **FIXED**
- Applications now automatically use safe default log paths (Documents folder)
- If log path is inaccessible, applications automatically fall back to temp folder
- No more crashes due to log file permission issues

### Build Errors
- Verify .NET 8.0 SDK is installed
- Check that all source files are present
- Use `Build-and-Run.bat` option 6 to clean and rebuild

## Legal Notice
This tool is for legitimate password recovery purposes only. Users are responsible for ensuring they have proper authorization to recover passwords for certificates they do not own.

## Support
For issues or questions:
- Check the error messages in the application
- Review the log files if logging is enabled
- Ensure certificate file is not corrupted
- Try different character sequences or starting strings

---
**Last Updated**: 2025 - Updated to .NET 8.0 with Windows 10/11 compatibility
