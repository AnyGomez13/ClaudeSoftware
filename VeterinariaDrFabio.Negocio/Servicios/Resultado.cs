namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>
/// Resultado de una operación de negocio: éxito o error con un mensaje que la interfaz puede mostrar (RNF-01).
/// </summary>
public sealed class Resultado
{
    private Resultado(bool exito, string mensaje)
    {
        Exito = exito;
        Mensaje = mensaje;
    }

    public bool Exito { get; }

    /// <summary>Vacío cuando la operación fue exitosa; en caso contrario explica qué falló.</summary>
    public string Mensaje { get; }

    public static Resultado Ok() => new(true, string.Empty);

    public static Resultado Error(string mensaje) => new(false, mensaje);
}
