using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Fila de la lista de veterinarios (P-12): Nombre, Registro profesional y Estado.</summary>
public class VeterinarioFila
{
    public VeterinarioFila(Veterinario veterinario)
    {
        Veterinario = veterinario;
    }

    public Veterinario Veterinario { get; }

    public int Id => Veterinario.Id;

    public string NombreCompleto => Veterinario.NombreCompleto;

    public string RegistroProfesional =>
        string.IsNullOrEmpty(Veterinario.RegistroProfesional) ? "—" : Veterinario.RegistroProfesional;

    public string Estado => Veterinario.Activo ? "Activo" : "Inactivo";
}
