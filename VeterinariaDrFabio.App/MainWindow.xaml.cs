using System.Windows;
using VeterinariaDrFabio.App.ViewModels;

namespace VeterinariaDrFabio.App;

/// <summary>Ventana principal (P-02). Solo conecta la vista con su ViewModel.</summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel modelo)
    {
        InitializeComponent();
        DataContext = modelo;
    }
}
