# CertificatePasswordRecovery

This repository holds the code for a simple tool that can be used to brute-force encryption keys to unlock private keys from .pfx files.

Publishing this in the hopes it'll be helpful to someone - and because it's easier to host samples here than on my blog :)

Note, that brute-forcing passwords is, in any case, very slow.

## Repository contents

This repository contains the following solutions:

* CertificatePasswordRecovery: The original password recovery tool by [boredwookie](https://github.com/boredwookie/)(?),
  * Documentation and the original download package available on his site: https://7thzero.com/blog/certificate-password-recovery-tool
* CertificatePasswordRecovery2: A modernized password recovery tool version by [koskila](https://github.com/koskila/) 
  * Core improvements:
    * Multi-threaded
    * NET5
    * Around 9-10 times performance improvement
  * See this post for details: [How to recover the private key of a (.pfx) certificate?](https://www.koskila.net/how-to-recover-the-private-key-of-a-pfx-certificate/)

## Latest Updates (2025)

* Upgraded project to .NET 8.0 for latest performance and security
* Full cross-compatibility with Windows 10 and Windows 11
* Modernized codebase to C# 10+ standards (file-scoped namespaces, nullable reference types, etc.)
* Cleaned and improved WinForms designer and resource files
* Added missing event handler stubs for error-free builds
* Improved solution file for Visual Studio 2022/2025 compatibility
* Updated project configuration for Any CPU and x86 platforms
* Enhanced code readability and maintainability
* Verified all resources and settings for modern Windows support

## Contact

* https://www.koskila.net/contact
* https://twitter.com/koskila

## Original author

* https://github.com/boredwookie
* https://7thzero.com/ 