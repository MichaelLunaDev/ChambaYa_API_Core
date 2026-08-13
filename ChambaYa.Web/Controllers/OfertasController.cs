using Microsoft.AspNetCore.Mvc;
using ChambaYa.Web.Models;
using ChambaYa.Web.Services;

namespace ChambaYa.Web.Controllers
{
    public class OfertasController : Controller
    {
        private readonly OfertaApiService _service;

        public OfertasController(OfertaApiService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var ofertas = await _service.ListarAsync();
            return View(ofertas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OfertaModel oferta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Debug = "ModelState inválido: " + string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return View(oferta);
            }

            try
            {
                oferta.NombreCategoria = "Pendiente";
                var resultado = await _service.CrearAsync(oferta);

                ViewBag.Debug = $"Status HTTP: {resultado.StatusCode} | JSON crudo: {resultado.RawJson}";

                if (resultado.Success)
                {
                    TempData["Mensaje"] = "Oferta creada correctamente";
                    return RedirectToAction("Index");
                }

                return View(oferta);
            }
            catch (Exception ex)
            {
                ViewBag.Debug = "EXCEPCIÓN: " + ex.Message;
                return View(oferta);
            }
        }
    }
}