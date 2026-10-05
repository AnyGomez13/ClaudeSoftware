using VeterinariaDrFabio.App.Infraestructura;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Diálogos de prueba: no abren ventanas, registran lo que se pidió y responden lo que se configure.</summary>
public sealed class DialogoFalso : IDialogService
{
    public bool RespuestaConfirmar { get; set; } = true;

    public string? RutaDeGuardado { get; set; }

    public string? NombreSugeridoRecibido { get; private set; }

    public string? FiltroRecibido { get; private set; }

    public List<string> Mensajes { get; } = [];

    public List<string> Errores { get; } = [];

    public List<string> Confirmaciones { get; } = [];

    public void MostrarMensaje(string titulo, string mensaje) => Mensajes.Add(mensaje);

    public void MostrarError(string titulo, string mensaje) => Errores.Add(mensaje);

    public bool Confirmar(string titulo, string mensaje)
    {
        Confirmaciones.Add(mensaje);
        return RespuestaConfirmar;
    }

    public string? PedirRutaDeGuardado(string nombreSugerido, string filtro)
    {
        NombreSugeridoRecibido = nombreSugerido;
        FiltroRecibido = filtro;
        return RutaDeGuardado;
    }
}
