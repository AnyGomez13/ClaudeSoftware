using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Opción del selector de veterinario en los formularios de procedimiento y vacunación (P-08, P-09).</summary>
public class VeterinarioOpcion
{
    public VeterinarioOpcion(Veterinario veterinario)
    {
        Id = veterinario.Id;
        Texto = veterinario.NombreCompleto;
    }

    public int Id { get; }

    public string Texto { get; }
}
