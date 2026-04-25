using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirmwareManager
{
    public partial class ProgressForm : Form
    {
        public event EventHandler CancelRequested;

        public ProgressForm()
        {

        }

        public ProgressForm(string title)
        {
            this.Text = title;
            InitializeComponent();
        }

        public void SetProgressText(string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }

        public void HideCancelButton()
        {
            cancelButton.Visible = false;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
