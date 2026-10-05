namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>Apertura de diálogos sin que los ViewModels conozcan WPF (§1.3).</summary>
public interface IDialogService
{
    void MostrarMensaje(string titulo, string mensaje);

    void MostrarError(string titulo, string mensaje);

    /// <summary>Pregunta Sí/No al usuario; devuelve verdadero si confirma.</summary>
    bool Confirmar(string titulo, string mensaje);

    /// <summary>
    /// Pide dónde guardar un archivo (por ejemplo el carnet en PDF, supuesto A-12). Devuelve la ruta elegida o nulo si cancela.
    /// </summary>
    string? PedirRutaDeGuardado(string nombreSugerido, string filtro);
}
