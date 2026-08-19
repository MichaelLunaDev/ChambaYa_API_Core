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

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var usuario = _service.ValidarUsuario(request.Email, request.Clave);
            if (usuario != null)
            {
                return Ok(new ApiResponse<Usuario> { Success = true, Message = "Login exitoso", Data = usuario });
            }
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Credenciales inválidas o usuario inactivo" });
        }

        [HttpPost("Registro")]
        public IActionResult Registro([FromBody] RegistroRequest req)
        {
            try
            {
                bool resultado = _service.RegistrarUsuario(req);
                if (resultado)
                    return Ok(new ApiResponse<object> { Success = true, Message = "Registro exitoso" });
                return BadRequest(new ApiResponse<object> { Success = false, Message = "No se pudo registrar el usuario" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("MisPostulaciones/{idUsuario}")]
        public IActionResult MisPostulaciones(int idUsuario)
        {
            var lista = _service.ListarMisPostulaciones(idUsuario);
            return Ok(new ApiResponse<List<MisPostulacionesDTO>> { Success = true, Message = "OK", Data = lista });
        }

        [HttpGet("MisOfertasAdmin/{idUsuario}")]
        public IActionResult MisOfertasAdmin(int idUsuario)
        {
            var lista = _service.ListarMisOfertasAdmin(idUsuario);
            return Ok(new ApiResponse<List<MisOfertasAdminDTO>> { Success = true, Message = "OK", Data = lista });
        }

        [HttpGet("Candidatos/{idOferta}")]
        public IActionResult Candidatos(int idOferta)
        {
            var lista = _service.ListarCandidatosPorOferta(idOferta);
            return Ok(new ApiResponse<List<CandidatoDTO>> { Success = true, Message = "OK", Data = lista });
        }

        [HttpPost("EvaluarCandidato")]
        public IActionResult EvaluarCandidato([FromBody] EvaluarRequest req)
        {
            bool res = _service.EvaluarPostulante(req);
            if (res) return Ok(new ApiResponse<object> { Success = true, Message = "Evaluación guardada" });
            return BadRequest(new ApiResponse<object> { Success = false, Message = "No se pudo guardar la evaluación" });
        }

        [HttpGet("Oferta/{id}")]
        public IActionResult ObtenerOferta(int id)
        {
            var of = _service.ObtenerOferta(id);
            if(of != null) return Ok(new ApiResponse<Oferta> { Success = true, Message = "OK", Data = of });
            return NotFound(new ApiResponse<object> { Success = false, Message = "No encontrada" });
        }

        [HttpPut("Oferta")]
        public IActionResult ActualizarOferta([FromBody] Oferta oferta)
        {
            bool res = _service.ActualizarOferta(oferta);
            if(res) return Ok(new ApiResponse<object> { Success = true, Message = "Actualizada" });
            return BadRequest(new ApiResponse<object> { Success = false, Message = "No se pudo actualizar" });
        }

        [HttpDelete("Oferta/{id}")]
        public IActionResult EliminarOferta(int id)
        {
            bool res = _service.EliminarOferta(id);
            if(res) return Ok(new ApiResponse<object> { Success = true, Message = "Eliminada" });
            return BadRequest(new ApiResponse<object> { Success = false, Message = "No se pudo eliminar" });
        }
    }
}
