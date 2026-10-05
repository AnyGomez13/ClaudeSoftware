using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using VeterinariaDrFabio.Dominio;
using VeterinariaDrFabio.Negocio.Servicios;

namespace VeterinariaDrFabio.App.Infraestructura;

/// <summary>
/// Puesta en marcha: crea la única cuenta de acceso con <c>--crear-usuario</c> y cierra la aplicación,
/// sin pantalla nueva (RF-01, RNF-05, SUP-07).
/// Sin más argumentos pide usuario y contraseña en una consola; también acepta
/// <c>--crear-usuario &lt;usuario&gt; &lt;contraseña&gt;</c> para scripts de instalación.
/// </summary>
internal static class ComandoCrearUsuario
{
    public const string Bandera = "--crear-usuario";

    private const int AdjuntarConsolaDelProceso = -1;

    public static bool Solicitado(string[] argumentos) =>
        argumentos.Any(a => a.Equals(Bandera, StringComparison.OrdinalIgnoreCase));

    /// <summary>Ejecuta el comando y devuelve el código de salida: 0 si se creó el usuario, 1 si falló.</summary>
    public static int Ejecutar(string[] argumentos, IServiceProvider servicios)
    {
        var parametros = argumentos
            .SkipWhile(a => !a.Equals(Bandera, StringComparison.OrdinalIgnoreCase))
            .Skip(1)
            .ToArray();
        var interactivo = parametros.Length < 2;

        PrepararConsola(interactivo);

        string nombreUsuario;
        string clave;
        if (interactivo)
        {
            Console.WriteLine($"Creación del usuario de {Clinica.Nombre}");
            Console.Write("Usuario: ");
            nombreUsuario = Console.ReadLine() ?? string.Empty;
            clave = LeerClave("Contraseña: ");
            var confirmacion = LeerClave("Repita la contraseña: ");
            if (clave != confirmacion)
            {
                return Terminar("Las contraseñas no coinciden.", 1, interactivo);
            }
        }
        else
        {
            nombreUsuario = parametros[0];
            clave = parametros[1];
        }

        var resultado = servicios.GetRequiredService<IAutenticacionService>().CrearUsuarioInicial(nombreUsuario, clave);
        return resultado.Exito
            ? Terminar("Usuario creado correctamente.", 0, interactivo)
            : Terminar(resultado.Mensaje, 1, interactivo);
    }

    private static void PrepararConsola(bool interactivo)
    {
        // La aplicación es WinExe: se crea una consola propia al pedir datos, o se adjunta la del proceso padre.
        if (interactivo)
        {
            AllocConsole();
        }
        else
        {
            AttachConsole(AdjuntarConsolaDelProceso);
        }

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput(), Console.InputEncoding));
    }

    private static string LeerClave(string indicacion)
    {
        Console.Write(indicacion);
        var clave = new System.Text.StringBuilder();
        while (true)
        {
            var tecla = Console.ReadKey(intercept: true);
            if (tecla.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return clave.ToString();
            }

            if (tecla.Key == ConsoleKey.Backspace)
            {
                if (clave.Length > 0)
                {
                    clave.Length--;
                }
            }
            else if (!char.IsControl(tecla.KeyChar))
            {
                clave.Append(tecla.KeyChar);
            }
        }
    }

    private static int Terminar(string mensaje, int codigo, bool interactivo)
    {
        Console.WriteLine(mensaje);
        if (interactivo)
        {
            Console.WriteLine("Presione una tecla para cerrar.");
            Console.ReadKey(intercept: true);
        }

        return codigo;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int idProceso);
}
