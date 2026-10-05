using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IMascotaRepository"/>
public class MascotaRepository : IMascotaRepository
{
    private readonly VeterinariaDbContext _contexto;

    public MascotaRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Mascota mascota)
    {
        _contexto.Mascotas.Add(mascota);
        _contexto.SaveChanges();
    }

    public void Actualizar(Mascota mascota) => _contexto.GuardarModificacion(mascota);

    public List<Mascota> Buscar(string texto)
    {
        var consulta = _contexto.Mascotas.Include(m => m.Propietario).AsQueryable();

        var criterio = texto?.Trim();
        if (!string.IsNullOrEmpty(criterio))
        {
            consulta = consulta.Where(m =>
                m.Nombre.Contains(criterio) || m.Propietario!.NombreCompleto.Contains(criterio));
        }

        return consulta.OrderBy(m => m.Nombre).ToList();
    }

    public Mascota? ObtenerPorId(int id) =>
        _contexto.Mascotas.Include(m => m.Propietario).FirstOrDefault(m => m.Id == id);
}
