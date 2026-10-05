using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Opción del selector de propietario en el formulario de mascota (P-06).</summary>
public class PropietarioOpcion
{
    public PropietarioOpcion(Propietario propietario)
    {
        Id = propietario.Id;
        Texto = propietario.Activo
            ? $"{propietario.NombreCompleto} — {propietario.Telefono}"
            : $"{propietario.NombreCompleto} — {propietario.Telefono} (inactivo)";
    }

    public int Id { get; }

    public string Texto { get; }
}
