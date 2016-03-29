using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSRDPNatTraverseClient
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public ProgressForm(string title, string content)
        {
            InitializeComponent();
            this.Text = title;
            this.contentLabel.Text = content;
        }
    }
}
