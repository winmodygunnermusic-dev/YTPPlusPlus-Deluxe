namespace YTPDeluxe
{
    partial class AboutBox
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button okButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblProductName = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblProductName.Location = new System.Drawing.Point(18, 18);
            this.lblProductName.Size = new System.Drawing.Size(360, 22);
            this.lblProductName.Text = "YTP++ Deluxe";
            this.lblVersion.Location = new System.Drawing.Point(18, 48);
            this.lblVersion.Size = new System.Drawing.Size(360, 22);
            this.lblVersion.Text = "Version";
            this.lblCopyright.Location = new System.Drawing.Point(18, 78);
            this.lblCopyright.Size = new System.Drawing.Size(360, 22);
            this.lblCopyright.Text = "Copyright";
            this.txtDescription.Location = new System.Drawing.Point(21, 112);
            this.txtDescription.Multiline = true;
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(357, 90);
            this.okButton.Location = new System.Drawing.Point(303, 218);
            this.okButton.Size = new System.Drawing.Size(75, 26);
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            this.AcceptButton = this.okButton;
            this.ClientSize = new System.Drawing.Size(400, 260);
            this.Controls.Add(this.lblProductName);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
