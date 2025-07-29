# 🔒 SECURITY NOTICE - IMPORTANT FOR CERTIFICATE PASSWORD RECOVERY

## ⚠️ NEVER COMMIT THESE FILES TO GITHUB:

### 1. **Certificate Files** 
- `*.pfx` - PKCS#12 certificate files (contain private keys)
- `*.p12` - PKCS#12 certificate files (contain private keys) 
- `*.cer` - Certificate files
- `*.crt` - Certificate files
- `*.key` - Private key files
- `*.pem` - Privacy-Enhanced Mail files

**Why?** These files may contain private keys and sensitive certificate data.

### 2. **Log Files**
- `*.log` - May contain password attempts or recovered passwords
- `CertificatePasswordRecoveryLog.txt` - Application log file
- Any files ending in `Log.txt`

**Why?** Log files may contain sensitive information including:
- Password attempts during brute force
- Successfully recovered passwords
- Certificate file paths
- System information

### 3. **Build Artifacts**
- `bin/` - Compiled executables and libraries
- `obj/` - Build intermediate files
- `*.exe` - Executable files
- `*.dll` - Dynamic link libraries
- `*.pdb` - Debug symbol files

**Why?** These are generated files that can be rebuilt and take up unnecessary space.

### 4. **User-Specific Files**
- `.vs/` - Visual Studio cache
- `.vscode/` - VS Code settings
- `*.user` - User-specific project settings
- `*.suo` - Visual Studio solution user options

**Why?** These contain local development environment settings.

## ✅ WHAT IS SAFE TO COMMIT:

- Source code files (`.cs`, `.Designer.cs`)
- Project files (`.csproj`, `.sln`)
- Resource files (`.resx`)
- Documentation (`.md` files)
- Batch files (`.bat`)
- This `.gitignore` file

## 🛡️ BEST PRACTICES:

1. **Never test with real certificates** - Use test certificates only
2. **Clear log files** before development work
3. **Review commits** before pushing to ensure no sensitive data
4. **Use private repositories** for any work with real certificates
5. **Rotate passwords** if accidentally committed

## 🔍 CHECKING FOR SENSITIVE DATA:

Before committing, check for sensitive files:
```bash
# Check what files will be committed
git status

# Check for potential certificate files
git ls-files | grep -E "\.(pfx|p12|cer|crt|key|pem)$"

# Check for log files
git ls-files | grep -E "\.(log|txt)$"
```

## 🚨 IF YOU ACCIDENTALLY COMMIT SENSITIVE DATA:

1. **DO NOT PANIC** - but act quickly
2. **Remove from history** using `git filter-branch` or BFG Repo-Cleaner
3. **Force push** to overwrite remote history
4. **Rotate any compromised passwords/certificates**
5. **Consider the repository compromised** and create a new one if necessary

## 📞 CONTACT:

If you discover sensitive data in this repository, please contact the maintainers immediately.

---
**Remember: Security is everyone's responsibility!** 🔐
