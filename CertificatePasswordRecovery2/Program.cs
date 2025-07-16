using System;
using System.Threading;
using System.Windows.Forms;

namespace CertificatePasswordRecovery;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize(); // Modern WinForms bootstrap (.NET 6+)
        Application.Run(new CertPasswordRecoveryForm());
    }
}