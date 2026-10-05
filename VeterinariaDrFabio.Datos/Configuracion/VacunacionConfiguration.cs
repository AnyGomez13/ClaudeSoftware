using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Vacunacion (RF-11, RF-12, RN-07, RN-08).</summary>
internal class VacunacionConfiguration : IEntityTypeConfiguration<Vacunacion>
{
    public void Configure(EntityTypeBuilder<Vacunacion> builder)
    {
        builder.ToTable("Vacunacion");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.FechaAplicacion).ComoFecha();
        builder.Property(v => v.ProximaFecha).ComoFecha();
        builder.HasIndex(v => new { v.MascotaId, v.FechaAplicacion });
        builder.HasIndex(v => v.VeterinarioId);
        builder.HasIndex(v => v.ProximaFecha);

        builder.HasOne(v => v.Mascota)
            .WithMany(m => m.Vacunaciones)
            .HasForeignKey(v => v.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Veterinario)
            .WithMany()
            .HasForeignKey(v => v.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
