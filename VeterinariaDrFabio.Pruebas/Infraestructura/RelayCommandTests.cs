using VeterinariaDrFabio.App.Infraestructura;

namespace VeterinariaDrFabio.Pruebas.Infraestructura;

/// <summary>Pruebas de <see cref="RelayCommand"/> (patrón Command de MVVM).</summary>
public class RelayCommandTests
{
    [Fact]
    [Trait("Req", "RNF-01")]
    public void EjecutaLaAccionConElParametro()
    {
        object? recibido = null;
        var comando = new RelayCommand(parametro => recibido = parametro);

        comando.Execute("hola");

        Assert.Equal("hola", recibido);
    }

    [Fact]
    [Trait("Req", "RNF-01")]
    public void NoEjecutaLaAccionSiNoEstaHabilitado()
    {
        var llamadas = 0;
        var habilitado = false;
        var comando = new RelayCommand(() => llamadas++, () => habilitado);

        comando.Execute(null);
        Assert.False(comando.CanExecute(null));
        Assert.Equal(0, llamadas);

        habilitado = true;
        comando.Execute(null);
        Assert.True(comando.CanExecute(null));
        Assert.Equal(1, llamadas);
    }

    [Fact]
    [Trait("Req", "RNF-01")]
    public void SinCondicionSiempreEstaHabilitado()
    {
        Assert.True(new RelayCommand(() => { }).CanExecute(null));
    }
}
