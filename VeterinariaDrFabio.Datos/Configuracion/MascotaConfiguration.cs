using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Mascota (RF-05..RF-08, RN-02, RN-03, RN-06).</summary>
internal class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("Mascota");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FechaNacimiento).ComoFecha();
        builder.HasIndex(m => m.PropietarioId);
        builder.HasIndex(m => m.Activo);

        builder.HasOne(m => m.Propietario)
            .WithMany(p => p.Mascotas)
            .HasForeignKey(m => m.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
