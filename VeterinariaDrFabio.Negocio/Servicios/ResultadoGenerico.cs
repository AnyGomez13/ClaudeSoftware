namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>
/// Resultado de una operación de negocio que, si tiene éxito, devuelve un valor (por ejemplo el carnet generado).
/// </summary>
public sealed class Resultado<T>
{
    private Resultado(bool exito, string mensaje, T? valor)
    {
        Exito = exito;
        Mensaje = mensaje;
        Valor = valor;
    }

    public bool Exito { get; }

    /// <summary>Vacío cuando la operación fue exitosa; en caso contrario explica qué falló.</summary>
    public string Mensaje { get; }

    /// <summary>El valor producido; solo tiene sentido cuando <see cref="Exito"/> es verdadero.</summary>
    public T? Valor { get; }

    public static Resultado<T> Ok(T valor) => new(true, string.Empty, valor);

    public static Resultado<T> Error(string mensaje) => new(false, mensaje, default);
}
