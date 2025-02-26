using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Runner
{
    public partial class LogReader : Form
    {
        private string file;
        public LogReader(string file)
        {
            this.file = file;
            InitializeComponent();
        }

        private void LogReader_Load(object sender, EventArgs e)
        {
            this.uiTextBox1.Text = File.ReadAllText(file).Replace("\n", Environment.NewLine);
        }

        private void uiTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
