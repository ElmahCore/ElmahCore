using ElmahCore.DemoCore5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace ElmahCore.DemoCore5.Controllers
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
            /*_logger.LogTrace("Test");
            _logger.LogDebug("Test");
            _logger.LogError("Test");
            _logger.LogInformation("Test");
            _logger.LogWarning("Test");*/
            _logger.LogCritical(new InvalidOperationException("Test"), "Test");
            ElmahExtensions.RiseError(new Exception("test2"));
            var r = 0;
            // ReSharper disable once UnusedVariable
            // ReSharper disable once IntDivisionByZero
            var d = 100 / r;
            return View();
        }

        public IActionResult Privacy()
        {
            throw new Exception("Test");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
