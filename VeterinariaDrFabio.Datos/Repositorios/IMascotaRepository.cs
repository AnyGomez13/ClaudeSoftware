using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de las mascotas (RF-05, RF-06, RF-07).</summary>
public interface IMascotaRepository
{
    void Agregar(Mascota mascota);

    /// <summary>Guarda los cambios, incluida la inactivación (SUP-08).</summary>
    void Actualizar(Mascota mascota);

    /// <summary>
    /// Busca por nombre de la mascota o del propietario (coincidencia parcial) e incluye al propietario.
    /// Con texto vacío devuelve todas las mascotas.
    /// </summary>
    List<Mascota> Buscar(string texto);

    /// <summary>Devuelve la mascota con su propietario o nulo si no existe.</summary>
    Mascota? ObtenerPorId(int id);
}
