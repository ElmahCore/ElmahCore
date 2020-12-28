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
            ElmahExtensions.RiseError(new InvalidOperationException("This is the Test Exception from code."));

            _logger.LogTrace("Test");
            _logger.LogDebug("Test");
            _logger.LogError("Test");
            _logger.LogInformation("Test");
            _logger.LogWarning("Test");
            _logger.LogCritical(new InvalidOperationException("Test"), "Test");

            ElmahExtensions.RiseError(new NullReferenceException());

            if (DateTime.Now.Millisecond < 500)
            {
                string str = null;
                foreach (var cc in str)
                {
                }
            }

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
