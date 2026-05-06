using System;
using System.Reflection;
using System.Windows.Forms;

namespace YTPDeluxe
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Text = "About YTP++ Deluxe";
            lblProductName.Text = AssemblyProduct;
            lblVersion.Text = "Version " + AssemblyVersion;
            lblCopyright.Text = AssemblyCopyright;
            txtDescription.Text = "Automated YouTube Poop-style video generator for legacy and modern Windows desktops.";
        }

        private string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? "YTP++ Deluxe" : ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        private string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        private string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ? String.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
