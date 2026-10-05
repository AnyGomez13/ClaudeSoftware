namespace VeterinariaDrFabio.Negocio.Utilidades;

/// <summary>
/// Indica si hay conexión a internet. Solo el envío de recordatorios la necesita; el resto funciona sin red (RNF-04, RN-15).
/// </summary>
public interface IConectividad
{
    bool HayInternet();
}
