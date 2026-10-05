using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IVacunacionRepository"/>
public class VacunacionRepository : IVacunacionRepository
{
    private readonly VeterinariaDbContext _contexto;

    public VacunacionRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Vacunacion vacunacion)
    {
        _contexto.Vacunaciones.Add(vacunacion);
        _contexto.SaveChanges();
    }

    public List<Vacunacion> ListarPorMascota(int mascotaId) =>
        _contexto.Vacunaciones
            .AsNoTracking()
            .Include(v => v.Veterinario)
            .Where(v => v.MascotaId == mascotaId)
            .OrderBy(v => v.FechaAplicacion)
            .ThenBy(v => v.Id)
            .ToList();

    public List<Vacunacion> ListarConProximaFecha() =>
        _contexto.Vacunaciones
            .Include(v => v.Mascota!).ThenInclude(m => m.Propietario)
            .Where(v => v.ProximaFecha != null)
            .OrderBy(v => v.ProximaFecha)
            .ThenBy(v => v.Id)
            .ToList();
}
