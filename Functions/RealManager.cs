using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using gm.BLL;
using gm.Models.Function;
using MySql.Data.MySqlClient;

namespace gm.Functions
{
    class RealManager
    {
        System.Timers.Timer timer = new System.Timers.Timer();

        public StringBuilder FullData { get; set; } = new StringBuilder();  // 实现ajax传递时首次传递这个
        public StringBuilder FullError { get; set; } = new StringBuilder();

        StringBuilder bufferedData = new StringBuilder();  // 用作缓存，以实现ajax传递
        StringBuilder bufferedError = new StringBuilder();
        
        public bool Status { get; set; } = false;

        public string bufferCmd = null;  // 缓存自动命令
        public bool isAutoCallBack = false;  // 为了区分内部调用
        public bool IsLoading { get; set; } = false;  // 判断是否控制台正在输出
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
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Process \"{proc.ProcessName}\" started.");
            Status = true;

            timer.Interval = 3000;
            timer.Elapsed += timer_tick;
            // 如果打开注释，则以同步方式执行命令，此例子中用Exited事件异步执行。  
            // proc.WaitForExit();
        }
        public void SendMessage(string message, bool autoCmd = false)
        {
            if (!autoCmd)
                proc_SendMsg(message);
            else
            {
                if (isAutoCallBack)
                {
                    proc_SendMsg(message);
                    bufferCmd = null;
                    isAutoCallBack = false;
                }
                else
                {
                    bufferCmd = message;
                    if (!timer.Enabled)
                        timer_tick(null, null);
                }
            }
        }

        private void proc_SendMsg(string message)
        {
            proc.StandardInput.Write(message + "\n");
        }
        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!IsLoading)
            {
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Started outputting.");
                IsLoading = true;
            }
            timer.Stop();
            timer.Start();
            //
            string data = e.Data;
            if (data == null)
                return;
            string subString = RemoveTimeAndInfo(data);
            if (subString.IndexOf("<") == 0)
                AddMessage(subString);
            //db.UseInsert("INSERT INTO tbl_console VALUES (@date, @cmd)", new MySqlParameter("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), new MySqlParameter("@cmd", data));
            FullData.AppendLine(data);
            bufferedData.AppendLine(data);
        }

        private void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!IsLoading)
            {
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Started outputting.");
                IsLoading = true;
            }
            timer.Stop();
            timer.Start();
            //
            string data = e.Data;
            FullError.AppendLine(data);
            bufferedError.AppendLine(data);
        }

        private void proc_Exited(object sender, EventArgs e)
        {
            // 执行结束后触发  
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Process \"{proc.ProcessName}\" exited.");
            Status = false;
        }
        private void timer_tick(object sender, EventArgs e)
        {
            if (e != null)
            {
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Stopped outputting.");
                IsLoading = false;
            }
            if (bufferCmd != null)
            {
                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Received an autoCmd: {bufferCmd.ToString()}.");
                isAutoCallBack = true;
                SendMessage(bufferCmd, true);
            }
            timer.Stop();
        }
        private void AddMessage(string line)
        {
            BLL_Chat bll_chat = new BLL_Chat();
            int pos = line.IndexOf("> ");
            string user = line.Substring(1, pos - 1);
            string message = line.Substring(pos + 2, line.Length - user.Length - 3);
            bll_chat.InsertChat(user, message);
            //MessageList.Insert(0, new ServerUser(DateTime.Now, user, message));
        }
        private string RemoveTimeAndInfo(string line)
        {
            try
            {
                int index = line.IndexOf("]:") + 2;
                return line.Substring(index, line.Length - index).Trim();
            }
            catch
            {
                return line;
            }
        }
        public string GetBufferData()
        {
            string tmp = bufferedData.ToString();
            bufferedData.Clear();
            return tmp;
        }
        public string GetBufferError()
        {
            string tmp = bufferedError.ToString();
            bufferedError.Clear();
            return tmp;
        }
    }
}
