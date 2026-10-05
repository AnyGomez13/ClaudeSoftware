using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="AlertasViewModel"/>, la pantalla P-11 (RF-14, RF-15, RN-10, RNF-04, RNF-06, CU-11, CU-12).</summary>
public class AlertasViewModelTests : PantallaTestBase
{
    private static readonly DateTime Hoy = DateTime.Today;
    private int _mascotaId;
    private int _fabioId;

    private AlertasViewModel CrearPantalla() => Servicios.GetRequiredService<AlertasViewModel>();

    private void SembrarMascota()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        _fabioId = DatosDePrueba.IdDeVeterinario(Bd, "Fabio");
    }

    private void SembrarAlertas()
    {
        SembrarMascota();
        DatosDePrueba.CrearVacunacion(Bd, _mascotaId, _fabioId, "Rabia", Hoy.AddDays(-300), Hoy.AddDays(-5));
        DatosDePrueba.CrearVacunacion(Bd, _mascotaId, _fabioId, "Parvovirus", Hoy.AddDays(-200), Hoy.AddDays(10));
        DatosDePrueba.CrearVacunacion(Bd, _mascotaId, _fabioId, "Moquillo", Hoy.AddDays(-100), Hoy.AddDays(200));
    }

    private int ContarRecordatorios(string? estado = null) =>
        Bd.Escalar<int>(estado is null
            ? "SELECT COUNT(*) FROM Recordatorio;"
            : $"SELECT COUNT(*) FROM Recordatorio WHERE Estado = '{estado}';");

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_ListaLasAlertasVencidasYProximasOrdenadasPorFecha()
    {
        SembrarAlertas();

        var pantalla = CrearPantalla();

        Assert.Equal(["Vacunación: Rabia", "Vacunación: Parvovirus"], pantalla.Alertas.Select(a => a.Tipo).ToList());
        Assert.Equal(["Vencida", "Próxima"], pantalla.Alertas.Select(a => a.Estado).ToList());
        Assert.Equal([true, false], pantalla.Alertas.Select(a => a.EsVencida).ToList());
        Assert.Equal(Hoy.AddDays(-5).ToString("dd/MM/yyyy"), pantalla.Alertas[0].FechaObjetivo);
        Assert.All(pantalla.Alertas, a => Assert.Equal("Rocky", a.Mascota));
        Assert.All(pantalla.Alertas, a => Assert.Equal("Ana Pérez", a.Propietario));
        Assert.False(pantalla.SinAlertas);
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_UnaDesparasitacionSeMuestraComoTal()
    {
        SembrarMascota();
        DatosDePrueba.CrearProcedimiento(Bd, _mascotaId, _fabioId, "Desparasitación", Hoy.AddDays(-80), Hoy.AddDays(5));

        var pantalla = CrearPantalla();

        Assert.Equal("Desparasitación", Assert.Single(pantalla.Alertas).Tipo);
    }

    [Fact]
    [Trait("Req", "RF-14")]
    public void RF14_SinAlertasLoIndica()
    {
        SembrarMascota();

        var pantalla = CrearPantalla();

        Assert.Empty(pantalla.Alertas);
        Assert.True(pantalla.SinAlertas);
        Assert.False(pantalla.HayRecordatorio);
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_EnviarRecordatorioMuestraElMensajeYAbreElEnlaceDeWhatsApp()
    {
        SembrarAlertas();
        var pantalla = CrearPantalla();

        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);

        var enlace = Assert.Single(Abridor.Enlaces);
        Assert.StartsWith("https://wa.me/573001234567?text=", enlace);
        Assert.True(pantalla.HayRecordatorio);
        Assert.Equal("Recordatorio para Ana Pérez (Rocky)", pantalla.TituloRecordatorio);
        Assert.Contains("Rocky", pantalla.MensajeRecordatorio);
        Assert.Contains("vacuna Rabia", pantalla.MensajeRecordatorio);
        Assert.Equal(Uri.UnescapeDataString(enlace["https://wa.me/573001234567?text=".Length..]), pantalla.MensajeRecordatorio);
        Assert.Equal(1, ContarRecordatorios("Pendiente"));
        Assert.Equal(2, pantalla.Alertas.Count);
    }

    [Fact]
    [Trait("Req", "RN-10")]
    public void RN10_MarcarComoEnviadoGuardaElEstadoYLaAlertaDejaDeMostrarse()
    {
        SembrarAlertas();
        var pantalla = CrearPantalla();
        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);
        Assert.True(pantalla.MarcarEnviadoCommand.CanExecute(null));

        pantalla.MarcarEnviadoCommand.Execute(null);

        Assert.False(pantalla.HayRecordatorio);
        Assert.Equal(1, ContarRecordatorios("Enviado"));
        Assert.Equal(0, ContarRecordatorios("Pendiente"));
        Assert.Equal(["Vacunación: Parvovirus"], pantalla.Alertas.Select(a => a.Tipo).ToList());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_CerrarElPanelNoMarcaComoEnviadoYLaAlertaSigueVisible()
    {
        SembrarAlertas();
        var pantalla = CrearPantalla();
        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);

        pantalla.CerrarRecordatorioCommand.Execute(null);

        Assert.False(pantalla.HayRecordatorio);
        Assert.Equal(1, ContarRecordatorios("Pendiente"));
        Assert.Equal(2, pantalla.Alertas.Count);
        Assert.False(pantalla.MarcarEnviadoCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RNF-04")]
    public void RNF04_SinInternetSeAvisaYNoSeAbreNiSeGuardaNada()
    {
        SembrarAlertas();
        Conectividad.HayConexion = false;
        var pantalla = CrearPantalla();

        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);

        Assert.Contains("conexión a internet", Assert.Single(Dialogos.Errores));
        Assert.Empty(Abridor.Enlaces);
        Assert.False(pantalla.HayRecordatorio);
        Assert.Equal(0, ContarRecordatorios());
        Assert.Equal(2, pantalla.Alertas.Count);
    }

    [Fact]
    [Trait("Req", "RNF-04")]
    public void RNF04_LaListaDeAlertasFuncionaSinInternet()
    {
        SembrarAlertas();
        Conectividad.HayConexion = false;

        var pantalla = CrearPantalla();

        Assert.Equal(2, pantalla.Alertas.Count);
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_EnviarDosVecesLaMismaAlertaReutilizaElRecordatorio()
    {
        SembrarAlertas();
        var pantalla = CrearPantalla();

        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);
        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);

        Assert.Equal(2, Abridor.Enlaces.Count);
        Assert.Equal(1, ContarRecordatorios());
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_SiNoSePuedeAbrirWhatsAppSeAvisaYElMensajeQuedaALaVista()
    {
        SembrarAlertas();
        Abridor.Falla = new InvalidOperationException("No hay un navegador predeterminado.");
        var pantalla = CrearPantalla();

        pantalla.EnviarRecordatorioCommand.Execute(pantalla.Alertas[0]);

        Assert.Contains("No hay un navegador predeterminado.", Assert.Single(Dialogos.Errores));
        Assert.True(pantalla.HayRecordatorio);
        Assert.Contains("Rocky", pantalla.MensajeRecordatorio);
    }

    [Fact]
    [Trait("Req", "RF-15")]
    public void RF15_UnaFilaNulaNoHaceNada()
    {
        SembrarAlertas();
        var pantalla = CrearPantalla();

        pantalla.EnviarRecordatorioCommand.Execute(null);

        Assert.Empty(Abridor.Enlaces);
        Assert.False(pantalla.HayRecordatorio);
    }
}
