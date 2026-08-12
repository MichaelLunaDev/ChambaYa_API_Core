namespace ChambaYa_Solucion.Models
{
    public class Oferta
    {
        public int IdOferta { get; set; }

        public int IdCategoria { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public string Requisitos { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Salario { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}