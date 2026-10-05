namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>Una vacuna del listado, con cada cuántos meses se suele aplicar el refuerzo y a qué especies corresponde.</summary>
/// <param name="Nombre">Nombre con el que se guarda en la vacunación.</param>
/// <param name="MesesRefuerzo">Meses hasta el refuerzo habitual; es solo una sugerencia que el veterinario puede cambiar.</param>
/// <param name="Especies">Especies a las que se ofrece; una lista vacía significa todas.</param>
public sealed record VacunaSugerida(string Nombre, int MesesRefuerzo, IReadOnlyList<string> Especies);

/// <summary>
/// Listado de vacunas habituales con su intervalo de refuerzo, para agilizar el registro de una vacunación (RF-11, SUP-10).
/// Los intervalos son valores iniciales para que los revisen los veterinarios; en pantalla siempre se pueden cambiar.
/// </summary>
public static class CatalogoVacunas
{
    private static readonly string[] Perro = ["Perro"];
    private static readonly string[] Gato = ["Gato"];
    private static readonly string[] PerroYGato = ["Perro", "Gato"];

    public static IReadOnlyList<VacunaSugerida> Todas { get; } =
    [
        new("Rabia", 12, PerroYGato),
        new("Triple canina", 12, Perro),
        new("Séxtuple canina", 12, Perro),
        new("Parvovirus", 12, Perro),
        new("Moquillo", 12, Perro),
        new("Leptospirosis", 12, Perro),
        new("Bordetella", 6, Perro),
        new("Triple felina", 12, Gato),
        new("Leucemia felina", 12, Gato),
    ];

    /// <summary>
    /// Vacunas que se ofrecen para la especie dada. Si la especie no es perro ni gato (o no se conoce) se ofrecen todas.
    /// </summary>
    public static IReadOnlyList<VacunaSugerida> ParaEspecie(string? especie)
    {
        var buscada = TextoComparable.Normalizar(especie);
        var coincidentes = Todas
            .Where(v => v.Especies.Any(e => TextoComparable.Normalizar(e) == buscada))
            .ToList();
        return coincidentes.Count > 0 ? coincidentes : Todas;
    }

    /// <summary>Busca una vacuna por su nombre sin importar tildes ni mayúsculas; nulo si no está en el listado.</summary>
    public static VacunaSugerida? Buscar(string? nombre)
    {
        var buscado = TextoComparable.Normalizar(nombre);
        return buscado.Length == 0 ? null : Todas.FirstOrDefault(v => TextoComparable.Normalizar(v.Nombre) == buscado);
    }

    /// <summary>Fecha sugerida del refuerzo: la de aplicación más los meses de la vacuna; nulo si la vacuna no está en el listado.</summary>
    public static DateTime? CalcularRefuerzo(string? nombre, DateTime fechaAplicacion) =>
        Buscar(nombre) is { } vacuna ? fechaAplicacion.Date.AddMonths(vacuna.MesesRefuerzo) : null;
}
