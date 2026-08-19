using Microsoft.AspNetCore.Mvc;
using ChambaYa.Web.Models;
using ChambaYa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Json;

namespace ChambaYa.Web.Controllers
{
    [Authorize]
    public class OfertasController : Controller
    {
        private readonly OfertaApiService _service;
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public OfertasController(OfertaApiService service, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _http = httpClientFactory.CreateClient("ChambaYaApi");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string buscar = "", string ubicacion = "", int pagina = 1)
        {
            var ofertas = await _service.ListarAsync();

            if (!string.IsNullOrEmpty(buscar))
                ofertas = ofertas.Where(o => o.Titulo.Contains(buscar, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(ubicacion))
                ofertas = ofertas.Where(o => o.Ubicacion.Contains(ubicacion, StringComparison.OrdinalIgnoreCase)).ToList();

            int pageSize = 10;
            int totalRecords = ofertas.Count;
            int totalPaginas = (int)Math.Ceiling(totalRecords / (double)pageSize);
            
            ofertas = ofertas.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Buscar = buscar;
            ViewBag.Ubicacion = ubicacion;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(ofertas);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(OfertaModel oferta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Debug = "ModelState inválido: " + string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(oferta);
            }

            try
            {
                oferta.NombreCategoria = "Pendiente";
                var resultado = await _service.CrearAsync(oferta);
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

        [HttpPost]
        [Authorize(Roles = "Postulante")]
        public async Task<IActionResult> PostulacionMasiva([FromBody] List<int> seleccionadas)
        {
            if (seleccionadas == null || !seleccionadas.Any())
                return Json(new { success = false, message = "No se seleccionaron ofertas." });

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
                return Json(new { success = false, message = "Usuario no válido." });

            var carrito = new {
                IdUsuario = idUsuario,
                OfertasSeleccionadas = seleccionadas
            };

            var response = await _http.PostAsJsonAsync("ProcesarCarrito", carrito);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
                return Json(new { success = result?.Success ?? true, message = result?.Message ?? "Postulaciones enviadas." });
            }

            return Json(new { success = false, message = "Error en el servidor al procesar las postulaciones." });
        }
    }
}
