using System.Windows.Input;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>Implementación de <see cref="ICommand"/> para enlazar botones a métodos del ViewModel (patrón Command).</summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _ejecutar;
    private readonly Func<object?, bool>? _puedeEjecutar;

    public RelayCommand(Action<object?> ejecutar, Func<object?, bool>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar;
        _puedeEjecutar = puedeEjecutar;
    }

    public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
        : this(_ => ejecutar(), puedeEjecutar is null ? null : _ => puedeEjecutar())
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parametro) => _puedeEjecutar?.Invoke(parametro) ?? true;

    public void Execute(object? parametro)
    {
        if (CanExecute(parametro))
        {
            _ejecutar(parametro);
        }
    }

    /// <summary>Pide a WPF que vuelva a evaluar si el comando está habilitado.</summary>
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
