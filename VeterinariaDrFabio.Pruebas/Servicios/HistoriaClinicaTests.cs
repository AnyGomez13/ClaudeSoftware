using System.Diagnostics;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Negocio.Servicios;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;
using Xunit.Abstractions;

namespace VeterinariaDrFabio.Pruebas.Servicios;

/// <summary>
/// Pruebas de la historia clínica armada por <see cref="MascotaService"/> y de la inmutabilidad del historial
/// (RF-06, RF-10, RN-07, RN-08, RNF-07, RNF-09).
/// </summary>
public class HistoriaClinicaTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();
    private readonly ITestOutputHelper _salida;

    public HistoriaClinicaTests(ITestOutputHelper salida)
    {
        _salida = salida;
    }

    public void Dispose() => _bd.Dispose();

    private MascotaService CrearServicio()
    {
        var contexto = _bd.CrearContexto();
        return new MascotaService(
            new MascotaRepository(contexto),
            new PropietarioRepository(contexto),
            new ProcedimientoRepository(contexto),
            new VacunacionRepository(contexto),
            new CalculadoraEdad());
    }

    private void RegistrarProcedimiento(int mascotaId, int veterinarioId, DateTime fecha, string tipo, string descripcion)
    {
        var contexto = _bd.CrearContexto();
        var servicio = new ProcedimientoService(
            new ProcedimientoRepository(contexto), new MascotaRepository(contexto), new VeterinarioRepository(contexto));
        Assert.True(servicio.Registrar(new Procedimiento
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            Fecha = fecha,
            TipoProcedimiento = tipo,
            Descripcion = descripcion,
        }).Exito);
    }

    private void RegistrarVacunacion(int mascotaId, int veterinarioId, DateTime fecha, string vacuna, DateTime? refuerzo = null)
    {
        var contexto = _bd.CrearContexto();
        var servicio = new VacunacionService(
            new VacunacionRepository(contexto), new MascotaRepository(contexto), new VeterinarioRepository(contexto));
        Assert.True(servicio.Registrar(new Vacunacion
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            NombreVacuna = vacuna,
            FechaAplicacion = fecha,
            ProximaFecha = refuerzo,
        }).Exito);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_LaHistoriaMezclaProcedimientosYVacunacionesOrdenadosPorFechaConSuVeterinario()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var misu = DatosDePrueba.CrearMascota(_bd, ana.Id, "Misu");
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        var william = DatosDePrueba.IdDeVeterinario(_bd, "William");
        RegistrarVacunacion(rocky.Id, william, new DateTime(2026, 6, 1), "Rabia", new DateTime(2027, 6, 1));
        RegistrarProcedimiento(rocky.Id, fabio, new DateTime(2026, 9, 10), "Control", "Sin novedades");
        RegistrarProcedimiento(rocky.Id, william, new DateTime(2026, 2, 1), "Consulta", "Primera consulta");
        RegistrarVacunacion(misu.Id, fabio, new DateTime(2026, 4, 1), "Triple felina");

        var historia = CrearServicio().ObtenerHistoriaClinica(rocky.Id)!;

        Assert.Equal("Rocky", historia.Mascota.Nombre);
        Assert.Equal(new DateTime(2026, 2, 1), historia.FechaApertura);
        Assert.Equal(
            [new DateTime(2026, 2, 1), new DateTime(2026, 6, 1), new DateTime(2026, 9, 10)],
            historia.Registros.Select(r => r.Fecha).ToList());
        Assert.Equal(["Procedimiento", "Vacunacion", "Procedimiento"], historia.Registros.Select(r => r.Origen).ToList());
        Assert.Equal(["William", "William", "Fabio"], historia.Registros.Select(r => r.Veterinario).ToList());
        Assert.Equal("Consulta: Primera consulta", historia.Registros[0].Detalle);
        Assert.Equal("Rabia. Próximo refuerzo: 01/06/2027", historia.Registros[1].Detalle);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_EnLaMismaFechaElProcedimientoVaAntesQueLaVacunacion()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var fabio = DatosDePrueba.IdDeVeterinario(_bd, "Fabio");
        var fecha = new DateTime(2026, 5, 5);
        RegistrarVacunacion(rocky.Id, fabio, fecha, "Rabia");
        RegistrarProcedimiento(rocky.Id, fabio, fecha, "Consulta", "Control");

        var historia = CrearServicio().ObtenerHistoriaClinica(rocky.Id)!;

        Assert.Equal(["Procedimiento", "Vacunacion"], historia.Registros.Select(r => r.Origen).ToList());
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_UnaMascotaSinRegistrosTieneHistoriaVaciaYUnaInexistenteDevuelveNulo()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id);
        var servicio = CrearServicio();

        var historia = servicio.ObtenerHistoriaClinica(rocky.Id)!;

        Assert.Empty(historia.Registros);
        Assert.Equal(default, historia.FechaApertura);
        Assert.Null(servicio.ObtenerHistoriaClinica(999));
    }

    [Fact]
    [Trait("Req", "RN-08")]
    public void RN08_NoExisteOperacionDeEdicionNiDeBorradoDeRegistrosClinicos()
    {
        var tipos = new[]
        {
            typeof(IProcedimientoService),
            typeof(IVacunacionService),
            typeof(IProcedimientoRepository),
            typeof(IVacunacionRepository),
        };
        var prohibidos = new[] { "Editar", "Actualizar", "Eliminar", "Borrar", "Remover", "Modificar" };

        foreach (var tipo in tipos)
        {
            var metodos = tipo.GetMethods().Select(m => m.Name);
            Assert.DoesNotContain(metodos, nombre => prohibidos.Any(p => nombre.Contains(p, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    [Trait("Req", "RNF-09")]
    public void RNF09_LaHistoriaDeUnaMascotaCon10000RegistrosSeObtieneEnMenosDeUnSegundo()
    {
        var ana = DatosDePrueba.CrearPropietario(_bd);
        var rocky = DatosDePrueba.CrearMascota(_bd, ana.Id, "Rocky");
        var otra = DatosDePrueba.CrearMascota(_bd, ana.Id, "Misu");
        _bd.Ejecutar(
            """
            WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n WHERE x < 5000)
            INSERT INTO Procedimiento (MascotaId, VeterinarioId, Fecha, TipoProcedimiento, Descripcion)
            SELECT 1, 1, date('2000-01-01', '+' || x || ' days'), 'Control', 'Registro ' || x FROM n;
            """);
        _bd.Ejecutar(
            """
            WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n WHERE x < 5000)
            INSERT INTO Vacunacion (MascotaId, VeterinarioId, NombreVacuna, FechaAplicacion)
            SELECT 1, 2, 'Vacuna ' || x, date('2000-01-01', '+' || x || ' days') FROM n;
            """);
        Assert.Equal(10000, _bd.Escalar<int>("SELECT (SELECT COUNT(*) FROM Procedimiento) + (SELECT COUNT(*) FROM Vacunacion);"));
        Assert.Equal(rocky.Id, _bd.Escalar<int>("SELECT MIN(MascotaId) FROM Procedimiento;"));

        // Se calienta el modelo de EF y la conexión con otra mascota para medir la consulta en régimen normal.
        CrearServicio().ObtenerHistoriaClinica(otra.Id);

        var reloj = Stopwatch.StartNew();
        var historia = CrearServicio().ObtenerHistoriaClinica(rocky.Id)!;
        reloj.Stop();
        _salida.WriteLine($"Historia de 10 000 registros obtenida en {reloj.ElapsedMilliseconds} ms.");

        Assert.Equal(10000, historia.Registros.Count);
        Assert.True(reloj.ElapsedMilliseconds < 1000, $"La historia tardó {reloj.ElapsedMilliseconds} ms.");
        Assert.Equal(historia.Registros.OrderBy(r => r.Fecha).Select(r => r.Fecha), historia.Registros.Select(r => r.Fecha));
    }
}
