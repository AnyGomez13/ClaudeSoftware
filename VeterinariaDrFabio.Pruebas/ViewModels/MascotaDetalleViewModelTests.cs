using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.App.ViewModels;
using VeterinariaDrFabio.Negocio.Utilidades;
using VeterinariaDrFabio.Pruebas.Datos;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Pruebas de <see cref="MascotaDetalleViewModel"/>, la ficha P-07 (RF-06, RF-08, RF-10, RN-07, RNF-09).</summary>
public class MascotaDetalleViewModelTests : PantallaTestBase
{
    private int _fabioId;
    private int _williamId;

    private MascotaDetalleViewModel Abrir(int mascotaId)
    {
        var pantalla = Servicios.GetRequiredService<MascotaDetalleViewModel>();
        pantalla.Cargar(mascotaId);
        return pantalla;
    }

    private (int PropietarioId, int MascotaId) SembrarMascota()
    {
        var ana = DatosDePrueba.CrearPropietario(Bd, "Ana Pérez", "3001234567");
        var rocky = DatosDePrueba.CrearMascota(Bd, ana.Id, "Rocky");
        _fabioId = DatosDePrueba.IdDeVeterinario(Bd, "Fabio");
        _williamId = DatosDePrueba.IdDeVeterinario(Bd, "William");
        return (ana.Id, rocky.Id);
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_LaFichaMuestraLosDatosDeLaMascotaYDelPropietario()
    {
        var (_, rocky) = SembrarMascota();

        var ficha = Abrir(rocky);

        Assert.True(ficha.Encontrada);
        Assert.False(ficha.NoEncontrada);
        Assert.Equal("Rocky", ficha.Nombre);
        Assert.Equal("Perro", ficha.Especie);
        Assert.Equal("—", ficha.Raza);
        Assert.Equal("—", ficha.Sexo);
        Assert.Equal("15/03/2020", ficha.Nacimiento);
        Assert.Equal("12,5 kg", ficha.Peso);
        Assert.Equal("—", ficha.ColorSenas);
        Assert.Equal("Activa", ficha.Estado);
        Assert.Equal("Ana Pérez", ficha.PropietarioNombre);
        Assert.Equal("3001234567", ficha.PropietarioTelefono);
    }

    [Fact]
    [Trait("Req", "RF-08")]
    public void RF08_LaEdadSeCalculaConLaFechaActual()
    {
        var (_, rocky) = SembrarMascota();

        var ficha = Abrir(rocky);

        Assert.Equal(new CalculadoraEdad().Legible(new DateTime(2020, 3, 15)), ficha.Edad);
    }

    [Fact]
    [Trait("Req", "SUP-04")]
    public void SUP04_UnaFechaEstimadaSeIndicaEnLaFicha()
    {
        var (_, rocky) = SembrarMascota();
        Bd.Ejecutar($"UPDATE Mascota SET FechaNacimientoEstimada = 1, Sexo = 'Macho', Raza = 'Criollo', ColorSenas = 'Negro' WHERE Id = {rocky};");

        var ficha = Abrir(rocky);

        Assert.Equal("15/03/2020 (estimada)", ficha.Nacimiento);
        Assert.Equal("Macho", ficha.Sexo);
        Assert.Equal("Criollo", ficha.Raza);
        Assert.Equal("Negro", ficha.ColorSenas);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_LaHistoriaMezclaProcedimientosYVacunacionesEnOrdenCronologicoConSuVeterinario()
    {
        var (_, rocky) = SembrarMascota();
        DatosDePrueba.CrearVacunacion(Bd, rocky, _williamId, "Rabia", new DateTime(2026, 6, 1), new DateTime(2027, 6, 1));
        DatosDePrueba.CrearProcedimiento(Bd, rocky, _fabioId, "Control", new DateTime(2026, 9, 10));
        DatosDePrueba.CrearProcedimiento(Bd, rocky, _williamId, "Consulta", new DateTime(2026, 2, 1));

        var ficha = Abrir(rocky);

        Assert.Equal(["01/02/2026", "01/06/2026", "10/09/2026"], ficha.Registros.Select(r => r.Fecha).ToList());
        Assert.Equal(["Procedimiento", "Vacunación", "Procedimiento"], ficha.Registros.Select(r => r.Tipo).ToList());
        Assert.Equal(["William", "William", "Fabio"], ficha.Registros.Select(r => r.Veterinario).ToList());
        Assert.Equal("Rabia. Próximo refuerzo: 01/06/2027", ficha.Registros[1].Detalle);
        Assert.False(ficha.SinRegistros);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_UnaMascotaSinRegistrosLoIndica()
    {
        var (_, rocky) = SembrarMascota();

        var ficha = Abrir(rocky);

        Assert.Empty(ficha.Registros);
        Assert.True(ficha.SinRegistros);
    }

    [Fact]
    [Trait("Req", "RF-06")]
    public void RF06_UnaMascotaInexistenteMuestraElAvisoEnLugarDeLaFicha()
    {
        SembrarMascota();

        var ficha = Abrir(999);

        Assert.False(ficha.Encontrada);
        Assert.True(ficha.NoEncontrada);
        Assert.False(ficha.SinRegistros);
        Assert.Empty(ficha.Registros);
    }

    [Fact]
    [Trait("Req", "RF-10")]
    public void RF10_RecargarIncorporaLosRegistrosNuevos()
    {
        var (_, rocky) = SembrarMascota();
        var ficha = Abrir(rocky);
        Assert.True(ficha.SinRegistros);
        var cambios = new List<string?>();
        ficha.PropertyChanged += (_, e) => cambios.Add(e.PropertyName);

        DatosDePrueba.CrearProcedimiento(Bd, rocky, _fabioId, "Consulta", new DateTime(2026, 2, 1));
        ficha.Recargar();

        Assert.Single(ficha.Registros);
        Assert.False(ficha.SinRegistros);
        Assert.Contains(nameof(MascotaDetalleViewModel.Registros), cambios);
    }

    [Fact]
    [Trait("Req", "RF-07")]
    public void RF07_UnaMascotaInactivaSeMuestraComoInactiva()
    {
        var (_, rocky) = SembrarMascota();
        Bd.Ejecutar($"UPDATE Mascota SET Activo = 0 WHERE Id = {rocky};");

        Assert.Equal("Inactiva", Abrir(rocky).Estado);
    }
}
