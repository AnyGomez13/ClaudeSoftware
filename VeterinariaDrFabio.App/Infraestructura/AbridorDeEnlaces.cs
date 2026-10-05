using System.Diagnostics;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <inheritdoc cref="IAbridorDeEnlaces"/>
public class AbridorDeEnlaces : IAbridorDeEnlaces
{
    public void Abrir(string direccion)
    {
        if (!Uri.TryCreate(direccion, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException("Solo se pueden abrir direcciones web http o https.", nameof(direccion));
        }

        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
}
