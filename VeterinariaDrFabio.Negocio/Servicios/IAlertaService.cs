using VeterinariaDrFabio.Dominio.Modelos;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <summary>Alertas de vacunaciones y desparasitaciones próximas o vencidas (RF-14, RN-11, CU-11).</summary>
public interface IAlertaService
{
    /// <summary>
    /// Lista, ordenadas por fecha, las vacunaciones y desparasitaciones vencidas o que vencen en los próximos
    /// 30 días. Se omiten las de mascotas inactivas, las que ya tienen un recordatorio enviado y las que
    /// una aplicación posterior dejó sin efecto.
    /// </summary>
    List<AlertaProximaFecha> ProximasFechas();
}
