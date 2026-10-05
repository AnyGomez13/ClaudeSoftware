using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IProcedimientoRepository"/>
public class ProcedimientoRepository : IProcedimientoRepository
{
    private readonly VeterinariaDbContext _contexto;

    public ProcedimientoRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Procedimiento procedimiento)
    {
        _contexto.Procedimientos.Add(procedimiento);
        _contexto.SaveChanges();
    }

    public List<Procedimiento> ListarPorMascota(int mascotaId) =>
        _contexto.Procedimientos
            .AsNoTracking()
            .Include(p => p.Veterinario)
            .Where(p => p.MascotaId == mascotaId)
            .OrderBy(p => p.Fecha)
            .ThenBy(p => p.Id)
            .ToList();

    public List<Procedimiento> ListarConProximaFecha() =>
        _contexto.Procedimientos
            .Include(p => p.Mascota!).ThenInclude(m => m.Propietario)
            .Where(p => p.ProximaFechaRecomendada != null)
            .OrderBy(p => p.ProximaFechaRecomendada)
            .ThenBy(p => p.Id)
            .ToList();
}
