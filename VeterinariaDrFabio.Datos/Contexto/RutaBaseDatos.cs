using Microsoft.Data.Sqlite;

namespace VeterinariaDrFabio.Datos.Contexto;

/// <summary>
/// Ubicación y cadena de conexión de la base SQLite local (RNF-03, RN-12).
/// </summary>
public static class RutaBaseDatos
{
    public const string NombreArchivo = "veterinaria.db";

    /// <summary>Carpeta de la base: %LocalAppData%\VeterinariaDrFabio.</summary>
    public static string DirectorioPorDefecto =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VeterinariaDrFabio");

    /// <summary>Archivo de la base: %LocalAppData%\VeterinariaDrFabio\veterinaria.db.</summary>
    public static string ArchivoPorDefecto => Path.Combine(DirectorioPorDefecto, NombreArchivo);

    /// <summary>
    /// Arma la cadena de conexión con las llaves foráneas activas, requisito del DDL (RNF-08).
    /// Crea la carpeta si no existe.
    /// </summary>
    public static string CadenaConexion(string? archivo = null)
    {
        var ruta = archivo ?? ArchivoPorDefecto;

        var directorio = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = ruta,
            ForeignKeys = true,
        }.ToString();
    }
}
