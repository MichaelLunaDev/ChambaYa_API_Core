using System.ComponentModel.DataAnnotations;

namespace ChambaYa_Web.Models
{
    public class UsuarioModel
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Clave { get; set; } = string.Empty;
    }
}
