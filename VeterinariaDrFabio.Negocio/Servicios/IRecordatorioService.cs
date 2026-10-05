using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>
/// Recordatorios por WhatsApp con enlace gratuito wa.me (click-to-chat); no usa la API de pago
/// (RF-15, RN-10, RN-14, RN-15, RNF-04, RNF-06, CU-12).
/// </summary>
public interface IRecordatorioService
{
    /// <summary>
    /// Arma el mensaje y el enlace para la alerta y guarda el recordatorio como Pendiente. Si ya existe uno
    /// pendiente para ese mismo origen lo reutiliza, sin duplicar avisos. Devuelve un error, sin guardar nada, si el
    /// teléfono del propietario no es un celular colombiano válido o si no hay conexión a internet.
    /// </summary>
    Resultado<Recordatorio> Preparar(AlertaProximaFecha alerta);

    /// <summary>Marca el recordatorio como Enviado con la fecha y hora actuales; ya no vuelve a aparecer en las alertas.</summary>
    Resultado MarcarEnviado(Recordatorio recordatorio);
}
