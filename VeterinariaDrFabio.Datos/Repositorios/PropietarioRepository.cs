using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IPropietarioRepository"/>
public class PropietarioRepository : IPropietarioRepository
{
    private readonly VeterinariaDbContext _contexto;

    public PropietarioRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Propietario propietario)
    {
        _contexto.Propietarios.Add(propietario);
        _contexto.SaveChanges();
    }

    public void Actualizar(Propietario propietario) => _contexto.GuardarModificacion(propietario);

    public List<Propietario> Buscar(string texto)
    {
        var consulta = _contexto.Propietarios.Include(p => p.Mascotas).AsQueryable();

        var criterio = texto?.Trim();
        if (!string.IsNullOrEmpty(criterio))
        {
            consulta = consulta.Where(p => p.NombreCompleto.Contains(criterio) || p.Telefono.Contains(criterio));
        }

        return consulta.OrderBy(p => p.NombreCompleto).ToList();
    }

    public Propietario? ObtenerPorId(int id) =>
        _contexto.Propietarios.Include(p => p.Mascotas).FirstOrDefault(p => p.Id == id);
}
