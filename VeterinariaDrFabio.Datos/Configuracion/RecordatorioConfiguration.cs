using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Recordatorio (RF-14, RF-15, SUP-D06).</summary>
internal class RecordatorioConfiguration : IEntityTypeConfiguration<Recordatorio>
{
    public void Configure(EntityTypeBuilder<Recordatorio> builder)
    {
        builder.ToTable("Recordatorio");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FechaObjetivo).ComoFecha();
        builder.Property(r => r.FechaEnvio).ComoMarcaTiempo();
        builder.HasIndex(r => r.Estado);
        builder.HasIndex(r => r.MascotaId);

        builder.HasOne(r => r.Mascota)
            .WithMany(m => m.Recordatorios)
            .HasForeignKey(r => r.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Vacunacion)
            .WithMany()
            .HasForeignKey(r => r.VacunacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Procedimiento)
            .WithMany()
            .HasForeignKey(r => r.ProcedimientoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
