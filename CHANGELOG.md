# Version 2.0.0

Core improvements:
* Multi-threaded
* Built on NET5
* Around 9-10 times performance improvement
* See this post for details: [How to recover the private key of a (.pfx) certificate?](https://www.koskila.net/how-to-recover-the-private-key-of-a-pfx-certificate/)
  
---

# Version 3.0.0 (2025)

Major updates:
* Upgraded project to .NET 8.0 for latest performance and security
* Ensured full cross-compatibility with Windows 10 and Windows 11
* Modernized all code to C# 10+ standards (file-scoped namespaces, nullable reference types, etc.)
* Cleaned and improved WinForms designer and resource files
* Added missing event handler stubs for error-free builds
* Improved solution file for Visual Studio 2022/2025 compatibility
* Updated project configuration for Any CPU and x86 platforms
* Enhanced code readability and maintainability
* Verified all resources and settings for modern Windows support
* **Fixed certificate file browser** - Now shows only certificate files (.pfx, .p12, .cer) by default
* **Fixed log file access issues** - Applications now use safe default paths and handle permission errors gracefully
* Created user-friendly build and launch batch files for easy deployment
