using Sunny.UI;
using Sunny.UI.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Runner
{
    public class ProcessRunner
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]

        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes type);

        // 控制消息
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private enum ConsoleCtrlEvent
        {
            CTRL_C = 0,
            CTRL_BREAK = 1
        }

        private string logFile = "";

        private Process process;
        private bool running = false;
        private string uuid_job = "";

        public ProcessRunner(string uuid, string command, string workdir)
        {
            uuid_job = uuid;
            logFile = $"{Common.appRoot}\\log\\{uuid}.txt";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command, // /c 参数表示执行完命令后关闭 CMD 窗口
                RedirectStandardOutput = true, // 重定向标准输出
                RedirectStandardError = true, // 重定向错误输出
                RedirectStandardInput = true,
                UseShellExecute = false, // 必须禁用使用操作系统 shell 执行
                CreateNoWindow = true, // 不创建窗口,
                WorkingDirectory = workdir
            };
            startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";

            process = new Process
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, arg) => recordLog(arg.Data);
            process.ErrorDataReceived += (sender, arg) => recordLog(arg.Data, true);
            process.Exited += (sender, args) => running = false;
        }

        public void Start()
        {
            process.Start();
            try
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }catch (Exception ex) { }
            running = true;
        }

        public void Stop()
        {
            // Attach to the process console
            AttachConsole(process.Id);
            SetConsoleCtrlHandler(null, true);
            GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C, 0);
            FreeConsole(); // Detach from the console


            process.Kill();
            process.WaitForExit(800);
        }

        public bool IsRunning()
        {
            return running;
        }

        private void recordLog(string message,bool isError=false)
        {
            try
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.AppendAllText(logFile, time + " [info] " + message + "\n");

                /*
                FileStream _file = new FileStream(logFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                using (StreamWriter writer = new StreamWriter(_file))
                {
                    writer.WriteLine(time + " [info] " + message + "\n");
                    writer.Flush();
                    writer.Close();

                    _file.Close();
                }
                */
                if (!isError)
                {
                    Console.WriteLine(message);
                }
                else
                {
                    Console.WriteLine("error" + message);
                }
            }catch (Exception ex)
            {
                Console.WriteLine (ex.ToString());
            }
        }
    }
}
