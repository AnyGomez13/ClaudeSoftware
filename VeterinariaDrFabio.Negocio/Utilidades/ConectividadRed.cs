using System.Net.NetworkInformation;

namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <inheritdoc cref="IConectividad"/>
/// Consulta si alguna interfaz de red está activa; no hace peticiones a internet.
public class ConectividadRed : IConectividad
{
    public bool HayInternet() => NetworkInterface.GetIsNetworkAvailable();
}
