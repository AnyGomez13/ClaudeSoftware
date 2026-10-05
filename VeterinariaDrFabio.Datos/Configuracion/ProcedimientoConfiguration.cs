using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Procedimiento (RF-09, RF-10, RN-07, RN-08).</summary>
internal class ProcedimientoConfiguration : IEntityTypeConfiguration<Procedimiento>
{
    public void Configure(EntityTypeBuilder<Procedimiento> builder)
    {
        builder.ToTable("Procedimiento");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Fecha).ComoFecha();
        builder.Property(p => p.ProximaFechaRecomendada).ComoFecha();
        builder.HasIndex(p => new { p.MascotaId, p.Fecha });
        builder.HasIndex(p => p.VeterinarioId);
        builder.HasIndex(p => p.ProximaFechaRecomendada);

        builder.HasOne(p => p.Mascota)
            .WithMany(m => m.Procedimientos)
            .HasForeignKey(p => p.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Veterinario)
            .WithMany()
            .HasForeignKey(p => p.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
