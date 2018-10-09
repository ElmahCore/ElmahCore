using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElmahCore.Demo.Models;

namespace ElmahCore.Demo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
	        throw new InvalidOperationException("Test");
            //return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
