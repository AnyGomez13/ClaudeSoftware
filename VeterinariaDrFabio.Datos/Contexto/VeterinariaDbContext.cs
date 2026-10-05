using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Contexto;

/// <summary>
/// Contexto de EF Core sobre la única base SQLite local (RNF-03, RN-12).
/// El esquema lo crea la migración inicial con el DDL del diseño; EF solo mapea.
/// </summary>
public class VeterinariaDbContext : DbContext
{
    public VeterinariaDbContext(DbContextOptions<VeterinariaDbContext> opciones)
        : base(opciones)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();

    public DbSet<Propietario> Propietarios => Set<Propietario>();

    public DbSet<Mascota> Mascotas => Set<Mascota>();

    public DbSet<Procedimiento> Procedimientos => Set<Procedimiento>();

    public DbSet<Vacunacion> Vacunaciones => Set<Vacunacion>();

    public DbSet<Recordatorio> Recordatorios => Set<Recordatorio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VeterinariaDbContext).Assembly);
    }
}
