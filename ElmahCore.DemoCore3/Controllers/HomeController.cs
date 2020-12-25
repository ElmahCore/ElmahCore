using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElmahCore.DemoCore3.Models;
using Microsoft.Extensions.Logging;

namespace ElmahCore.DemoCore3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            //for (int i= 0;i<=100; i++)
                ElmahExtensions.RiseError(new InvalidOperationException("test"));
            var r = 0;
            // ReSharper disable once UnusedVariable
            // ReSharper disable once IntDivisionByZero
            var d = 100 / r;
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
