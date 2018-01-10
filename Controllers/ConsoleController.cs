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
            ViewData["Message"] = "这里是主页";

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

            if (page == null || page < 1)
            {
                if (Session("PageCount") != null)
                    page = int.Parse(Session("PageCount"));
                else
                    page = 1;
            }
            int length = 16;
            ViewData["Message"] = "";
            try
            {
                files = new DirectoryInfo(root).GetFiles();
                int offset = length * ((int)page - 1);
                if (offset > files.Length - 1) throw new Exception();
                FileInfo[] files_new = new FileInfo[length];
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

            return View();
        }

        [HttpPost]
        public IActionResult Upload(string remark, FileViewModel file)
        {
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

        public IActionResult Command()
        {
            if (!Validate())
                return RedirectLogin();
            if (rm == null || rm.Status == false)
            {
                rm = new RealManager();
                try
                {
                    if (Directory.Exists("c:\\"))
                        rm.RealAction("systeminfo", "");
                    //rm.RealAction("systeminfo", "");
                    else
                    {
                        rm.RealAction("bash", "");
                        rm.SendMessage("cat /proc/version;lsb_release -a;exit");
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
        public IActionResult GetCommand()
        {
            return Json(new
            {
                code = "200",
                status = rm.Status,
                data = rm.OutputData.ToString(),
                error = rm.ErrorData.ToString()
            });
        }

        [HttpPost]
        public IActionResult SendCommand(string command)
        {
            try
            {
                rm.SendMessage(command);
                return Json(new
                {
                    code = "200",
                    message = "success"
                });
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
        private bool Validate()
        {
            if (Session("User") == null)
                return false;
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
    }
}
