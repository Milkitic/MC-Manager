using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using gm.Models;
using gm.Functions;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using gm.Models.Function;
using gm.BLL;

namespace gm.Controllers
{
    public class ConsoleController : Controller
    {
        static RealManager rm;
        static FileInfo[] files;

        public static string root = "/";
        public static string uname, pword;

        public IActionResult Index()
        {
            if (!Validate())
                return RedirectLogin();

            if (files == null) GetFiles();
            ViewData["Message"] = "这里是主页";
            ViewData["Current"] = "1";
            int count = 0;
            foreach (var item in files)
            {
                if (item.Extension == ".jar" || item.Extension == ".zip") count++;
            }
            ViewData["FileCount"] = count;
            return View();
        }
        public IActionResult LoginOut()
        {
            SetSession("User", null);

            return RedirectToAction("index", "front");
        }

        public IActionResult Manage(int? page)
        {
            if (!Validate())
                return RedirectLogin();
            ViewData["Current"] = "3";
            if (page == null || page < 1)
            {
                if (Session("PageCount") != null)
                    page = int.Parse(Session("PageCount"));
                else
                    page = 1;
            }
            int length = 10;
            ViewData["Message"] = "";
            try
            {
                FileInfo[] files_new = new FileInfo[length];
                GetFiles();
                int offset = length * ((int)page - 1);
                if (offset > files.Length - 1) throw new Exception();
                int j = 0;
                for (int i = offset; i < length + offset; i++)
                {
                    if (i > files.Length - 1) break;
                    files_new[j] = files[i];
                    j++;
                }
                ViewData["PageCount"] = (files.Length - 1) / length + 1;
                ViewData["CurrentPage"] = page;
                ViewData["PageLength"] = length;
                SetSession("PageCount", page.ToString());
                //files = new DirectoryInfo(@"C:\Users\YureruMiira\Desktop\新建文件夹\新建文件夹\.minecraft\mods").GetFiles();
                return View(files_new);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewData["Message"] = "Nothing Here.";
                return View();
            };
        }

        public IActionResult Modify(int id, bool enabled)
        {
            if (!Validate())
                return RedirectLogin();
            ViewData["Current"] = "3";
            try
            {
                FileInfo fi = files[id];
                if (enabled)
                {
                    fi.MoveTo(fi.FullName.Replace(".disabled", ""));
                    ViewData["Message"] = "已启用：" + fi.Name.Replace(".disabled", "");
                }
                else
                {
                    if (fi.Extension != ".disabled") fi.MoveTo(fi.FullName + ".disabled");
                    ViewData["Message"] = "已禁用：" + fi.Name.Replace(".disabled", "");
                }
                ViewData["Title"] = "Success";
            }
            catch (Exception ex)
            {
                ViewData["Message"] = ex.Message;
                ViewData["Title"] = "Failed";
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            if (!Validate())
                return RedirectLogin();
            ViewData["Current"] = "3";
            try
            {
                FileInfo fi = files[id];
                System.IO.File.Delete(fi.FullName);
                ViewData["Message"] = "已删除：" + fi.Name;
            }
            catch (Exception ex)
            {
                ViewData["Message"] = ex.Message;
                ViewData["Title"] = "Failed";
            }

            return View();
        }

        public IActionResult Upload()
        {
            if (!Validate())
                return RedirectLogin();
            ViewData["Message"] = "";
            ViewData["Current"] = "3";
            return View();
        }

        [HttpPost]
        public IActionResult Upload(string remark, FileViewModel file)
        {
            if (!Validate())
                return RedirectLogin();
            try
            {
                string fileName;
                string[] extensions = file.ModFile.FileName.Split('.');
                string extension = extensions[extensions.Length - 1];
                if (extension != "zip" && extension != "jar")
                    throw new Exception("不支持上传." + extension + "格式的文件。");

                if (remark == null || remark.Trim() == string.Empty)
                    fileName = file.ModFile.FileName;
                else
                    fileName = remark + "." + extension;
                foreach (var letter in fileName.ToCharArray())
                {
                    if (letter > 127)
                        throw new Exception("不支持上传含有非英文字符的的文件。");
                }
                if (System.IO.File.Exists(Path.Combine(root, fileName)))
                {
                    ViewData["Message"] = "上传失败：已存在文件" + fileName + "。请至管理页面删除。";
                    ViewData["State"] = "500";
                }
                else
                {
                    using (var stream = new FileStream(Path.Combine(root, fileName), FileMode.CreateNew))
                    {
                        file.ModFile.CopyTo(stream);
                    }
                    System.IO.File.SetLastWriteTime(Path.Combine(root, fileName), DateTime.Now);
                    ViewData["Message"] = "上传文件成功：" + fileName;
                    ViewData["State"] = "200";
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "上传失败：" + ex.Message;
                ViewData["State"] = "500";
            }
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Command
        public IActionResult Command()
        {
            if (!Validate())
                return RedirectLogin();
            ViewData["Current"] = "2";
            if (rm == null || rm.Status == false)
            {
                rm = new RealManager();
                try
                {
                    if (Directory.Exists(@"C:\"))
                        rm.RealAction("cmd", "");
                    //rm.RealAction("systeminfo", "");
                    else
                    {
                        string[] mcRoot = new string[2];
                        string[] tmpRoot = System.IO.File.ReadAllText("MC.conf").Split("|");
                        if (tmpRoot.Length != 2)
                            throw new Exception("运行时出错。");
                        else
                        {
                            mcRoot[0] = tmpRoot[0];
                            mcRoot[1] = tmpRoot[1];
                        }

                        try
                        {
                            rm.RealAction(mcRoot[0], mcRoot[1]);
                        }
                        catch (Exception)
                        {

                        }
                        //rm.RealAction("bash", "");
                        //rm.SendMessage("cat /proc/version;lsb_release -a;exit");
                    }

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        code = "-101",
                        message = ex.ToString()
                    });
                }
                rm.Status = true;
            }
            return View();
        }

        [HttpPost]
        public IActionResult GetFullCommand()
        {
            if (!Validate())
                return null;
            return Json(new
            {
                code = "200",
                status = rm.Status,
                data = rm.FullData.ToString(),
                error = rm.FullError.ToString()
            });
        }
        [HttpPost]
        public IActionResult GetCommand()
        {
            if (!Validate())
                return null;
            string data = rm.GetBufferData(), error = rm.GetBufferError();
            if (data.Trim() == "" && error.Trim() == "")
                return Json(new
                {
                    code = "000"
                });

            return Json(new
            {
                code = "200",
                status = rm.Status,
                data,
                error
            });
        }

        [HttpPost]
        public IActionResult SendCommand(string command, bool autoCmd = false)
        {
            if (!Validate())
                return null;
            try
            {
                rm.SendMessage(command, autoCmd);
                if (autoCmd)
                {
                    return Json(new
                    {
                        code = "200",
                        message = "success"
                    });
                }
                else
                {
                    return Json(new
                    {
                        code = "200",
                        message = "success"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = "-101",
                    message = ex.ToString()
                });
            }
        }

        // Message
        [HttpPost]
        public IActionResult GetMessages()
        {
            if (!Validate())
                return null;

            BLL_Chat bll_chat = new BLL_Chat();
            List<ServerUser> list = bll_chat.GetChatListOrderByTime(0, 10);

            return Json(new
            {
                code = "200",
                count = list.Count,
                data = list
            });

        }

        [HttpGet]
        public IActionResult MarkMessage(int id)
        {
            if (!Validate())
                return null;

            BLL_Chat bll_chat = new BLL_Chat();
            try
            {
                int result = bll_chat.MarkChatById(id);
                return Json(new
                {
                    code = "200"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = "-101",
                    message = ex.Message
                });
            }
        }

        public IActionResult MarkAllMessages()
        {
            if (!Validate())
                return null;

            BLL_Chat bll_chat = new BLL_Chat();
            try
            {
                int result = bll_chat.MarkAllChat();
                return Json(new
                {
                    code = "200"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    code = "-101",
                    message = ex.Message
                });
            }
        }

        private bool Validate()
        {
            if (Session("User") == null)
                return false;
            ViewData["User"] = Session("User");
            if (rm != null && rm.Status != false)
            {
                ViewData["Running"] = "";
            }
            return true;
        }

        private string Session(string name)
        {
            return HttpContext.Session.GetString(name);
        }
        private void SetSession(string name, string value)
        {
            if (value == null)
                HttpContext.Session.Remove(name);
            else
                HttpContext.Session.SetString(name, value);
        }

        private IActionResult RedirectLogin(bool redirect = true)
        {
            if (redirect)
            {
                Response.StatusCode = 403;
                return RedirectToAction("login", "front", new { type = "unlogin" });
            }
            else
                return RedirectToAction("login", "front");
        }

        private static void GetFiles()
        {
            files = new DirectoryInfo(root).GetFiles();
        }
    }
}
