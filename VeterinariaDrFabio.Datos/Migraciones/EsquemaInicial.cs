using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VeterinariaDrFabio.Datos.Contexto;

namespace VeterinariaDrFabio.Datos.Migraciones;

/// <summary>
/// Migración inicial: ejecuta el DDL de diseño (tablas, CHECK, índices, triggers NoBorrar y seed de veterinarios).
/// EF no genera el esquema desde el modelo porque no produce los triggers ni el CHECK GLOB del teléfono (RN-08, RN-14).
/// </summary>
[DbContext(typeof(VeterinariaDbContext))]
[Migration("20261005000000_EsquemaInicial")]
public class EsquemaInicial : Migration
{
    private const string NombreRecurso = "VeterinariaDrFabio.Datos.Migraciones.EsquemaInicial.sql";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(LeerScript());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS TR_Vacunacion_NoBorrar;
            DROP TRIGGER IF EXISTS TR_Procedimiento_NoBorrar;
            DROP TRIGGER IF EXISTS TR_Mascota_NoBorrar;
            DROP TRIGGER IF EXISTS TR_Propietario_NoBorrar;
            DROP TABLE IF EXISTS Recordatorio;
            DROP TABLE IF EXISTS Vacunacion;
            DROP TABLE IF EXISTS Procedimiento;
            DROP TABLE IF EXISTS Mascota;
            DROP TABLE IF EXISTS Propietario;
            DROP TABLE IF EXISTS Veterinario;
            DROP TABLE IF EXISTS Usuario;
            """);
    }

    private static string LeerScript()
    {
        using var flujo = typeof(EsquemaInicial).Assembly.GetManifestResourceStream(NombreRecurso)
            ?? throw new InvalidOperationException($"No se encontró el recurso embebido {NombreRecurso}.");
        using var lector = new StreamReader(flujo);
        return lector.ReadToEnd();
    }
}
