using ChambaYa_Solucion.Models;
using ChambaYa_Solucion.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChambaYa_Solucion.Controllers
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
                using (var cn = new Microsoft.Data.SqlClient.SqlConnection("Server=localhost\\SQLEXPRESS;Database=BD_ChambaYa;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    var cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_insertar_oferta", cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCategoria", oferta.IdCategoria);
                    cmd.Parameters.AddWithValue("@Titulo", oferta.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", oferta.Descripcion);
                    cmd.Parameters.AddWithValue("@Salario", oferta.Salario);
                    cmd.Parameters.AddWithValue("@Ubicacion", oferta.Ubicacion);
                    cmd.Parameters.AddWithValue("@Modalidad", oferta.Modalidad);
                    cmd.Parameters.AddWithValue("@Requisitos", oferta.Requisitos);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                    return Ok(new ApiResponse<object> { Success = true, Message = "Oferta Creada" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }
    }
}
