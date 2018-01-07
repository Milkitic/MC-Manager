using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace gm.Functions
{
    class RealManager
    {
        public StringBuilder OutputData { get; set; } = new StringBuilder();
        public StringBuilder ErrorData { get; set; } = new StringBuilder();
        public bool Status { get; set; } = false;

        Process proc;
        public void RealAction(string StartFileName, string StartFileArg)
        {
            if (proc != null)
            {
                proc.Kill();
                proc = null;
            }
            proc = new Process();
            proc.StartInfo.FileName = StartFileName;      // 命令  
            if (StartFileArg != "") proc.StartInfo.Arguments = StartFileArg;      // 参数  
            
            proc.StartInfo.CreateNoWindow = true;         // 不创建新窗口  
            //proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;  // 重定向输入  
            proc.StartInfo.RedirectStandardOutput = true; // 重定向标准输出  
            proc.StartInfo.RedirectStandardError = true;  // 重定向错误输出  
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
            proc.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);

            proc.EnableRaisingEvents = true;                      // 启用Exited事件  
            proc.Exited += new EventHandler(proc_Exited);   // 注册进程结束事件  

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            Status = true;

            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。  
            // proc.WaitForExit();
        }
        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            OutputData.AppendLine(data);
        }

        private void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            ErrorData.AppendLine(data);
        }

        private void proc_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发  
            Console.WriteLine("Exited.");
            Status = false;
        }
    }
}
