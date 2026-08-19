using System.ComponentModel.DataAnnotations;

namespace ChambaYa.Web.Models
{
    public class OfertaModel
    {
        public int IdOferta { get; set; }

        [Required(ErrorMessage = "Selecciona una categoría")]
        public int IdCategoria { get; set; }

        public string? NombreCategoria { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000, ErrorMessage = "Ingresa un salario válido")]
        public decimal Salario { get; set; }

        [Required]
        [StringLength(100)]
        public string Ubicacion { get; set; } = string.Empty;

        [Required]
        public string Modalidad { get; set; } = string.Empty;

        [Required]
        public string Requisitos { get; set; } = string.Empty;

        public DateTime FechaPublicacion { get; set; }
    }

    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public T? Data { get; set; }
    }
}