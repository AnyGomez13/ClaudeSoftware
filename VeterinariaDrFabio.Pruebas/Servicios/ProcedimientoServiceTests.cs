using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="ProcedimientoService"/> (RF-09, RN-07, RN-08, RNF-08).</summary>
public class ProcedimientoServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();
    private readonly int _mascotaId;
    private readonly int _fabioId;
    private readonly int _williamId;

    public ProcedimientoServiceTests()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        _mascotaId = DatosDePrueba.CrearMascota(_bd, ana.Id).Id;
        _fabioId = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        _williamId = DatosDePrueba.IdDeVeterinario(_bd, "William");
    }

    public void Dispose() => _bd.Dispose();

    private ProcedimientoService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new ProcedimientoService(
            new ProcedimientoRepository(contexto),
            new MascotaRepository(contexto),
            new VeterinarioRepository(contexto));
    }

    private int ContarProcedimientos() => _bd.Escalar<int>("SELECT COUNT(*) FROM Procedimiento;");

    private Procedimiento Nuevo() => new()
    {
        MascotaId = _mascotaId,
        VeterinarioId = _fabioId,
        Fecha = new DateTime(2026, 10, 4),
        TipoProcedimiento = "Consulta",
        Descripcion = "Control general",
    };

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_RegistrarGuardaElProcedimientoConSuVeterinario()
    {
        var procedimiento = Nuevo();
        procedimiento.VeterinarioId = _williamId;
        procedimiento.TipoProcedimiento = "  Desparasitación ";
        procedimiento.Tratamiento = "  ";
        procedimiento.PesoEnElMomento = 12.8;
        procedimiento.ProximaFechaRecomendada = new DateTime(2027, 1, 4);

        var resultado = CrearServicio().Registrar(procedimiento);

        Assert.True(resultado.Exito);
        Assert.True(procedimiento.Id > 0);
        using var lectura = _bd.CrearContexto();
        var guardado = lectura.Procedimientos.Single();
        Assert.Equal(_williamId, guardado.VeterinarioId);
        Assert.Equal("Desparasitación", guardado.TipoProcedimiento);
        Assert.Null(guardado.Tratamiento);
        Assert.Equal(12.8, guardado.PesoEnElMomento);
        Assert.Equal(new DateTime(2027, 1, 4), guardado.ProximaFechaRecomendada);
    }

    [Theory]
    [Trait("Req", "RN-07")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void RN07_SinVeterinarioAsignadoNoSeGuarda(int veterinarioId)
    {
        var procedimiento = Nuevo();
        procedimiento.VeterinarioId = veterinarioId;

        var resultado = CrearServicio().Registrar(procedimiento);

        Assert.False(resultado.Exito);
        Assert.Contains("veterinario", resultado.Mensaje);
        Assert.Equal(0, ContarProcedimientos());
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_UnVeterinarioInactivoNoPuedeSerAsignado()
    {
        _bd.Ejecutar("UPDATE Veterinario SET Activo = 0 WHERE NombreCompleto = 'William';");
        var procedimiento = Nuevo();
        procedimiento.VeterinarioId = _williamId;

        var resultado = CrearServicio().Registrar(procedimiento);

        Assert.False(resultado.Exito);
        Assert.Equal(0, ContarProcedimientos());
    }

    [Theory]
    [Trait("Req", "RNF-08")]
    [InlineData(0)]
    [InlineData(999)]
    public void RNF08_SinMascotaExistenteNoSeGuarda(int mascotaId)
    {
        var procedimiento = Nuevo();
        procedimiento.MascotaId = mascotaId;

        var resultado = CrearServicio().Registrar(procedimiento);

        Assert.False(resultado.Exito);
        Assert.Equal(0, ContarProcedimientos());
    }

    [Fact]
    [Trait("Req", "RF-09")]
    public void RF09_LaFechaEsObligatoriaYNoPuedeSerFutura()
    {
        var sinFecha = Nuevo();
        sinFecha.Fecha = default;
        var futura = Nuevo();
        futura.Fecha = DateTime.Today.AddDays(1);
        var hoy = Nuevo();
        hoy.Fecha = DateTime.Today;

        Assert.False(CrearServicio().Registrar(sinFecha).Exito);
        Assert.False(CrearServicio().Registrar(futura).Exito);
        Assert.True(CrearServicio().Registrar(hoy).Exito);
        Assert.Equal(1, ContarProcedimientos());
    }

    [Theory]
    [Trait("Req", "RF-09")]
    [InlineData("", "Control")]
    [InlineData("   ", "Control")]
    [InlineData("Consulta", "")]
    [InlineData("Consulta", "   ")]
    public void RF09_TipoYDescripcionSonObligatorios(string tipo, string descripcion)
    {
        var procedimiento = Nuevo();
        procedimiento.TipoProcedimiento = tipo;
        procedimiento.Descripcion = descripcion;

        Assert.False(CrearServicio().Registrar(procedimiento).Exito);
        Assert.Equal(0, ContarProcedimientos());
    }

    [Theory]
    [Trait("Req", "RN-06")]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(double.NaN)]
    public void RN06_UnPesoInformadoDebeSerMayorACero(double peso)
    {
        var procedimiento = Nuevo();
        procedimiento.PesoEnElMomento = peso;

        Assert.False(CrearServicio().Registrar(procedimiento).Exito);
        Assert.Equal(0, ContarProcedimientos());
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_LaProximaFechaNoPuedeSerAnteriorALaDelProcedimiento()
    {
        var procedimiento = Nuevo();
        procedimiento.ProximaFechaRecomendada = procedimiento.Fecha.AddDays(-1);

        var resultado = CrearServicio().Registrar(procedimiento);

        Assert.False(resultado.Exito);
        Assert.Equal(0, ContarProcedimientos());
    }
}
