using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace gm.Controllers
{
    public class ErrorController : Controller
    {
        [Route("errors/{statusCode}")]
        public IActionResult CustomError(int statusCode)
        {
            Response.StatusCode = statusCode;
            ViewData["Code"] = statusCode.ToString();
            if (statusCode == 404)
            {
                ViewData["Message"] = "Oops，找不到页面";
            }
            else
            {
                ViewData["Message"] = statusCode.ToString();
            }
            return View("~/Views/Errors/ErrorPage.cshtml");
        }
    }
}