using VeterinariaDrFabio.App.Infraestructura;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Abridor de enlaces de prueba: no abre el navegador, solo registra las direcciones pedidas.</summary>
public sealed class AbridorFalso : IAbridorDeEnlaces
{
    public List<string> Enlaces { get; } = [];

    /// <summary>Si se asigna, <see cref="Abrir"/> lanza esta excepción para probar el fallo al abrir WhatsApp.</summary>
    public Exception? Falla { get; set; }

    public void Abrir(string direccion)
    {
        if (Falla is not null)
        {
            throw Falla;
        }

        Enlaces.Add(direccion);
    }
}
