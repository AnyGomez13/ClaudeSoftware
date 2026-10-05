using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IVeterinarioRepository"/>
public class VeterinarioRepository : IVeterinarioRepository
{
    private readonly VeterinariaDbContext _contexto;

    public VeterinarioRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public List<Veterinario> ListarActivos() =>
        _contexto.Veterinarios.Where(v => v.Activo).OrderBy(v => v.NombreCompleto).ToList();

    public List<Veterinario> ListarTodos() =>
        _contexto.Veterinarios.OrderBy(v => v.NombreCompleto).ToList();

    public Veterinario? ObtenerPorId(int id) => _contexto.Veterinarios.Find(id);

    public void Agregar(Veterinario veterinario)
    {
        _contexto.Veterinarios.Add(veterinario);
        _contexto.SaveChanges();
    }

    public void Actualizar(Veterinario veterinario) => _contexto.GuardarModificacion(veterinario);
}
