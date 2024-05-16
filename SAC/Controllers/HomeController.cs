using Microsoft.AspNetCore.Mvc;
using SAC.Models;
using System.Diagnostics;
using SAC9;

namespace SAC.Controllers
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

            return View();
        }

        public IActionResult Source(string source )
        {
            tk tk_ = new tk();
             if (tk_.tree(source).Trim().Length==0)
            {
                return Ok(new { tree = "???", lex = tk_.lex(source) });
            }
            return Ok(new { tree =  tk_.tree(source), lex = tk_.lex(source) });
            
        }
            
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            int x=0, y=0; ;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
