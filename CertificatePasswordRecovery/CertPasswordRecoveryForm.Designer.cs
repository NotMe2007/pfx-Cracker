namespace CertificatePasswordRecovery;

partial class CertPasswordRecoveryForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        maxGenLbl = new System.Windows.Forms.Label();
        maxGenBx = new System.Windows.Forms.NumericUpDown();
        prefixLbl = new System.Windows.Forms.Label();
        suffixLbl = new System.Windows.Forms.Label();
        helpAboutLinkLbl = new System.Windows.Forms.LinkLabel();
        prefixBx = new System.Windows.Forms.TextBox();
        suffixBx = new System.Windows.Forms.TextBox();
        label1 = new System.Windows.Forms.Label();
        symbolSequenceBx = new System.Windows.Forms.TextBox();
        label2 = new System.Windows.Forms.Label();
        startStringLbl = new System.Windows.Forms.Label();
        startStringBx = new System.Windows.Forms.TextBox();
        recoverPasswordBtn = new System.Windows.Forms.Button();
        logPathLbl = new System.Windows.Forms.Label();
        logPathBx = new System.Windows.Forms.TextBox();
        logLevelLbl = new System.Windows.Forms.Label();
        logLevelBx = new System.Windows.Forms.ComboBox();
        pathToCertLbl = new System.Windows.Forms.Label();
        pathToCertBx = new System.Windows.Forms.TextBox();
        certBrowseBtn = new System.Windows.Forms.Button();
        progressBar = new System.Windows.Forms.ProgressBar();

        ((System.ComponentModel.ISupportInitialize)(maxGenBx)).BeginInit();
        SuspendLayout();

        // maxGenLbl
        maxGenLbl.AutoSize = true;
        maxGenLbl.Location = new System.Drawing.Point(12, 13);
        maxGenLbl.Name = "maxGenLbl";
        maxGenLbl.Size = new System.Drawing.Size(137, 13);
        maxGenLbl.TabIndex = 0;
        maxGenLbl.Text = "Max Generated Characters:";

        // maxGenBx
        maxGenBx.Location = new System.Drawing.Point(305, 11);
        maxGenBx.Maximum = new decimal(new int[] { 25, 0, 0, 0 });
        maxGenBx.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        maxGenBx.Name = "maxGenBx";
        maxGenBx.TabIndex = 1;
        maxGenBx.Value = new decimal(new int[] { 3, 0, 0, 0 });
        maxGenBx.ValueChanged += maxGenBx_ValueChanged;

        // prefixLbl
        prefixLbl.AutoSize = true;
        prefixLbl.Location = new System.Drawing.Point(12, 62);
        prefixLbl.Name = "prefixLbl";
        prefixLbl.Size = new System.Drawing.Size(66, 13);
        prefixLbl.TabIndex = 2;
        prefixLbl.Text = "Prefix String:";

        // suffixLbl
        suffixLbl.AutoSize = true;
        suffixLbl.Location = new System.Drawing.Point(12, 88);
        suffixLbl.Name = "suffixLbl";
        suffixLbl.Size = new System.Drawing.Size(66, 13);
        suffixLbl.TabIndex = 3;
        suffixLbl.Text = "Suffix String:";

        // helpAboutLinkLbl
        helpAboutLinkLbl.AutoSize = true;
        helpAboutLinkLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
        helpAboutLinkLbl.Location = new System.Drawing.Point(12, 303);
        helpAboutLinkLbl.Name = "helpAboutLinkLbl";
        helpAboutLinkLbl.Size = new System.Drawing.Size(85, 17);
        helpAboutLinkLbl.TabIndex = 4;
        helpAboutLinkLbl.TabStop = true;
        helpAboutLinkLbl.Text = "Help / about";
        helpAboutLinkLbl.Click += helpAboutLinkLbl_Click;

        // prefixBx
        prefixBx.Location = new System.Drawing.Point(243, 59);
        prefixBx.Name = "prefixBx";
        prefixBx.Size = new System.Drawing.Size(132, 20);
        prefixBx.TabIndex = 2;

        // suffixBx
        suffixBx.Location = new System.Drawing.Point(243, 85);
        suffixBx.Name = "suffixBx";
        suffixBx.Size = new System.Drawing.Size(132, 20);
        suffixBx.TabIndex = 3;

        // label1
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(12, 112);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(190, 13);
        label1.TabIndex = 7;
        label1.Text = "Symbol Sequence (Comma-separated):";

        // symbolSequenceBx
        symbolSequenceBx.Font = new System.Drawing.Font("Courier New", 8.25F);
        symbolSequenceBx.Location = new System.Drawing.Point(15, 129);
        symbolSequenceBx.Multiline = true;
        symbolSequenceBx.Name = "symbolSequenceBx";
        symbolSequenceBx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        symbolSequenceBx.Size = new System.Drawing.Size(360, 68);
        symbolSequenceBx.TabIndex = 4;
        symbolSequenceBx.Text = " ,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,0,1,2,3,4,5,6,7,8,9,!,@,#,$,%,^,&,*,(,),_,-,+,=,~";

        // label2
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(326, 204);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(0, 13);
        label2.TabIndex = 10;

        // startStringLbl
        startStringLbl.AutoSize = true;
        startStringLbl.Location = new System.Drawing.Point(12, 36);
        startStringLbl.Name = "startStringLbl";
        startStringLbl.Size = new System.Drawing.Size(76, 13);
        startStringLbl.TabIndex = 11;
        startStringLbl.Text = "Starting String:";

        // startStringBx
        startStringBx.Location = new System.Drawing.Point(243, 36);
        startStringBx.Name = "startStringBx";
        startStringBx.Size = new System.Drawing.Size(132, 20);
        startStringBx.TabIndex = 12;

        // recoverPasswordBtn
        recoverPasswordBtn.Location = new System.Drawing.Point(259, 300);
        recoverPasswordBtn.Name = "recoverPasswordBtn";
        recoverPasswordBtn.Size = new System.Drawing.Size(116, 23);
        recoverPasswordBtn.TabIndex = 13;
        recoverPasswordBtn.Text = "Recover Password";
        recoverPasswordBtn.UseVisualStyleBackColor = true;
        recoverPasswordBtn.Click += recoverPasswordBtn_Click;

        // logPathLbl
        logPathLbl.AutoSize = true;
        logPathLbl.Location = new System.Drawing.Point(12, 238);
        logPathLbl.Name = "logPathLbl";
        logPathLbl.Size = new System.Drawing.Size(84, 13);
        logPathLbl.TabIndex = 14;
        logPathLbl.Text = "Path to Log File:";

        // logPathBx
        logPathBx.Location = new System.Drawing.Point(102, 235);
        logPathBx.Name = "logPathBx";
        logPathBx.Size = new System.Drawing.Size(273, 20);
        logPathBx.TabIndex = 15;
        logPathBx.Text = "";

        // logLevelLbl
        logLevelLbl.AutoSize = true;
        logLevelLbl.Location = new System.Drawing.Point(12, 264);
        logLevelLbl.Name = "logLevelLbl";
        logLevelLbl.Size = new System.Drawing.Size(57, 13);
        logLevelLbl.TabIndex = 16;
        logLevelLbl.Text = "Log Level:";

        // logLevelBx
        logLevelBx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        logLevelBx.FormattingEnabled = true;
        logLevelBx.Items.AddRange(new object[] {
            "Off",
            "Success Only (Default)",
            "Every 10,000 + Success",
            "Everything"});
        logLevelBx.Location = new System.Drawing.Point(102, 261);
        logLevelBx.Name = "logLevelBx";
        logLevelBx.Size = new System.Drawing.Size(162, 21);
        logLevelBx.TabIndex = 18;
        logLevelBx.SelectedIndexChanged += logLevelBx_SelectedIndexChanged;

        // pathToCertLbl
        pathToCertLbl.AutoSize = true;
        pathToCertLbl.Location = new System.Drawing.Point(12, 209);
        pathToCertLbl.Name = "pathToCertLbl";
        pathToCertLbl.Size = new System.Drawing.Size(70, 13);
        pathToCertLbl.TabIndex = 19;
        pathToCertLbl.Text = "Path To Cert:";

        // pathToCertBx
        pathToCertBx.Location = new System.Drawing.Point(102, 206);
        pathToCertBx.Name = "pathToCertBx";
        pathToCertBx.Size = new System.Drawing.Size(206, 20);
        pathToCertBx.TabIndex = 20;

        // certBrowseBtn
        certBrowseBtn.Location = new System.Drawing.Point(314, 204);
        certBrowseBtn.Name = "certBrowseBtn";
        certBrowseBtn.Size = new System.Drawing.Size(61, 23);
        certBrowseBtn.TabIndex = 21;
        certBrowseBtn.Text = "Browse...";
        certBrowseBtn.UseVisualStyleBackColor = true;
        certBrowseBtn.Click += certBrowseBtn_Click;

        // progressBar
        progressBar.Location = new System.Drawing.Point(15, 329);
        progressBar.Name = "progressBar";
        progressBar.Size = new System.Drawing.Size(360, 23);
        progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
        progressBar.TabIndex = 22;
        progressBar.Visible = false;

        // CertPasswordRecoveryForm
        AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(389, 360);
        Controls.Add(progressBar);
        Controls.Add(certBrowseBtn);
        Controls.Add(pathToCertBx);
        Controls.Add(pathToCertLbl);
        Controls.Add(logLevelBx);
        Controls.Add(logLevelLbl);
        Controls.Add(logPathBx);
        Controls.Add(logPathLbl);
        Controls.Add(recoverPasswordBtn);
        Controls.Add(startStringBx);
        Controls.Add(startStringLbl);
        Controls.Add(label2);
        Controls.Add(symbolSequenceBx);
        Controls.Add(label1);
        Controls.Add(suffixBx);
        Controls.Add(prefixBx);
        Controls.Add(helpAboutLinkLbl);
        Controls.Add(suffixLbl);
        Controls.Add(prefixLbl);
        Controls.Add(maxGenBx);
        Controls.Add(maxGenLbl);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new System.Drawing.Size(395, 385);
        Name = "CertPasswordRecoveryForm";
        Text = "Certificate Password Recovery Tool";
        ((System.ComponentModel.ISupportInitialize)(maxGenBx)).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Label maxGenLbl;
    private System.Windows.Forms.NumericUpDown maxGenBx;
    private System.Windows.Forms.Label prefixLbl;
    private System.Windows.Forms.Label suffixLbl;
    private System.Windows.Forms.LinkLabel helpAboutLinkLbl;
    private System.Windows.Forms.TextBox prefixBx;
    private System.Windows.Forms.TextBox suffixBx;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox symbolSequenceBx;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label startStringLbl;
    private System.Windows.Forms.TextBox startStringBx;
    private System.Windows.Forms.Button recoverPasswordBtn;
    private System.Windows.Forms.Label logPathLbl;
    private System.Windows.Forms.TextBox logPathBx;
    private System.Windows.Forms.Label logLevelLbl;
    private System.Windows.Forms.ComboBox logLevelBx;
    private System.Windows.Forms.Label pathToCertLbl;
    private System.Windows.Forms.TextBox pathToCertBx;
    private System.Windows.Forms.Button certBrowseBtn;
    private System.Windows.Forms.ProgressBar progressBar;
}

