namespace VeterinariaDrFabio.Dominio.Entidades;

/// <summary>
/// Dueño de una o varias mascotas (RF-02, RF-03, RF-04, RN-02, RN-04, RN-14).
/// </summary>
public class Propietario
{
    public int Id { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Celular colombiano de 10 dígitos que inicia en 3, sin prefijo +57 (SUP-D07).</summary>
    public string Telefono { get; set; } = string.Empty;

    public string? Documento { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    /// <summary>Borrado lógico; los propietarios no se eliminan (SUP-08).</summary>
    public bool Activo { get; set; } = true;

    public ICollection<Mascota> Mascotas { get; set; } = [];
}
