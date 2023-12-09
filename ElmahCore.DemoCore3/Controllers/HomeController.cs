﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ElmahCore.DemoCore3.Models;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            await this.HttpContext.RaiseError(new InvalidOperationException("This is the Test Exception from code."));

            _logger.LogTrace("Test");
            _logger.LogDebug("Test");
            _logger.LogError("Test");
            _logger.LogInformation("Test");
            _logger.LogWarning("Test");
            _logger.LogCritical(new InvalidOperationException("Test"), "Test");

            await this.HttpContext.RaiseError(new NullReferenceException());

            if (DateTime.Now.Millisecond < 500)
            {
                string str = null;
                // ReSharper disable once PossibleNullReferenceException
                foreach (var cc in str)
                {
                    Debug.WriteLine(cc);
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
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}