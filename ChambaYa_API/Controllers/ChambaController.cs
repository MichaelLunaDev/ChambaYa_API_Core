using ChambaYa_API.Models;
using ChambaYa_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChambaYa_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChambaController : ControllerBase
    {
        private readonly INegocioService _service;

        public ChambaController(INegocioService service)
        {
            _service = service;
        }

        [HttpGet("Ofertas")]
        public IActionResult GetOfertas()
        {
            var lista = _service.ListarOfertasActivas();
            return Ok(new ApiResponse<List<Oferta>> { Success = true, Message = "Listado OK", Data = lista });
        }

        [HttpGet("Categorias")]
        public IActionResult GetCategorias()
        {
            var lista = _service.ListarCategorias();
            return Ok(new ApiResponse<List<Categoria>> { Success = true, Message = "Listado OK", Data = lista });
        }

        [HttpPost("ProcesarCarrito")]
        public IActionResult Postular([FromBody] PostulacionCarrito carrito)
        {
            if (carrito.OfertasSeleccionadas == null || !carrito.OfertasSeleccionadas.Any())
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "El carrito está vacío" });
            }

            bool resultado = _service.ProcesarCarritoPostulacion(carrito);

            if (resultado)
                return Ok(new ApiResponse<object> { Success = true, Message = "Postulaciones registradas exitosamente" });
            else
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al procesar la postulación (Rollback ejecutado)" });
        }

        [HttpPost("CrearOferta")]
        public IActionResult PostOferta([FromBody] Oferta oferta)
        {
            try
            {
                bool resultado = _service.CrearOferta(oferta);
                if (resultado)
                    return Ok(new ApiResponse<object> { Success = true, Message = "Oferta Creada" });
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al crear la oferta" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }
    }
}
