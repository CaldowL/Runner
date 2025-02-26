using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Runner
{
    public partial class CreateNewProject : Form
    {
        private Utils utils = new Utils();
        public bool check_ok = false;

        public string name = "";
        public string uuid = "";
        public string workdir = "";
        public string command = "";
        public bool autorun = false;

        public CreateNewProject()
        {
            InitializeComponent();
        }

        private void CreateNewProject_Load(object sender, EventArgs e)
        {
            label_UUID.Text = utils.getUUID();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text=="" || textBox3.Text == "")
            {
                MessageBox.Show("配置不可为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            check_ok = true;

            uuid = label_UUID.Text;
            name = textBox1.Text;            
            workdir = textBox2.Text;
            command = textBox3.Text;
            autorun = checkBox1.Checked;

            this.Close();
        }
    }
}
