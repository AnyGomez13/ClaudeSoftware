using System.Windows;
using Microsoft.Win32;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <inheritdoc cref="IDialogService"/>
public class DialogService : IDialogService
{
    public void MostrarMensaje(string titulo, string mensaje) =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);

    public void MostrarError(string titulo, string mensaje) =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirmar(string titulo, string mensaje) =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public string? PedirRutaDeGuardado(string nombreSugerido, string filtro)
    {
        var dialogo = new SaveFileDialog
        {
            FileName = nombreSugerido,
            Filter = filtro,
            AddExtension = true,
            OverwritePrompt = true,
        };

        return dialogo.ShowDialog() == true ? dialogo.FileName : null;
    }
}
