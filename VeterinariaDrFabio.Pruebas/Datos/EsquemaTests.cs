using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>
/// Verifica el esquema SQLite creado por la migración inicial: CHECK, FK, UNIQUE, triggers y seed
/// (RNF-03, RNF-07, RNF-08, RN-08, RN-12, RN-14, SUP-D01, SUP-D10).
/// </summary>
public class EsquemaTests : IDisposable
{
    private readonly BaseDatosTemporal _bd = new();

    public void Dispose() => _bd.Dispose();

    private void InsertarPropietario(string telefono = "3001234567") =>
        _bd.Ejecutar($"INSERT INTO Propietario (NombreCompleto, Telefono) VALUES ('Ana Perez', '{telefono}');");

    private void InsertarMascota() =>
        _bd.Ejecutar(
            "INSERT INTO Mascota (PropietarioId, Nombre, Especie, FechaNacimiento, Peso) " +
            "VALUES (1, 'Rocky', 'Perro', '2020-03-15', 12.5);");

    private void InsertarProcedimiento() =>
        _bd.Ejecutar(
            "INSERT INTO Procedimiento (MascotaId, VeterinarioId, Fecha, TipoProcedimiento, Descripcion) " +
            "VALUES (1, 1, '2026-10-04', 'Consulta', 'Control general');");

    private void InsertarVacunacion() =>
        _bd.Ejecutar(
            "INSERT INTO Vacunacion (MascotaId, VeterinarioId, NombreVacuna, FechaAplicacion) " +
            "VALUES (1, 2, 'Rabia', '2026-10-04');");

    private void InsertarCadenaCompleta()
    {
        InsertarPropietario();
        InsertarMascota();
        InsertarProcedimiento();
        InsertarVacunacion();
    }

    [Fact]
    [Trait("Req", "RNF-08")]
    public void RNF08_LasLlavesForaneasEstanActivas()
    {
        Assert.Equal(1, _bd.Escalar<int>("PRAGMA foreign_keys;"));
    }

    [Fact]
    [Trait("Req", "RN-12")]
    public void RN12_LaMigracionCreaLasSieteTablasDelDiseno()
    {
        var tablas = new[] { "Usuario", "Veterinario", "Propietario", "Mascota", "Procedimiento", "Vacunacion", "Recordatorio" };

        foreach (var tabla in tablas)
        {
            Assert.Equal(1, _bd.Escalar<int>($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tabla}';"));
        }
    }

    [Fact]
    [Trait("Req", "RN-14")]
    public void RN14_TelefonoColombianoValidoSeAcepta()
    {
        InsertarPropietario("3001234567");

        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Propietario;"));
    }

    [Theory]
    [Trait("Req", "RN-14")]
    [InlineData("2001234567")]
    [InlineData("300123456")]
    [InlineData("30012345678")]
    [InlineData("30012345ab")]
    [InlineData("573001234567")]
    [InlineData("")]
    public void RN14_TelefonoInvalidoSeRechaza(string telefono)
    {
        var error = Assert.Throws<SqliteException>(() => InsertarPropietario(telefono));

        Assert.Contains("CHECK constraint failed", error.Message);
    }

    [Fact]
    [Trait("Req", "RNF-08")]
    public void RNF08_NombreYTelefonoDelPropietarioSonObligatorios()
    {
        Assert.Throws<SqliteException>(() => _bd.Ejecutar("INSERT INTO Propietario (NombreCompleto) VALUES ('Ana');"));
        Assert.Throws<SqliteException>(() => _bd.Ejecutar("INSERT INTO Propietario (Telefono) VALUES ('3001234567');"));
    }

    [Fact]
    [Trait("Req", "RNF-08")]
    public void RNF08_MascotaSinPropietarioExistenteSeRechaza()
    {
        var error = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(
                "INSERT INTO Mascota (PropietarioId, Nombre, Especie, FechaNacimiento, Peso) " +
                "VALUES (999, 'Rocky', 'Perro', '2020-03-15', 12.5);"));

        Assert.Contains("FOREIGN KEY constraint failed", error.Message);
    }

    [Fact]
    [Trait("Req", "RNF-08")]
    public void RNF08_PesoDeLaMascotaDebeSerMayorACero()
    {
        InsertarPropietario();

        var error = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(
                "INSERT INTO Mascota (PropietarioId, Nombre, Especie, FechaNacimiento, Peso) " +
                "VALUES (1, 'Rocky', 'Perro', '2020-03-15', 0);"));

        Assert.Contains("CHECK constraint failed", error.Message);
    }

    [Fact]
    [Trait("Req", "RN-07")]
    public void RN07_ProcedimientoYVacunacionExigenUnVeterinarioExistente()
    {
        InsertarPropietario();
        InsertarMascota();

        var errorProcedimiento = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(
                "INSERT INTO Procedimiento (MascotaId, VeterinarioId, Fecha, TipoProcedimiento, Descripcion) " +
                "VALUES (1, 999, '2026-10-04', 'Consulta', 'Control');"));
        var errorVacunacion = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(
                "INSERT INTO Vacunacion (MascotaId, VeterinarioId, NombreVacuna, FechaAplicacion) " +
                "VALUES (1, 999, 'Rabia', '2026-10-04');"));

        Assert.Contains("FOREIGN KEY constraint failed", errorProcedimiento.Message);
        Assert.Contains("FOREIGN KEY constraint failed", errorVacunacion.Message);
    }

    [Fact]
    [Trait("Req", "RN-01")]
    public void RN01_NombreDeUsuarioEsUnico()
    {
        const string insertar = "INSERT INTO Usuario (NombreUsuario, ContrasenaHash, Salt) VALUES ('fabio', 'hash', 'sal');";
        _bd.Ejecutar(insertar);

        var error = Assert.Throws<SqliteException>(() => _bd.Ejecutar(insertar));

        Assert.Contains("UNIQUE constraint failed", error.Message);
    }

    [Theory]
    [Trait("Req", "RN-08")]
    [InlineData("Propietario")]
    [InlineData("Mascota")]
    [InlineData("Procedimiento")]
    [InlineData("Vacunacion")]
    public void RN08_NoSePermiteBorrarEnDuro(string tabla)
    {
        InsertarCadenaCompleta();

        var error = Assert.Throws<SqliteException>(() => _bd.Ejecutar($"DELETE FROM {tabla};"));

        Assert.Contains("no se elimin", error.Message);
        Assert.Equal(1, _bd.Escalar<int>($"SELECT COUNT(*) FROM {tabla};"));
    }

    [Fact]
    [Trait("Req", "SUP-06")]
    public void SUP06_LaMigracionSiembraAFabioYWilliamSinUsuario()
    {
        Assert.Equal(2, _bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario WHERE Activo = 1;"));
        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario WHERE NombreCompleto = 'Fabio';"));
        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Veterinario WHERE NombreCompleto = 'William';"));
        Assert.Equal(0, _bd.Escalar<int>("SELECT COUNT(*) FROM Usuario;"));
    }

    [Fact]
    [Trait("Req", "SUP-D06")]
    public void SUPD06_RecordatorioExigeExactamenteUnOrigen()
    {
        InsertarCadenaCompleta();
        const string columnas = "INSERT INTO Recordatorio (MascotaId, Tipo, FechaObjetivo, Mensaje, Enlace, VacunacionId, ProcedimientoId) ";

        var sinOrigen = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(columnas + "VALUES (1, 'Vacunacion', '2027-10-04', 'm', 'e', NULL, NULL);"));
        var dosOrigenes = Assert.Throws<SqliteException>(() =>
            _bd.Ejecutar(columnas + "VALUES (1, 'Vacunacion', '2027-10-04', 'm', 'e', 1, 1);"));
        _bd.Ejecutar(columnas + "VALUES (1, 'Vacunacion', '2027-10-04', 'm', 'e', 1, NULL);");

        Assert.Contains("CHECK constraint failed", sinOrigen.Message);
        Assert.Contains("CHECK constraint failed", dosOrigenes.Message);
        Assert.Equal(1, _bd.Escalar<int>("SELECT COUNT(*) FROM Recordatorio;"));
        Assert.Equal("Pendiente", _bd.Escalar<string>("SELECT Estado FROM Recordatorio;"));
    }

    [Fact]
    [Trait("Req", "SUP-D01")]
    public void SUPD01_LasFechasSeGuardanComoTextoIso8601YSeLeenDeVuelta()
    {
        var nacimiento = new DateTime(2020, 3, 15);
        var proxima = new DateTime(2027, 3, 15);
        var envio = new DateTime(2026, 10, 5, 14, 30, 0);

        using (var contexto = _bd.CrearContexto())
        {
            var veterinario = contexto.Veterinarios.OrderBy(v => v.Id).First();
            var procedimiento = new Procedimiento
            {
                Fecha = new DateTime(2026, 10, 4),
                TipoProcedimiento = "Desparasitación",
                Descripcion = "Control",
                ProximaFechaRecomendada = proxima,
                Veterinario = veterinario,
            };
            var mascota = new Mascota
            {
                Nombre = "Rocky",
                Especie = "Perro",
                FechaNacimiento = nacimiento,
                Peso = 12.5,
                Procedimientos = [procedimiento],
            };
            var propietario = new Propietario
            {
                NombreCompleto = "Ana Pérez",
                Telefono = "3001234567",
                Mascotas = [mascota],
            };
            mascota.Recordatorios.Add(new Recordatorio
            {
                Tipo = "Desparasitacion",
                FechaObjetivo = proxima,
                Mensaje = "m",
                Enlace = "e",
                FechaEnvio = envio,
                Procedimiento = procedimiento,
            });
            contexto.Propietarios.Add(propietario);
            contexto.SaveChanges();
        }

        Assert.Equal("2020-03-15", _bd.Escalar<string>("SELECT FechaNacimiento FROM Mascota;"));
        Assert.Equal("2026-10-04", _bd.Escalar<string>("SELECT Fecha FROM Procedimiento;"));
        Assert.Equal("2027-03-15", _bd.Escalar<string>("SELECT ProximaFechaRecomendada FROM Procedimiento;"));
        Assert.Equal("2026-10-05T14:30:00", _bd.Escalar<string>("SELECT FechaEnvio FROM Recordatorio;"));

        using var lectura = _bd.CrearContexto();
        var leida = lectura.Mascotas.Include(m => m.Procedimientos).Single();
        Assert.Equal(nacimiento, leida.FechaNacimiento);
        Assert.Equal(proxima, leida.Procedimientos.Single().ProximaFechaRecomendada);
        Assert.Equal(envio, lectura.Recordatorios.Single().FechaEnvio);
    }
}
