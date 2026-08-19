using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChambaYa_Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class OfertaController : Controller
    {
        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Revisiones()
        {
            return View();
        }

        public IActionResult Publicaciones()
        {
            return View();
        }

        public IActionResult Candidatos(int id)
        {
            ViewBag.IdOferta = id;
            return View();
        }
    }
}
