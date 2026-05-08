using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UI.Web.Models;

namespace UI.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("index")]
        [HttpGet("home")]
        public IActionResult Index()
        {
            _logger.LogInformation("[HOME] Index açıldı");
            return View();
        }

        // ✅ GİZLİLİK SAYFASI
        [HttpGet("privacy")]
        public IActionResult Privacy()
        {
            _logger.LogInformation("[HOME] Privacy açıldı");
            return View();
        }

        // ✅ HATA SAYFASI
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("[HOME] Error sayfası");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}