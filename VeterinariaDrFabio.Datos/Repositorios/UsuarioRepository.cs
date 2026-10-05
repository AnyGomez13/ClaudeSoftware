using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IUsuarioRepository"/>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly VeterinariaDbContext _contexto;

    public UsuarioRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Usuario? ObtenerPorNombre(string nombreUsuario) =>
        _contexto.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

    public bool HayUsuarios() => _contexto.Usuarios.Any();

    public void Actualizar(Usuario usuario) => _contexto.GuardarModificacion(usuario);

    public void Agregar(Usuario usuario)
    {
        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();
    }
}
