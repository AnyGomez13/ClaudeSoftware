using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Pruebas.Dominio;

/// <summary>
/// Pruebas de las entidades y modelos del dominio (RN-02, RN-05, SUP-08, SUP-D04, SUP-D05, SUP-D06).
/// </summary>
public class ModelosTests
{
    [Fact]
    [Trait("Req", "RN-02")]
    public void RN02_UnPropietarioPuedeTenerVariasMascotas()
    {
        var propietario = new Propietario { Id = 1, NombreCompleto = "Ana Pérez", Telefono = "3001234567" };
        var rocky = new Mascota { Id = 1, PropietarioId = propietario.Id, Nombre = "Rocky", Propietario = propietario };
        var misu = new Mascota { Id = 2, PropietarioId = propietario.Id, Nombre = "Misu", Propietario = propietario };

        propietario.Mascotas.Add(rocky);
        propietario.Mascotas.Add(misu);

        Assert.Equal(2, propietario.Mascotas.Count);
        Assert.All(propietario.Mascotas, m => Assert.Same(propietario, m.Propietario));
    }

    [Fact]
    [Trait("Req", "RN-05")]
    public void RN05_MascotaNoTienePropiedadEdadPersistente()
    {
        Assert.Null(typeof(Mascota).GetProperty("Edad"));
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_LasEntidadesConBorradoLogicoNacenActivas()
    {
        Assert.True(new Usuario().Activo);
        Assert.True(new Veterinario().Activo);
        Assert.True(new Propietario().Activo);
        Assert.True(new Mascota().Activo);
    }

    [Fact]
    [Trait("Req", "SUP-D06")]
    public void SUPD06_RecordatorioNaceEnEstadoPendienteSinOrigen()
    {
        var recordatorio = new Recordatorio();

        Assert.Equal("Pendiente", recordatorio.Estado);
        Assert.Null(recordatorio.VacunacionId);
        Assert.Null(recordatorio.ProcedimientoId);
    }

    [Fact]
    [Trait("Req", "SUP-D04")]
    public void SUPD04_HistoriaClinicaNaceSinRegistros()
    {
        Assert.Empty(new HistoriaClinica().Registros);
    }

    [Fact]
    [Trait("Req", "SUP-D05")]
    public void SUPD05_CarnetDigitalNaceSinVacunas()
    {
        Assert.Empty(new CarnetDigital().Vacunas);
    }
}
