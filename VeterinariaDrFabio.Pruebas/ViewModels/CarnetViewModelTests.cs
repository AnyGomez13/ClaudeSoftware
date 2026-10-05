using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="CarnetViewModel"/>, la pantalla P-10 (RF-12, RF-13, RN-09, RNF-07).</summary>
public class CarnetViewModelTests : PantallaTestBase
{
    private int _mascotaId;

    private CarnetViewModel Abrir(bool conVacunas)
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        _mascotaId = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky").Id;
        if (conVacunas)
        {
            var fabio = DatosDePrueba.IdDeVeterinario(Bd, "Fabio");
            var william = DatosDePrueba.IdDeVeterinario(Bd, "William");
            DatosDePrueba.CrearVacunacion(Bd, _mascotaId, william, "Parvovirus", new DateTime(2026, 6, 1));
            DatosDePrueba.CrearVacunacion(Bd, _mascotaId, fabio, "Rabia", new DateTime(2026, 2, 1), new DateTime(2027, 2, 1));
        }

        var pantalla = Servicios.GetRequiredService<CarnetViewModel>();
        pantalla.Cargar(_mascotaId);
        return pantalla;
    }

    private static string CarpetaTemporal() => Path.Combine(Path.GetTempPath(), "VeterinariaDrFabio.Pruebas", Guid.NewGuid().ToString("N"));

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_LaVistaPreviaMuestraMascotaPropietarioYTodasLasVacunasEnOrden()
    {
        var pantalla = Abrir(conVacunas: true);

        Assert.True(pantalla.HayCarnet);
        Assert.False(pantalla.TieneAviso);
        var mascota = pantalla.DatosMascota.ToDictionary(d => d.Etiqueta, d => d.Valor);
        Assert.Equal("Rocky", mascota["Nombre"]);
        Assert.Equal("Perro", mascota["Especie"]);
        Assert.Equal("15/03/2020", mascota["Nacimiento"]);
        Assert.Equal("12,5 kg", mascota["Peso"]);
        Assert.False(string.IsNullOrWhiteSpace(mascota["Edad"]));
        Assert.Equal(["Ana Pérez", "3001234567"], pantalla.DatosPropietario.Select(d => d.Valor).ToList());
        Assert.Equal(["Rabia", "Parvovirus"], pantalla.Vacunas.Select(v => v.Vacuna).ToList());
        Assert.Equal(["01/02/2026", "01/06/2026"], pantalla.Vacunas.Select(v => v.Aplicada).ToList());
        Assert.Equal(["01/02/2027", "—"], pantalla.Vacunas.Select(v => v.Refuerzo).ToList());
        Assert.Equal(["Fabio", "William"], pantalla.Vacunas.Select(v => v.Veterinario).ToList());
        Assert.Equal($"Generado el {DateTime.Today:dd/MM/yyyy}", pantalla.FechaGeneracion);
        Assert.True(pantalla.DescargarPdfCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_SinVacunasMuestraUnAvisoYNoSePuedeDescargar()
    {
        var pantalla = Abrir(conVacunas: false);

        Assert.False(pantalla.HayCarnet);
        Assert.True(pantalla.TieneAviso);
        Assert.Contains("no tiene vacunas", pantalla.Aviso);
        Assert.Empty(pantalla.Vacunas);
        Assert.Empty(pantalla.DatosMascota);
        Assert.False(pantalla.DescargarPdfCommand.CanExecute(null));
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_UnaMascotaInexistenteMuestraElAviso()
    {
        Abrir(conVacunas: false);
        var pantalla = Servicios.GetRequiredService<CarnetViewModel>();

        pantalla.Cargar(999);

        Assert.False(pantalla.HayCarnet);
        Assert.Contains("no existe", pantalla.Aviso);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_DescargarPdfGuardaElArchivoEnLaRutaElegidaYLoInforma()
    {
        var pantalla = Abrir(conVacunas: true);
        var carpeta = CarpetaTemporal();
        var ruta = Path.Combine(carpeta, "carnet-rocky.pdf");
        Dialogos.RutaDeGuardado = ruta;
        try
        {
            pantalla.DescargarPdfCommand.Execute(null);

            Assert.True(new FileInfo(ruta).Length > 1024);
            Assert.Contains(ruta, Assert.Single(Dialogos.Mensajes));
            Assert.Empty(Dialogos.Errores);
            Assert.Equal($"Carnet_Rocky_{DateTime.Today:yyyy-MM-dd}.pdf", Dialogos.NombreSugeridoRecibido);
            Assert.Equal(CarnetViewModel.FiltroPdf, Dialogos.FiltroRecibido);
        }
        finally
        {
            if (Directory.Exists(carpeta))
            {
                Directory.Delete(carpeta, recursive: true);
            }
        }
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_SiElUsuarioCancelaElDialogoNoSeGuardaNiSeAvisaNada()
    {
        var pantalla = Abrir(conVacunas: true);
        Dialogos.RutaDeGuardado = null;

        pantalla.DescargarPdfCommand.Execute(null);

        Assert.Empty(Dialogos.Mensajes);
        Assert.Empty(Dialogos.Errores);
    }

    [Fact]
    [Trait("Req", "RF-13")]
    public void RF13_UnaRutaQueNoSePuedeEscribirSeInformaComoError()
    {
        var pantalla = Abrir(conVacunas: true);
        var carpeta = CarpetaTemporal();
        Directory.CreateDirectory(carpeta);
        var archivoComoCarpeta = Path.Combine(carpeta, "no-es-carpeta");
        File.WriteAllText(archivoComoCarpeta, "x");
        Dialogos.RutaDeGuardado = Path.Combine(archivoComoCarpeta, "carnet.pdf");
        try
        {
            pantalla.DescargarPdfCommand.Execute(null);

            Assert.Contains("No se pudo guardar", Assert.Single(Dialogos.Errores));
            Assert.Empty(Dialogos.Mensajes);
        }
        finally
        {
            Directory.Delete(carpeta, recursive: true);
        }
    }

    [Fact]
    [Trait("Req", "RF-12")]
    public void RF12_VolverRegresaALaFichaDeLaMascota()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        Navegacion.NavegarASeccion<MascotasViewModel>();
        Navegacion.NavegarA<MascotaDetalleViewModel>(vm => vm.Cargar(rocky.Id));
        var ficha = Assert.IsType<MascotaDetalleViewModel>(Navegacion.ViewModelActual);

        ficha.GenerarCarnetCommand.Execute(null);
        var carnet = Assert.IsType<CarnetViewModel>(Navegacion.ViewModelActual);
        Assert.True(carnet.VolverCommand.CanExecute(null));
        carnet.VolverCommand.Execute(null);

        Assert.Same(ficha, Navegacion.ViewModelActual);
    }
}
