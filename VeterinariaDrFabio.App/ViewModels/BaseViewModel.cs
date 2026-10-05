using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VeterinariaDrFabio.App.ViewModels;

/// <summary>Base de los ViewModels: notifica a la vista cuando cambia una propiedad (MVVM, §1.4).</summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Asigna el valor y notifica solo si cambió; devuelve verdadero cuando hubo cambio.</summary>
    protected bool SetProperty<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
        {
            return false;
        }

        campo = valor;
        OnPropertyChanged(nombrePropiedad);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
}
