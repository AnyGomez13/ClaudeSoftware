using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;

namespace VeterinariaDrFabio.Pruebas.Datos;

/// <summary>Atajos para sembrar datos en las pruebas de repositorios usando los propios repositorios.</summary>
internal static class DatosDePrueba
{
    public static Propietario CrearPropietario(
        BaseDatosTemporal bd, string nombre = "Ana Pérez", string telefono = "3001234567", bool activo = true)
    {
        using var contexto = bd.CrearContexto();
        var propietario = new Propietario { NombreCompleto = nombre, Telefono = telefono, Activo = activo };
        new PropietarioRepository(contexto).Agregar(propietario);
        return propietario;
    }

    public static Mascota CrearMascota(BaseDatosTemporal bd, int propietarioId, string nombre = "Rocky")
    {
        using var contexto = bd.CrearContexto();
        var mascota = new Mascota
        {
            PropietarioId = propietarioId,
            Nombre = nombre,
            Especie = "Perro",
            FechaNacimiento = new DateTime(2020, 3, 15),
            Peso = 12.5,
        };
        new MascotaRepository(contexto).Agregar(mascota);
        return mascota;
    }

    public static Vacunacion CrearVacunacion(
        BaseDatosTemporal bd, int mascotaId, int veterinarioId, string nombre, DateTime aplicacion, DateTime? proxima = null)
    {
        using var contexto = bd.CrearContexto();
        var vacunacion = new Vacunacion
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            NombreVacuna = nombre,
            FechaAplicacion = aplicacion,
            ProximaFecha = proxima,
        };
        new VacunacionRepository(contexto).Agregar(vacunacion);
        return vacunacion;
    }

    public static Procedimiento CrearProcedimiento(
        BaseDatosTemporal bd, int mascotaId, int veterinarioId, string tipo, DateTime fecha, DateTime? proxima = null)
    {
        using var contexto = bd.CrearContexto();
        var procedimiento = new Procedimiento
        {
            MascotaId = mascotaId,
            VeterinarioId = veterinarioId,
            TipoProcedimiento = tipo,
            Descripcion = "Descripción",
            Fecha = fecha,
            ProximaFechaRecomendada = proxima,
        };
        new ProcedimientoRepository(contexto).Agregar(procedimiento);
        return procedimiento;
    }

    public static int IdDeVeterinario(BaseDatosTemporal bd, string nombre)
    {
        using var contexto = bd.CrearContexto();
        return contexto.Veterinarios.Single(v => v.NombreCompleto == nombre).Id;
    }
}
