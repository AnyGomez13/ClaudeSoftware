using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>Pruebas de <see cref="MascotaService"/> (RF-05, RF-07, RF-08, RN-02, RN-03, RN-06, SUP-04, SUP-08).</summary>
public class MascotaServiceTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private MascotaService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new MascotaService(new MascotaRepository(contexto), new PropietarioRepository(contexto), new CalculadoraEdad());
    }

    private int ContarMascotas() => _bd.Escalar<int>("SELECT COUNT(*) FROM Mascota;");

    private static Mascota NuevaMascota(int propietarioId) => new()
    {
        PropietarioId = propietarioId,
        Nombre = "Rocky",
        Especie = "Perro",
        FechaNacimiento = new DateTime(2020, 3, 15),
        Peso = 12.5,
    };

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_RegistrarGuardaLaMascotaAsociadaAUnPropietarioExistente()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        mascota.Sexo = "macho";
        mascota.Raza = "  Criollo ";
        mascota.ColorSenas = "";

        var resultado = CrearServicio().Registrar(mascota);

        Assert.True(resultado.Exito);
        var guardada = CrearServicio().ObtenerPorId(mascota.Id)!;
        Assert.Equal("Ana Pérez", guardada.Propietario!.NombreCompleto);
        Assert.Equal("Macho", guardada.Sexo);
        Assert.Equal("Criollo", guardada.Raza);
        Assert.Null(guardada.ColorSenas);
        Assert.Equal(12.5, guardada.Peso);
    }

    [Fact]
    [Trait("Req", "RN-02")]
    public void RN02_UnPropietarioPuedeTenerVariasMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);

        Assert.True(CrearServicio().Registrar(NuevaMascota(ana.Id)).Exito);
        var segunda = NuevaMascota(ana.Id);
        segunda.Nombre = "Misu";
        Assert.True(CrearServicio().Registrar(segunda).Exito);

        Assert.Equal(2, new PropietarioService(new PropietarioRepository(_bd.CrearContexto())).ObtenerPorId(ana.Id)!.Mascotas.Count);
    }

    [Theory]
    [Trait("Req", "RN-03")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void RN03_SinPropietarioExistenteNoSeCreaLaMascota(int propietarioId)
    {
        var resultado = CrearServicio().Registrar(NuevaMascota(propietarioId));

        Assert.False(resultado.Exito);
        Assert.Contains("propietario", resultado.Mensaje);
        Assert.Equal(0, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_NombreYEspecieSonObligatorios()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var sinNombre = NuevaMascota(ana.Id);
        sinNombre.Nombre = "  ";
        var sinEspecie = NuevaMascota(ana.Id);
        sinEspecie.Especie = "";

        Assert.False(CrearServicio().Registrar(sinNombre).Exito);
        Assert.False(CrearServicio().Registrar(sinEspecie).Exito);
        Assert.Equal(0, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_LaFechaDeNacimientoEsObligatoriaYNoPuedeSerFutura()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var sinFecha = NuevaMascota(ana.Id);
        sinFecha.FechaNacimiento = default;
        var futura = NuevaMascota(ana.Id);
        futura.FechaNacimiento = DateTime.Today.AddDays(1);

        Assert.False(CrearServicio().Registrar(sinFecha).Exito);
        Assert.False(CrearServicio().Registrar(futura).Exito);
        Assert.Equal(0, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "SUP-04")]
    public void SUP04_UnaFechaDeNacimientoEstimadaSeAcepta()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        mascota.FechaNacimientoEstimada = true;

        Assert.True(CrearServicio().Registrar(mascota).Exito);

        Assert.True(CrearServicio().ObtenerPorId(mascota.Id)!.FechaNacimientoEstimada);
    }

    [Theory]
    [Trait("Req", "RN-06")]
    [InlineData(0)]
    [InlineData(-3.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RN06_UnPesoNoPositivoSeRechaza(double peso)
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        mascota.Peso = peso;

        var resultado = CrearServicio().Registrar(mascota);

        Assert.False(resultado.Exito);
        Assert.Contains("kilogramos", resultado.Mensaje);
        Assert.Equal(0, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "RF-05")]
    public void RF05_ElSexoSoloPuedeSerMachoOHembra()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        mascota.Sexo = "Otro";

        Assert.False(CrearServicio().Registrar(mascota).Exito);
        Assert.Equal(0, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "RF-08")]
    public void RF08_LaEdadSeCalculaAlConsultarYNoSeGuarda()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        mascota.FechaNacimiento = DateTime.Today.AddYears(-2).AddMonths(-3);
        CrearServicio().Registrar(mascota);

        var servicio = CrearServicio();
        var consultada = servicio.ObtenerPorId(mascota.Id)!;

        Assert.Equal(27, servicio.CalcularEdadMeses(consultada));
        Assert.Equal("2 años 3 meses", servicio.CalcularEdadLegible(consultada));
        Assert.Equal(0, _bd.Escalar<int>("SELECT COUNT(*) FROM pragma_table_info('Mascota') WHERE name = 'Edad';"));
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_EditarActualizaElPesoYLaDemasInformacion()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        CrearServicio().Registrar(mascota);

        var resultado = CrearServicio().Editar(new Mascota
        {
            Id = mascota.Id,
            PropietarioId = ana.Id,
            Nombre = "Rocky",
            Especie = "Perro",
            FechaNacimiento = new DateTime(2020, 3, 15),
            Peso = 13.2,
            ColorSenas = "Mancha blanca en el pecho",
            Activo = true,
        });

        Assert.True(resultado.Exito);
        var editada = CrearServicio().ObtenerPorId(mascota.Id)!;
        Assert.Equal(13.2, editada.Peso);
        Assert.Equal("Mancha blanca en el pecho", editada.ColorSenas);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_EditarConPesoInvalidoOMascotaInexistenteDevuelveErrorYConservaLoAnterior()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        CrearServicio().Registrar(mascota);
        var cambio = NuevaMascota(ana.Id);
        cambio.Id = mascota.Id;
        cambio.Peso = 0;
        var inexistente = NuevaMascota(ana.Id);
        inexistente.Id = 999;

        Assert.False(CrearServicio().Editar(cambio).Exito);
        Assert.False(CrearServicio().Editar(inexistente).Exito);
        Assert.Equal(12.5, CrearServicio().ObtenerPorId(mascota.Id)!.Peso);
    }

    [Fact]
    [Trait("Req", "SUP-08")]
    public void SUP08_InactivarNoBorraALaMascota()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var mascota = NuevaMascota(ana.Id);
        CrearServicio().Registrar(mascota);
        var cambio = NuevaMascota(ana.Id);
        cambio.Id = mascota.Id;
        cambio.Activo = false;

        Assert.True(CrearServicio().Editar(cambio).Exito);

        var inactiva = Assert.Single(CrearServicio().Buscar("Rocky"));
        Assert.False(inactiva.Activo);
        Assert.Equal(1, ContarMascotas());
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_BuscarSinResultadosDevuelveListaVaciaYPorPropietarioEncuentraSusMascotas()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd, "Ana Pérez");
        CrearServicio().Registrar(NuevaMascota(ana.Id));
        var servicio = CrearServicio();

        Assert.Empty(servicio.Buscar("Zzz"));
        Assert.Equal("Rocky", Assert.Single(servicio.Buscar("Pérez")).Nombre);
    }
}
