using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using Newtonsoft.Json.Linq;
using Sunny.UI.Win32;
using System.Xml.Linq;
using System.Diagnostics;
using Newtonsoft.Json.Serialization;

namespace Runner
{
    struct RunJob
    {
        public string Uuid;
        public string Name;
        public string WorkDir;
        public string Command;
        public bool AutuRun;
        public bool Running;
    }


    public partial class MainForm : Form
    {
        private string configPath = $"{Common.appRoot}\\config.json";

        private Dictionary<string, RunJob> runJobDict = new Dictionary<string, RunJob>();
        private Dictionary<string, ProcessRunner> runProcessDict = new Dictionary<string, ProcessRunner>();
        private Dictionary<int, string> indexUuiTransformdDict = new Dictionary<int, string>();

        private bool allowClose = false;
        private bool appAutoStart = false;


        /***************************************************************************/
        private List<RunJob> works = new List<RunJob>();
        private Dictionary<string,RunJob> runJobMap = new Dictionary<string, RunJob>();

        private Dictionary<int, string> statusMap = new Dictionary<int, string>();
        private Dictionary<int, string> statusToUuidMap = new Dictionary<int, string>();
        private Dictionary<string, ProcessRunner> processMap = new Dictionary<string, ProcessRunner>();

        private static Utils utils = new Utils();



        // 托盘
        ToolStripMenuItem checkableAutoRunMenu;

        public MainForm(bool command)
        {
            InitializeComponent();
            appAutoStart = command;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkableAutoRunMenu = new ToolStripMenuItem("选项", null, OnCheckableMenuItemClick);

            string jsonString = File.ReadAllText(configPath);
            var jsonObj = JObject.Parse(jsonString);
            JValue autorun = (JValue)jsonObj["autorun"];

            checkableAutoRunMenu.Checked = (bool)autorun;
            this.contextMenuStrip1.Items.Add(checkableAutoRunMenu);

            loadJobs();
            autoRunJobs();
        }

        private int getIndexByUuid(string uuid)
        {
            int index = 0;
            while (true)
            {
                if (statusToUuidMap.ContainsKey(index))
                {
                    if (statusToUuidMap[index] == uuid)return index;
                    index++;
                }
                else
                {
                    return -1;
                }
            }
        }
        private void autoRunJobs()
        {
            DataGridView dgv = this.dataGridView1;
            foreach (RunJob job in works.ToArray())
            {
                if (job.AutuRun)
                {
                    createNewProcessObj(job.Uuid);
                    processMap[job.Uuid].Start();
                    int index = getIndexByUuid(job.Uuid);
                    dgv.Rows[index].Cells[4].Value = "运行中";
                    statusMap[index] = "运行中";
                }
            }
        }

        private void OnCheckableMenuItemClick(object sender, EventArgs e)
        {
            checkableAutoRunMenu.Checked = !checkableAutoRunMenu.Checked;

            string jsonString = File.ReadAllText(configPath);
            var jsonObj = JObject.Parse(jsonString);
            jsonObj["autorun"] = checkableAutoRunMenu.Checked;
            File.WriteAllText(configPath, jsonObj.ToString());
        }

        private void createNewProcessObj(string uuid)
        {
            try
            {
                RunJob job = runJobMap[uuid];
                ProcessRunner runner = new ProcessRunner(uuid, job.Command, job.WorkDir);
                runProcessDict[uuid] = runner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 装载json文件中的任务到 runJobDict
        /// </summary>
        private void loadJobs()
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var jsonObj = JObject.Parse(jsonString);
                JArray projects = (JArray)jsonObj["projects"];
                for (int i = 0; i < projects.Count; i++)
                {
                    var item = projects[i];
                    string uuid = (string)item["uuid"];
                    string name = (string)item["name"];
                    string workdir = (string)item["workdir"];
                    string command = (string)item["command"];
                    bool autorun = (bool)item["autorun"];
                    // addRowItem(uuid, name, workdir, command, autorun);
                    RunJob job = new RunJob
                    {
                        Uuid = uuid,
                        Name = name,
                        WorkDir = workdir,
                        Command = command,
                        AutuRun = autorun,
                        Running = false
                    };

                    /*
                    works.Add(job);
                    runJobMap[uuid] = job;
                    statusMap[i] = "未运行";
                    statusToUuidMap[i] = uuid;
                    */

                    runJobDict[uuid] = job;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取配置错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.Message);
            }
        }

        public void refreshDataGrid()
        {
            int index = 0;
            RunJob temp;
            foreach(var item in runJobDict)
            {
                temp = item.Value;
                indexUuiTransformdDict[index++] = temp.Uuid;
                addRowItem(temp.Uuid, temp.Name, temp.WorkDir, temp.Command, temp.AutuRun);
            }
            dataViewToEnd();
        }

        private bool writeAndSaveNewJobToConfig(string uuid, string name, string workdir, string command, bool auturun)
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var jsonObj = JObject.Parse(jsonString);
                JArray projects = (JArray)jsonObj["projects"];

                JObject jb = new JObject
                {
                    ["uuid"] = uuid,
                    ["name"] = name,
                    ["workdir"] = workdir,
                    ["command"] = command,
                    ["autorun"] = auturun
                };
                projects.Add(jb);
                jsonObj["projects"] = projects;
                File.WriteAllText(configPath, jsonObj.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private bool deleteJobFromConfig(int index)
        {
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var jsonObj = JObject.Parse(jsonString);
                JArray projects = (JArray)jsonObj["projects"];
                projects.RemoveAt(index);
                jsonObj["projects"] = projects;
                File.WriteAllText(configPath, jsonObj.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private void addRowItem(string uuid,string name, string workdir, string command, bool autorun)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);
            row.Cells[0].Value = (dataGridView1.Rows.Count + 1).ToString();
            row.Cells[1].Value = name;
            row.Cells[2].Value = workdir;
            row.Cells[3].Value = command;
            row.Cells[4].Value = "未运行";
            row.Cells[5].Value = "打开日志";
            row.Cells[6].Value = autorun;
            row.Tag = uuid;
            dataGridView1.Rows.Add(row);
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = false;
        }

        private void dataViewToEnd()
        {
            this.dataGridView1.FirstDisplayedScrollingRowIndex = this.dataGridView1.Rows.Count - 1;
        }


        /// <summary>
        /// 打开窗口，创建新任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            CreateNewProject ctn = new CreateNewProject();
            ctn.ShowDialog();
            if (!ctn.check_ok) return;
            if (writeAndSaveNewJobToConfig(ctn.uuid, ctn.name, ctn.workdir, ctn.command, ctn.autorun))
            {
                addRowItem(ctn.uuid,ctn.name, ctn.workdir, ctn.command, ctn.autorun);
                works.Add(new RunJob
                {
                    Uuid = ctn.uuid,
                    Name = ctn.name,
                    WorkDir = ctn.workdir,
                    Command = ctn.command,
                    AutuRun = ctn.autorun
                });
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (index < 0) return;
            string uuid = runJobDict[indexUuiTransformdDict[index]].Uuid;
            bool newAutorun = !works[index].AutuRun;
            try
            {
                string jsonString = File.ReadAllText(configPath);
                var jsonObj = JObject.Parse(jsonString);
                JArray projects = (JArray)jsonObj["projects"];
                for (int i = 0; i < projects.Count; i++)
                {
                    if ((string)projects[i]["uuid"] == uuid)
                    {
                        projects[i]["autorun"] = newAutorun;
                        break;
                    }
                }
                jsonObj["projects"] = projects;
                Console.WriteLine(jsonObj.ToString());
                File.WriteAllText(configPath, jsonObj.ToString());

                RunJob tp = runJobDict[uuid];
                tp.AutuRun = newAutorun;
                runJobDict[indexUuiTransformdDict[index]] = tp;
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取配置错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 仅允许开机自启配置项编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            Console.WriteLine(dgv.Columns[e.ColumnIndex].HeaderText);

            if (dgv.Columns[e.ColumnIndex].HeaderText != "开机自启")
            {
                e.Cancel = true;
            }
        }


        /// <summary>
        /// 操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            string key = indexUuiTransformdDict[e.RowIndex];

            if (dgv.Columns[e.ColumnIndex].HeaderText == "操作")
            {
                Console.WriteLine("点击打开日志");
                LogReader lr = new LogReader($"{Common.appRoot}\\log\\{key}.txt");
                lr.Show();
            }
            if (dgv.Columns[e.ColumnIndex].HeaderText == "运行状态")
            {
                Console.WriteLine("切换运行状态");

                if (!runJobDict[key].Running)
                {
                    dgv.Rows[e.RowIndex].Cells[4].Value = "运行中";
                    //runJobDict[key].Running = true;
                    createNewProcessObj(key);
                    runProcessDict[key].Start();
                }
                else{
                    dgv.Rows[e.RowIndex].Cells[4].Value = "未运行";
                    //runJobDict[key].Running = true;
                    // 结束进程
                    processMap[statusToUuidMap[e.RowIndex]].Stop();
                }
            }
        }

        /// <summary>
        /// 表格取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        /// <summary>
        /// 用于提交修改，使得每次变更数据均可接收到事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// 删除某个项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("当前未选中行", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("是否确认删除？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            int index = dataGridView1.SelectedRows[0].Index;
            try
            {
                dataGridView1.Rows.RemoveAt(index);
                deleteJobFromConfig(index);
                works.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show("删除失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true; // 取消关闭窗体 
                this.Hide();
                return;
            }
            foreach(RunJob job in works)
            {
                if (processMap.ContainsKey(job.Uuid))
                {
                    try
                    {
                        processMap[job.Uuid].Stop();
                    }catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                }

            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowClose = true;
            this.Close();
        }

        private void notifyIcon1_Click(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (this.appAutoStart) this.Hide();
        }
    }
}
