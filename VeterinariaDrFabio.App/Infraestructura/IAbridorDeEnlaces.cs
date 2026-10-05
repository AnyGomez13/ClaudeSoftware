namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>Abre un enlace en el navegador del equipo, por ejemplo el click-to-chat de WhatsApp (RF-15, SUP-01).</summary>
public interface IAbridorDeEnlaces
{
    /// <summary>Abre la dirección http o https; lanza <see cref="ArgumentException"/> si no es una dirección web.</summary>
    void Abrir(string direccion);
}
