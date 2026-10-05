using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <inheritdoc cref="IRecordatorioRepository"/>
public class RecordatorioRepository : IRecordatorioRepository
{
    private readonly VeterinariaDbContext _contexto;

    public RecordatorioRepository(VeterinariaDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar(Recordatorio recordatorio)
    {
        _contexto.Recordatorios.Add(recordatorio);
        _contexto.SaveChanges();
    }

    public void Actualizar(Recordatorio recordatorio) => _contexto.GuardarModificacion(recordatorio);

    public List<Recordatorio> ListarPendientes() =>
        _contexto.Recordatorios
            .Include(r => r.Mascota!).ThenInclude(m => m.Propietario)
            .Where(r => r.Estado == "Pendiente")
            .OrderBy(r => r.FechaObjetivo)
            .ThenBy(r => r.Id)
            .ToList();
}
