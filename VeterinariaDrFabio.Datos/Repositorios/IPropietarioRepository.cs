using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de los propietarios (RF-02, RF-03, RF-04).</summary>
public interface IPropietarioRepository
{
    void Agregar(Propietario propietario);

    /// <summary>Guarda los cambios, incluida la inactivación (SUP-08).</summary>
    void Actualizar(Propietario propietario);

    /// <summary>
    /// Busca por nombre o teléfono (coincidencia parcial) e incluye las mascotas.
    /// Con texto vacío devuelve todos los propietarios, activos e inactivos.
    /// </summary>
    List<Propietario> Buscar(string texto);

    /// <summary>Devuelve el propietario con sus mascotas o nulo si no existe.</summary>
    Propietario? ObtenerPorId(int id);
}
