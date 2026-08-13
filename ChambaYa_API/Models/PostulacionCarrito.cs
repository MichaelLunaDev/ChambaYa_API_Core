namespace ChambaYa_API.Models
{
    public class PostulacionCarrito
    {
        public int IdUsuario { get; set; }
        public List<int> OfertasSeleccionadas { get; set; } = new List<int>();
    }
}
