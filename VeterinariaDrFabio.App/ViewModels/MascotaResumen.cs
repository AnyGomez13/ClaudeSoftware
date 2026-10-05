using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Mascota del propietario seleccionado, tal como se lista bajo la tabla de P-03.</summary>
public class MascotaResumen
{
    public MascotaResumen(Mascota mascota)
    {
        Nombre = mascota.Nombre;
        Especie = mascota.Especie;
        Estado = mascota.Activo ? "Activa" : "Inactiva";
    }

    public string Nombre { get; }

    public string Especie { get; }

    public string Estado { get; }
}
