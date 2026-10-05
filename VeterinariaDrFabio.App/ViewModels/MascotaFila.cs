using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Fila de la tabla de mascotas (P-05): Mascota, Especie, Propietario, Edad calculada y Estado.</summary>
public class MascotaFila
{
    public MascotaFila(Mascota mascota, string edad)
    {
        Mascota = mascota;
        Edad = edad;
    }

    public Mascota Mascota { get; }

    public int Id => Mascota.Id;

    public string Nombre => Mascota.Nombre;

    public string Especie => Mascota.Especie;

    public string Propietario => Mascota.Propietario?.NombreCompleto ?? string.Empty;

    /// <summary>Edad calculada con la fecha actual; nunca se guarda (RF-08).</summary>
    public string Edad { get; }

    public string Estado => Mascota.Activo ? "Activa" : "Inactiva";
}
