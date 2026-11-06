namespace CertificatePasswordRecovery;

partial class CertPasswordRecoveryForm
{
    private void InitializeComponent() => BuildLayout();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            recoveryCancellation?.Cancel();
            progressTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
