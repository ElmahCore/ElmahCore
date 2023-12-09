using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using ElmahCore.DemoCore5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElmahCore.DemoCore5.Controllers
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
            //TestMethod("test", 100);


            _logger.LogTrace("Test");
            _logger.LogDebug("Test");
            _logger.LogError("Test");
            _logger.LogInformation("Test");
            _logger.LogWarning("Test");
            _logger.LogCritical(new InvalidOperationException("Test"), "Test");

            await this.HttpContext.RaiseError(new Exception("test2"));

            var r = 0;
            // ReSharper disable once UnusedVariable
            // ReSharper disable once IntDivisionByZero
            var d = 100 / r;
            return View();
        }

        public void TestMethod(string p1, int p2)
        {
            this.LogParams(this.HttpContext, (nameof(p1), p1), (nameof(p2), p2));

            using var connection = new SqlConnection("Server=.;Database=elmahtest;Trusted_Connection=True;");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP (@top) * FROM ELMAH_error";
            command.Parameters.Add("@top", SqlDbType.Int).Value = p2;
            command.ExecuteReader();
        }

        public IActionResult Privacy()
        {
            throw new Exception("Test");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}