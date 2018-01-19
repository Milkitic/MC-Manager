using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gm.Controllers
{
    public class FrontController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = "这里是主页";

            return RedirectToAction("login","front");
        }
        public IActionResult Login(string type)
        {
            if (Session("User") != null)
                return RedirectToAction("index", "console");
            if (type == "unlogin")
                ViewData["Message"] = "登录失效或未登录，请重新登录。";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string uname, string pword, string code, bool remember)
        {
            if (uname == ConsoleController.uname && pword == ConsoleController.pword)
            {
                SetSession("User", uname);
                return RedirectToAction("index", "console");
            }
            else
            {
                ViewData["Message"] = "用户名或密码错误。";
                return View();
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
            HttpContext.Session.SetString(name, value);
        }

        private IActionResult RedirectLogin(bool redirect = true)
        {
            if (redirect)
                return RedirectToAction("Login", "Front");
            else
                return RedirectToAction("Login", "Front");
        }
    }
}