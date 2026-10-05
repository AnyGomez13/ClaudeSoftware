using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Propietario (RF-02..RF-04, RN-02, RN-04, RN-14).</summary>
internal class PropietarioConfiguration : IEntityTypeConfiguration<Propietario>
{
    public void Configure(EntityTypeBuilder<Propietario> builder)
    {
        builder.ToTable("Propietario");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.NombreCompleto);
        builder.HasIndex(p => p.Telefono);
    }
}
