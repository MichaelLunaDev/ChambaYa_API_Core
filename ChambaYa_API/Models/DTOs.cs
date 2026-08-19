namespace ChambaYa_API.Models
{
    public class MisPostulacionesDTO
    {
        public string Titulo { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public decimal Salario { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? MensajeRespuesta { get; set; }
    }

    public class MisOfertasAdminDTO
    {
        public int IdOferta { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public bool Activa { get; set; }
        public int TotalPostulantes { get; set; }
    }

    public class CandidatoDTO
    {
        public int IdDetalle { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? MensajeRespuesta { get; set; }
    }

    public class EvaluarRequest
    {
        public int IdDetalle { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}
