using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Datos.Repositorios;

/// <summary>Acceso a datos de los recordatorios de WhatsApp (RF-14, RF-15, SUP-D06).</summary>
public interface IRecordatorioRepository
{
    void Agregar(Recordatorio recordatorio);

    /// <summary>Guarda los cambios, por ejemplo el paso de Pendiente a Enviado.</summary>
    void Actualizar(Recordatorio recordatorio);

    /// <summary>Recordatorios en estado Pendiente con mascota y propietario, ordenados por fecha objetivo.</summary>
    List<Recordatorio> ListarPendientes();
}
