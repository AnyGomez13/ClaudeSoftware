using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Pruebas.ViewModels;

/// <summary>Conectividad de prueba: responde lo que se configure, sin consultar la red del equipo.</summary>
public sealed class ConectividadDePrueba : IConectividad
{
    public bool HayConexion { get; set; } = true;

    public bool HayInternet() => HayConexion;
}
