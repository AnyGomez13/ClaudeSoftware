using System.Text.RegularExpressions;

namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>
/// Arma el enlace gratuito wa.me de click-to-chat con el mensaje ya escrito; no usa la API de pago (RF-15, RN-10, RN-14, SUP-01).
/// </summary>
public partial class GeneradorEnlaceWhatsApp
{
    private const string PrefijoPais = "57";

    /// <summary>
    /// Devuelve https://wa.me/57&lt;celular&gt;?text=&lt;mensaje codificado&gt;.
    /// El teléfono debe ser un celular colombiano de 10 dígitos que inicia en 3, sin prefijo (SUP-D07).
    /// </summary>
    /// <exception cref="ArgumentException">El teléfono no es un celular colombiano válido o el mensaje está vacío.</exception>
    public string Construir(string telefono, string mensaje)
    {
        if (string.IsNullOrEmpty(telefono) || !CelularColombiano().IsMatch(telefono))
        {
            throw new ArgumentException("El teléfono debe ser un celular colombiano de 10 dígitos que inicia en 3.", nameof(telefono));
        }

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            throw new ArgumentException("El mensaje no puede estar vacío.", nameof(mensaje));
        }

        return $"https://wa.me/{PrefijoPais}{telefono}?text={Uri.EscapeDataString(mensaje)}";
    }

    [GeneratedRegex(@"^3[0-9]{9}\z")]
    private static partial Regex CelularColombiano();
}
