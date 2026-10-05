using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Fila de la tabla de propietarios (P-03): Nombre, Teléfono, N.º de mascotas y Estado.</summary>
public class PropietarioFila
{
    public PropietarioFila(Propietario propietario)
    {
        Propietario = propietario;
    }

    public Propietario Propietario { get; }

    public int Id => Propietario.Id;

    public string NombreCompleto => Propietario.NombreCompleto;

    public string Telefono => Propietario.Telefono;

    public int NumeroMascotas => Propietario.Mascotas.Count;

    public string Estado => Propietario.Activo ? "Activo" : "Inactivo";
}
