using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VeterinariaDrFabio.Datos.Contexto;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>
/// Base SQLite temporal con la migración inicial aplicada; se borra al terminar la prueba.
/// </summary>
public sealed class BaseDatosTemporal : IDisposable
{
    private readonly string _directorio;

    public BaseDatosTemporal()
    {
        _directorio = Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", Guid.NewGuid().ToString("N"));
        CadenaConexion = RutaBaseDatos.CadenaConexion(Path.Combine(_directorio, RutaBaseDatos.NombreArchivo));

        using var contexto = CrearContexto();
        contexto.Database.Migrate();
    }

    public string CadenaConexion { get; }

    public VeterinariaDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<VeterinariaDbContext>()
            .UseSqlite(CadenaConexion)
            .Options;
        return new VeterinariaDbContext(opciones);
    }

    /// <summary>Ejecuta SQL directo, sin pasar por las validaciones de EF, para probar las restricciones de la BD.</summary>
    public void Ejecutar(string sql)
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.ExecuteNonQuery();
    }

    public T? Escalar<T>(string sql)
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        var resultado = comando.ExecuteScalar();
        return resultado is null or DBNull ? default : (T)Convert.ChangeType(resultado, typeof(T));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_directorio, recursive: true);
        }
        catch (IOException)
        {
            // Un archivo temporal que no se pueda borrar no debe fallar la prueba.
        }
    }
}
