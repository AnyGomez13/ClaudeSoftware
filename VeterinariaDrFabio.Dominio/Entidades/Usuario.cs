namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Cuenta única de acceso al sistema (RF-01, RNF-05, RN-01).
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash PBKDF2 de la contraseña, en base64 (SUP-D03).</summary>
    public string ContrasenaHash { get; set; } = string.Empty;

    /// <summary>Sal aleatoria en base64.</summary>
    public string Salt { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
