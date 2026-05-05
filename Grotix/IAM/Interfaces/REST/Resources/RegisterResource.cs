using System.ComponentModel.DataAnnotations;

namespace GrotixBackend.IAM.Interfaces.REST.Resources;

public record RegisterResource(
    [Required]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El correo debe tener un formato tipo nombre@dominio.com")]
    string Email,

    [Required]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un carácter especial.")]
    string Password
);