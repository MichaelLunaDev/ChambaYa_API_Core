using Microsoft.AspNetCore.Mvc;

namespace ChambaYa_Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MisPostulaciones()
        {
            return View();
        }
    }
}
