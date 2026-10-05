using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de los veterinarios (R-03, RF-16, SUP-06).</summary>
public interface IVeterinarioRepository
{
    /// <summary>Veterinarios activos, para los selectores de procedimiento y vacunación.</summary>
    List<Veterinario> ListarActivos();

    /// <summary>Todos los veterinarios con su estado, para la pantalla P-12.</summary>
    List<Veterinario> ListarTodos();

    Veterinario? ObtenerPorId(int id);

    void Agregar(Veterinario veterinario);

    void Actualizar(Veterinario veterinario);
}
