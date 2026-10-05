using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Configuracion;

/// <summary>Mapeo de la tabla Veterinario (R-03, RF-16, SUP-06).</summary>
internal class VeterinarioConfiguration : IEntityTypeConfiguration<Veterinario>
{
    public void Configure(EntityTypeBuilder<Veterinario> builder)
    {
        builder.ToTable("Veterinario");
        builder.HasKey(v => v.Id);
    }
}
