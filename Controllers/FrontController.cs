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

            return View();
        }
        public IActionResult Login()
        {
            if (Session("id") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string code, bool remeberme = false)
        {
            return Json(new { a = "1" });
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

        private IActionResult RedirectLogin(bool redirect = true)
        {
            if (redirect)
                return RedirectToAction("Login", "Front");
            else
                return RedirectToAction("Login", "Front");
        }
    }
}