using System.Globalization;
using VeterinariaDrFabio.Datos.Repositorios;
using VeterinariaDrFabio.Dominio.Entidades;
using VeterinariaDrFabio.Dominio.Modelos;
using VeterinariaDrFabio.Negocio.Utilidades;

namespace VeterinariaDrFabio.Negocio.Servicios;

/// <inheritdoc cref="IMascotaService"/>
/// RF-05, RF-06, RF-07, RF-08, RF-10, RN-02, RN-03, RN-06, RN-08, SUP-04, SUP-08, CU-05, CU-06, CU-07.
public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotas;
    private readonly IPropietarioRepository _propietarios;
    private readonly IProcedimientoRepository _procedimientos;
    private readonly IVacunacionRepository _vacunaciones;
    private readonly CalculadoraEdad _calculadoraEdad;

    public MascotaService(
        IMascotaRepository mascotas,
        IPropietarioRepository propietarios,
        IProcedimientoRepository procedimientos,
        IVacunacionRepository vacunaciones,
        CalculadoraEdad calculadoraEdad)
    {
        _mascotas = mascotas;
        _propietarios = propietarios;
        _procedimientos = procedimientos;
        _vacunaciones = vacunaciones;
        _calculadoraEdad = calculadoraEdad;
    }

    public Resultado Registrar(Mascota mascota)
    {
        var error = Normalizar(mascota);
        if (error is not null)
        {
            return error;
        }

        _mascotas.Agregar(mascota);
        return Resultado.Ok();
    }

    public Resultado Editar(Mascota mascota)
    {
        var existente = _mascotas.ObtenerPorId(mascota.Id);
        if (existente is null)
        {
            return Resultado.Error("La mascota no existe.");
        }

        var error = Normalizar(mascota);
        if (error is not null)
        {
            return error;
        }

        existente.PropietarioId = mascota.PropietarioId;
        existente.Nombre = mascota.Nombre;
        existente.Especie = mascota.Especie;
        existente.Raza = mascota.Raza;
        existente.Sexo = mascota.Sexo;
        existente.FechaNacimiento = mascota.FechaNacimiento;
        existente.FechaNacimientoEstimada = mascota.FechaNacimientoEstimada;
        existente.Peso = mascota.Peso;
        existente.ColorSenas = mascota.ColorSenas;
        existente.Activo = mascota.Activo;
        _mascotas.Actualizar(existente);
        return Resultado.Ok();
    }

    public List<Mascota> Buscar(string texto) => _mascotas.Buscar(texto);

    public Mascota? ObtenerPorId(int id) => _mascotas.ObtenerPorId(id);

    public HistoriaClinica? ObtenerHistoriaClinica(int mascotaId)
    {
        var mascota = _mascotas.ObtenerPorId(mascotaId);
        if (mascota is null)
        {
            return null;
        }

        var registros = new List<(RegistroClinicoItem Item, int Orden, int Id)>();
        foreach (var p in _procedimientos.ListarPorMascota(mascotaId))
        {
            registros.Add((DescribirProcedimiento(p), 0, p.Id));
        }

        foreach (var v in _vacunaciones.ListarPorMascota(mascotaId))
        {
            registros.Add((DescribirVacunacion(v), 1, v.Id));
        }

        var ordenados = registros
            .OrderBy(r => r.Item.Fecha)
            .ThenBy(r => r.Orden)
            .ThenBy(r => r.Id)
            .Select(r => r.Item)
            .ToList();

        return new HistoriaClinica
        {
            Mascota = mascota,
            FechaApertura = ordenados.Count > 0 ? ordenados[0].Fecha : default,
            Registros = ordenados,
        };
    }

    public int CalcularEdadMeses(Mascota mascota) => _calculadoraEdad.EnMeses(mascota.FechaNacimiento);

    public string CalcularEdadLegible(Mascota mascota) => _calculadoraEdad.Legible(mascota.FechaNacimiento);

    /// <summary>Limpia los textos y valida las reglas; devuelve el error o nulo si todo está bien.</summary>
    private Resultado? Normalizar(Mascota mascota)
    {
        if (mascota.PropietarioId <= 0 || _propietarios.ObtenerPorId(mascota.PropietarioId) is null)
        {
            return Resultado.Error("Debe seleccionar un propietario existente.");
        }

        var nombre = mascota.Nombre?.Trim();
        if (string.IsNullOrEmpty(nombre))
        {
            return Resultado.Error("El nombre de la mascota es obligatorio.");
        }

        var especie = mascota.Especie?.Trim();
        if (string.IsNullOrEmpty(especie))
        {
            return Resultado.Error("La especie es obligatoria.");
        }

        if (mascota.FechaNacimiento == default)
        {
            return Resultado.Error("La fecha de nacimiento es obligatoria (puede ser estimada).");
        }

        if (mascota.FechaNacimiento.Date > DateTime.Today)
        {
            return Resultado.Error("La fecha de nacimiento no puede ser futura.");
        }

        if (!(mascota.Peso > 0) || double.IsInfinity(mascota.Peso))
        {
            return Resultado.Error("El peso debe ser mayor a cero (en kilogramos).");
        }

        if (!TryNormalizarSexo(mascota.Sexo, out var sexo))
        {
            return Resultado.Error("El sexo debe ser Macho o Hembra.");
        }

        mascota.Nombre = nombre;
        mascota.Especie = especie;
        mascota.FechaNacimiento = mascota.FechaNacimiento.Date;
        mascota.Sexo = sexo;
        mascota.Raza = NuloSiVacio(mascota.Raza);
        mascota.ColorSenas = NuloSiVacio(mascota.ColorSenas);
        return null;
    }

    private static bool TryNormalizarSexo(string? texto, out string? sexo)
    {
        var limpio = texto?.Trim();
        if (string.IsNullOrEmpty(limpio))
        {
            sexo = null;
            return true;
        }

        sexo = limpio.ToLowerInvariant() switch
        {
            "macho" => "Macho",
            "hembra" => "Hembra",
            _ => null,
        };
        return sexo is not null;
    }

    private static string? NuloSiVacio(string? texto)
    {
        var limpio = texto?.Trim();
        return string.IsNullOrEmpty(limpio) ? null : limpio;
    }

    private static RegistroClinicoItem DescribirProcedimiento(Procedimiento procedimiento)
    {
        var detalle = $"{procedimiento.TipoProcedimiento}: {procedimiento.Descripcion}";
        if (!string.IsNullOrEmpty(procedimiento.Tratamiento))
        {
            detalle += $". Tratamiento: {procedimiento.Tratamiento}";
        }

        if (procedimiento.PesoEnElMomento is { } peso)
        {
            detalle += $". Peso: {peso.ToString("0.##", CultureInfo.GetCultureInfo("es-CO"))} kg";
        }

        return new RegistroClinicoItem
        {
            Fecha = procedimiento.Fecha,
            Origen = "Procedimiento",
            Detalle = detalle,
            Veterinario = procedimiento.Veterinario?.NombreCompleto ?? string.Empty,
        };
    }

    private static RegistroClinicoItem DescribirVacunacion(Vacunacion vacunacion)
    {
        var detalle = vacunacion.NombreVacuna;
        if (!string.IsNullOrEmpty(vacunacion.Lote))
        {
            detalle += $". Lote: {vacunacion.Lote}";
        }

        if (vacunacion.ProximaFecha is { } refuerzo)
        {
            detalle += $". Próximo refuerzo: {refuerzo.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
        }

        if (!string.IsNullOrEmpty(vacunacion.Observaciones))
        {
            detalle += $". Observaciones: {vacunacion.Observaciones}";
        }

        return new RegistroClinicoItem
        {
            Fecha = vacunacion.FechaAplicacion,
            Origen = "Vacunacion",
            Detalle = detalle,
            Veterinario = vacunacion.Veterinario?.NombreCompleto ?? string.Empty,
        };
    }
}
