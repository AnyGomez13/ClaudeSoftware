# fases.md — Plan de desarrollo

Convenciones: `Dom` = `VeterinariaDrFabio.Dominio`, `Dat` = `VeterinariaDrFabio.Datos`, `Neg` = `VeterinariaDrFabio.Negocio`, `App` = `VeterinariaDrFabio.App`, `Pru` = `VeterinariaDrFabio.Pruebas`. Rutas de carpetas según `disenoC.md` §1.6. Estados: pendiente | en curso | terminada. Comandos `dotnet` definidos en Fase 1.

## Fase 1 — Solución, proyectos y tooling
- Objetivo: crear la solución con la estructura del diseño, referencias entre proyectos y paquetes gratuitos; fijar versiones y comandos.
- Archivos: `VeterinariaDrFabio.sln`, `*.csproj` de Dom, Dat, Neg, App, Pru; `.gitignore`; `App/App.xaml`, `App/App.xaml.cs` (contenedor DI vacío); `Pru/PruebaHumoTests.cs`; ajustar `CLAUDE.md` §3 y §8.
- Dependencias: ninguna.
- Cubre: RNF-02, RNF-06, SUP-D09, SUP-D13.
- Criterio de terminado: `dotnet build` sin errores ni advertencias; `dotnet test` pasa la prueba de humo; `dotnet format --verify-no-changes` sin diferencias; `CLAUDE.md` con versiones exactas y comandos reales.
- Estado: terminada

## Fase 2 — Dominio: entidades y modelos
- Objetivo: implementar las entidades POCO y los modelos agregados.
- Archivos: `Dom/Entidades/{Usuario,Veterinario,Propietario,Mascota,Procedimiento,Vacunacion,Recordatorio}.cs`; `Dom/Modelos/{CarnetDigital,HistoriaClinica,RegistroClinicoItem}.cs`; `Pru/Dominio/ModelosTests.cs`.
- Dependencias: Fase 1.
- Cubre: objetos §4.1–4.9 de `requisitosC.md`; RN-02; SUP-D04, SUP-D05, SUP-D14, SUP-D15.
- Criterio de terminado: `dotnet build` ok; `dotnet test --filter "Req=RN-02"` verde (propietario con varias mascotas; `Mascota` sin propiedad `Edad` persistente).
- Estado: terminada

## Fase 3 — Datos: DbContext, DDL y migración inicial
- Objetivo: `VeterinariaDbContext`, mapeos por entidad y creación de la BD ejecutando el DDL de `disenoC.md` §2.9 (con CHECK, triggers, índices y seed) mediante una migración inicial.
- Archivos: `Dat/Contexto/VeterinariaDbContext.cs`; `Dat/Configuracion/*Configuration.cs` (una por entidad); `Dat/Migraciones/*Inicial*.cs` y el script DDL embebido; clase de ruta de BD; `Pru/Datos/EsquemaTests.cs`.
- Dependencias: Fase 2.
- Cubre: RNF-03, RNF-07, RNF-08, RN-08, RN-12, RN-14 (a nivel BD), SUP-D01, SUP-D02, SUP-D10.
- Criterio de terminado: `dotnet test --filter "FullyQualifiedName~EsquemaTests"` verde sobre una BD temporal: teléfono `2…` o de 9 dígitos rechazado; `DELETE` en Propietario, Mascota, Procedimiento y Vacunacion aborta; FK RESTRICT activa; `NombreUsuario` único; mascota sin propietario rechazada; seed con Fabio y William; recordatorio sin origen o con dos orígenes rechazado.
- Estado: terminada

## Fase 4 — Repositorios
- Objetivo: interfaces y repositorios EF Core de las 7 entidades con las consultas del diseño.
- Archivos: `Dat/Repositorios/I*Repository.cs` y `*Repository.cs` (Usuario, Veterinario, Propietario, Mascota, Procedimiento, Vacunacion, Recordatorio); `Pru/Datos/*RepositoryTests.cs`.
- Dependencias: Fase 3.
- Cubre: RF-03, RF-06, RF-10, RNF-08, RNF-09 (índices).
- Criterio de terminado: `dotnet test --filter "FullyQualifiedName~RepositoryTests"` verde: alta/edición/inactivación, búsqueda de propietarios por nombre y por teléfono con sus mascotas, historial de mascota ordenado por fecha, `ListarConProximaFecha` en Vacunacion y Procedimiento.
- Estado: terminada

## Fase 5 — Utilidades: edad, hash y enlace WhatsApp
- Objetivo: utilidades puras de Negocio sin dependencia de BD ni UI.
- Archivos: `Neg/Utilidades/{CalculadoraEdad,HasherContrasena,GeneradorEnlaceWhatsApp}.cs`; `Pru/Utilidades/*Tests.cs`.
- Dependencias: Fase 2.
- Cubre: RF-08, RF-15, RNF-05, RNF-06, RN-05, RN-10, RN-14, SUP-D03, SUP-D07, SUP-D15.
- Criterio de terminado: `dotnet test --filter "FullyQualifiedName~Utilidades"` verde: edad correcta en casos de borde (cumpleaños hoy, 29 feb, menor de un mes); hash verifica la contraseña correcta, rechaza la incorrecta y genera sal distinta cada vez; enlace `https://wa.me/57<10 dígitos>?text=…` con mensaje codificado y rechazo de teléfonos no válidos.
- Estado: terminada

## Fase 6 — Autenticación, veterinarios y usuario inicial
- Objetivo: login contra BD, gestión de veterinarios y mecanismo de puesta en marcha para crear el usuario inicial.
- Archivos: `Neg/Servicios/{IAutenticacionService,AutenticacionService,IVeterinarioService,VeterinarioService}.cs`; arranque en `App/App.xaml.cs` con argumento `--crear-usuario`; `Pru/Servicios/{Autenticacion,Veterinario}ServiceTests.cs`.
- Dependencias: Fases 4 y 5.
- Cubre: RF-01, RF-16, RNF-05, RN-01, RN-13, CU-01, CU-13, SUP-06, SUP-07, SUP-D12.
- Criterio de terminado: pruebas verdes: credenciales válidas ok, inválidas rechazadas, usuario inactivo rechazado, cierre de sesión limpia el estado, veterinarios pre-registrados listados y alta de uno nuevo; `grep -rniE "password|contrasena\s*=\s*\"" --include=*.cs` sin credenciales literales fuera de pruebas; `App.exe --crear-usuario` crea un `Usuario` con hash y sal.
- Estado: terminada

## Fase 7 — Servicios de propietario y mascota
- Objetivo: reglas de negocio de propietarios y mascotas.
- Archivos: `Neg/Servicios/{IPropietarioService,PropietarioService,IMascotaService,MascotaService}.cs`; `Pru/Servicios/{Propietario,Mascota}ServiceTests.cs`.
- Dependencias: Fases 4 y 5.
- Cubre: RF-02, RF-03, RF-04, RF-05, RF-07, RF-08, RN-02, RN-03, RN-04, RN-06, RN-14, CU-02, CU-03, CU-04, CU-05, CU-07, RNF-08, SUP-04.
- Criterio de terminado: pruebas verdes: sin nombre o sin teléfono no guarda; teléfono inválido rechazado; mascota sin propietario rechazada; peso ≤ 0 rechazado; fecha estimada aceptada; edad calculada en la consulta; inactivar propietario o mascota no borra datos; búsqueda sin resultados devuelve lista vacía.
- Estado: terminada

## Fase 8 — Servicios de procedimiento, vacunación e historia clínica
- Objetivo: registrar procedimientos y vacunaciones y armar la historia clínica cronológica.
- Archivos: `Neg/Servicios/{IProcedimientoService,ProcedimientoService,IVacunacionService,VacunacionService}.cs`; construcción de `HistoriaClinica` y `RegistroClinicoItem` (en `MascotaService`); `Pru/Servicios/{Procedimiento,Vacunacion,HistoriaClinica}Tests.cs`.
- Dependencias: Fase 7.
- Cubre: RF-06, RF-09, RF-10, RF-11, RN-07, RN-08, RNF-07, RNF-08, RNF-09, CU-06, CU-08, CU-09.
- Criterio de terminado: pruebas verdes: sin veterinario no guarda (procedimiento y vacunación); historia mezcla ambos tipos ordenada por fecha con veterinario en cada ítem; no existe operación de borrado ni de edición de registros clínicos; consulta de historia < 1 s con 10 000 registros en BD de prueba.
- Estado: terminada

## Fase 9 — Carnet PDF, alertas y recordatorios
- Objetivo: carnet digital en PDF, alertas de próximas fechas y recordatorios persistidos.
- Archivos: `Neg/Utilidades/GeneradorCarnetPdf.cs`; `Neg/Servicios/{ICarnetService,CarnetService,IAlertaService,AlertaService,IRecordatorioService,RecordatorioService}.cs`; `Pru/Servicios/{Carnet,Alerta,Recordatorio}Tests.cs`.
- Dependencias: Fases 5 y 8.
- Cubre: RF-12, RF-13, RF-14, RF-15, RN-09, RN-10, RN-11, RNF-04, RNF-06, CU-10, CU-11, CU-12, SUP-01, SUP-09, SUP-D05, SUP-D06, SUP-D08.
- Criterio de terminado: pruebas verdes: carnet sin vacunas informa y no genera; el PDF existe, no está vacío y empieza con `%PDF`; alertas listan próximas (30 días) y vencidas ordenadas por fecha; desparasitación solo desde procedimientos de ese tipo; recordatorio se guarda `Pendiente` con origen único y pasa a `Enviado` con `FechaEnvio`; teléfono inválido se informa.
- Estado: terminada

## Fase 10 — Infraestructura UI, login y shell
- Objetivo: base MVVM, estilos, navegación, DI de la app, P-01 y P-02.
- Archivos: `App/Infraestructura/{RelayCommand,INavigationService,NavigationService,IDialogService,DialogService}.cs`; `App/ViewModels/{BaseViewModel,LoginViewModel,MainViewModel}.cs`; `App/Vistas/LoginView.xaml`; `App/MainWindow.xaml`; `App/Recursos/{Colores,Estilos}.xaml`; `App/App.xaml.cs` (registro completo de DI); `Pru/ViewModels/LoginViewModelTests.cs`.
- Dependencias: Fases 6 y 9 (servicios registrables).
- Cubre: RF-01, RNF-01, RNF-02, RNF-05, CU-01, P-01, P-02.
- Criterio de terminado: `dotnet build` ok; `LoginViewModelTests` verde; ejecución manual con `dotnet run --project VeterinariaDrFabio.App`: credencial incorrecta muestra error, correcta abre el shell con menú Propietarios · Mascotas · Alertas · Veterinarios y Cerrar sesión vuelve al login; sin sesión no hay acceso a vistas.
- Estado: terminada

## Fase 11 — UI Propietarios y Veterinarios
- Objetivo: pantallas P-03, P-04 y P-12.
- Archivos: `App/Vistas/{PropietariosView,PropietarioEdicionView,VeterinariosView}.xaml`; `App/ViewModels/{PropietariosViewModel,PropietarioEdicionViewModel,VeterinariosViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 10.
- Cubre: RF-02, RF-03, RF-04, RF-16, RNF-01, CU-02, CU-03, CU-04, CU-13, P-03, P-04, P-12.
- Criterio de terminado: pruebas de ViewModel verdes (validación en vivo del teléfono, guardar bloqueado sin nombre o teléfono); checklist manual: registrar, buscar por nombre y por teléfono, ver mascotas del propietario, editar, inactivar, ver Fabio y William y agregar un veterinario.
- Estado: terminada

## Fase 12 — UI Mascotas y ficha clínica
- Objetivo: pantallas P-05, P-06 y P-07.
- Archivos: `App/Vistas/{MascotasView,MascotaEdicionView,MascotaDetalleView}.xaml`; `App/ViewModels/{MascotasViewModel,MascotaEdicionViewModel,MascotaDetalleViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 11.
- Cubre: RF-05, RF-06, RF-07, RF-08, RF-10, RNF-01, RNF-09, CU-05, CU-06, CU-07, P-05, P-06, P-07.
- Criterio de terminado: pruebas de ViewModel verdes (guardar bloqueado sin propietario, peso > 0, edad en vivo); checklist manual: crear mascota con propietario existente y con fecha estimada, editar peso, abrir ficha con edad y línea de tiempo con veterinario de cada registro.
- Estado: terminada

## Fase 13 — UI Procedimiento y vacunación
- Objetivo: pantallas P-08 y P-09 abiertas desde la ficha.
- Archivos: `App/Vistas/{ProcedimientoEdicionView,VacunacionEdicionView}.xaml`; `App/ViewModels/{ProcedimientoEdicionViewModel,VacunacionEdicionViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 12.
- Cubre: RF-09, RF-11, RN-07, RNF-01, CU-08, CU-09, P-08, P-09.
- Criterio de terminado: pruebas de ViewModel verdes (sin veterinario no guarda); checklist manual: registrar procedimiento y vacunación con y sin próxima fecha y comprobar que aparecen ordenados en la historia y sin opción de editar ni borrar.
- Estado: terminada

## Fase 14 — UI Carnet y Alertas
- Objetivo: pantallas P-10 y P-11 con descarga de PDF y envío de recordatorio.
- Archivos: `App/Vistas/{CarnetView,AlertasView}.xaml`; `App/ViewModels/{CarnetViewModel,AlertasViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fases 9 y 13.
- Cubre: RF-12, RF-13, RF-14, RF-15, RNF-04, RNF-06, CU-10, CU-11, CU-12, P-10, P-11.
- Criterio de terminado: pruebas de ViewModel verdes; checklist manual: vista previa del carnet y PDF guardado que se abre con vacunas, mascota y propietario; alertas resaltadas (advertencia próximas, error vencidas); "Enviar recordatorio" muestra el mensaje y abre el enlace `wa.me` correcto; sin red se avisa y no se marca enviado; marcar como enviado oculta el aviso repetido.
- Estado: terminada

## Fase 15 — Empaquetado y revisión final
- Objetivo: publicar el ejecutable y cerrar la codificación con una revisión estática. Las pruebas de sistema y de usuario quedan para la fase de pruebas del SDLC (A-17).
- Archivos: instrucciones de puesta en marcha en `README.md`; salida de `dotnet publish`; `trazabilidad.md` (marcar archivos implementados).
- Dependencias: Fases 1 a 14.
- Cubre: RNF-02, RNF-03, RNF-05, RNF-06.
- Criterio de terminado: `dotnet test` completo verde (suite del desarrollador); `dotnet format --verify-no-changes` limpio; `dotnet publish -c Release -r win-x64 --self-contained` genera un `.exe` que arranca, crea la BD en `%LocalAppData%\VeterinariaDrFabioeterinaria.db` y permite `--crear-usuario`; búsqueda en el código sin credenciales literales; revisión de que no existe ninguna pantalla fuera de P-01 a P-12.
- Estado: pendiente

## Cambios solicitados por el usuario después de la Fase 14
Pedidos del cliente sobre lo ya construido. Los documentos fuente (`requisitosC.md`, `disenoC.md`) no se modificaron; estos cambios quedan registrados aquí y en `trazabilidad.md`.

| ID | Cambio | Alcance | Estado |
|---|---|---|---|
| CC-01 | El nombre de la veterinaria es "Villa de San Carlos". | `Dom/Clinica.cs` (constante única) usada en el título de la ventana, el login, el menú lateral, el carnet en PDF, el mensaje de WhatsApp y la consola de `--crear-usuario`. | terminado |
| CC-02 | Listado de vacunas con refuerzo propuesto según la vacuna, editable. | `Neg/Utilidades/CatalogoVacunas.cs`, `Neg/Utilidades/TextoComparable.cs`, `App/ViewModels/VacunacionEdicionViewModel.cs`, `App/Vistas/VacunacionEdicionView.xaml`. | terminado |
| CC-03 | Pantalla de configuración para cambiar la contraseña (P-13, RF-17, CU-14). | `IAutenticacionService.CambiarContrasena`, `IUsuarioRepository.Actualizar`, `App/ViewModels/ConfiguracionViewModel.cs`, `App/Vistas/ConfiguracionView.xaml`, sección "Configuración" del menú. | terminado |
| CC-04 | Botón para volver atrás en la ficha de la mascota. | `MascotaDetalleViewModel.VolverCommand`, botón "← Volver" en `MascotaDetalleView.xaml`. | terminado |

## Supuestos y contradicciones
Contradicciones técnicas que bloqueen: ninguna.

| ID | Supuesto | Justificación |
|---|---|---|
| A-01 | El archivo de diseño es `disenoC.md`, no `diseñoC.md` ni `diseño.md`. | Es el nombre real en el repositorio. |
| A-02 | Prioridad Alta = Must y Media = Should. | `requisitosC.md` usa Alta/Media y no MoSCoW. |
| A-03 | Versiones de EF Core, QuestPDF, provider SQLite, DI y xUnit se fijan en Fase 1. | El diseño solo fija .NET 8 (LTS). |
| A-04 | Pruebas con xUnit en `VeterinariaDrFabio.Pruebas` y lint con `dotnet format --verify-no-changes`. | Sin pruebas ni lint el criterio de terminado no sería verificable; ambos son gratuitos. |
| A-05 | La BD se crea con una migración inicial que ejecuta el DDL de `disenoC.md` §2.9. | El diseño lista `Migraciones/` y un DDL con CHECK GLOB y triggers que EF no genera por modelo. |
| A-06 | El usuario inicial se crea con `--crear-usuario` al arrancar la app, sin pantalla nueva. | SUP-07 lo pide en la puesta en marcha y P-01 no tiene registro. |
| A-07 | La BD vive en `%LocalAppData%\VeterinariaDrFabio\veterinaria.db`. | El diseño no define la ubicación. |
| A-08 | "Próximas" son las fechas dentro de los 30 días siguientes, más las vencidas; el valor es una constante en `AlertaService`. | RF-14 no define umbral. |
| A-09 | Solo los procedimientos de tipo "desparasitación" (sin distinguir tildes ni mayúsculas) generan alerta y recordatorio. | `Recordatorio.Tipo` solo admite Vacunacion/Desparasitacion y `TipoProcedimiento` es texto libre (SUP-D11). |
| A-10 | El mensaje de WhatsApp usa una plantilla fija con propietario, mascota, tipo y fecha. | El diseño no define el texto. |
| A-11 | Antes de abrir el enlace se consulta `NetworkInterface.GetIsNetworkAvailable()`; sin red se avisa y no se marca como enviado. | CU-12 (1a) pide posponer el envío sin red. |
| A-12 | El PDF se guarda con `Microsoft.Win32.SaveFileDialog` y `Generar(carnet, ruta)`. | RF-13 pide descarga; el diálogo viene con WPF, sin costo. |
| A-13 | Procedimientos y vacunaciones no se editan ni se borran desde la UI. | RF-10 y RN-08 exigen conservar el histórico; P-08 y P-09 solo crean. |
| A-14 | "Pocos segundos" de RNF-09 se verifica como < 1 s con 10 000 registros clínicos. | Necesita un criterio medible. |
| A-15 | Solo Windows (WPF); el empaquetado es `dotnet publish` self-contained win-x64 y no se crea instalador. | El diseño no especifica instalador. |
| A-16 | Commits en español con formato `Fase N: <resumen>`. | Los commits existentes están en español. |
| A-17 | La Fase 15 no incluye el recorrido manual de CU-01 a CU-13 ni pruebas de sistema o de usuario; son de la fase de pruebas del SDLC. | Decisión del usuario: xUnit y `dotnet format` son herramientas de codificación y no sustituyen esa fase. |
| A-18 | Las pruebas apuntan a `net8.0-windows` porque referencian el proyecto WPF `App` (necesario para probar ViewModels en las Fases 10 a 14). | Un proyecto `net8.0` no puede referenciar uno `net8.0-windows`. |
| A-19 | La solución usa formato `.sln` (no `.slnx`) y el SDK instalado es 9.0.313 compilando hacia `net8.0`. | `.sln` es compatible con `dotnet format` y el IDE; el diseño exige .NET 8 como destino, no como SDK. |
| A-20 | Las entidades llevan propiedades de navegación (`Propietario.Mascotas`, `Mascota.Propietario`, etc.) además de las columnas de §3.2. | §3.2 dibuja esas relaciones; EF Core las usa en la Fase 3 y no agregan columnas. |
| A-21 | Las columnas NULL se tipan `string?`, `double?` o `DateTime?`; los textos NOT NULL se inicializan con `string.Empty`. | Con `Nullable` activo el build debe quedar sin advertencias. |
| A-22 | `Sexo`, `Tipo` y `Estado` quedan como `string`, sin enums ni constantes. | Así figuran en §3.2 y en los CHECK del DDL. |
| A-23 | Las fechas usan convertidores explícitos: `yyyy-MM-dd` para fechas y `yyyy-MM-ddTHH:mm:ss` para `Recordatorio.FechaEnvio`. | Garantiza el formato exacto de SUP-D01 y que las comparaciones de texto sigan el orden cronológico. |
| A-24 | La migración `EsquemaInicial` se escribió a mano y no tiene `ModelSnapshot`. | El esquema lo define el DDL del diseño; EF solo lo aplica y lo mapea. |
| A-25 | La cadena de conexión se arma con `Foreign Keys=True` (clase `RutaBaseDatos`). | El `PRAGMA foreign_keys` del DDL no tiene efecto dentro de la transacción de la migración. |
| A-26 | Se agregaron a las interfaces del diseño: `IUsuarioRepository.Agregar` y, en `IVeterinarioRepository`, `ListarTodos`, `ObtenerPorId` y `Actualizar`. | `--crear-usuario` (A-06) necesita guardar el usuario, y P-12 muestra el estado de todos los veterinarios y permite editarlos. |
| A-27 | Los repositorios son síncronos, reciben el `VeterinariaDbContext` por constructor y guardan en cada `Agregar`/`Actualizar`. Los listados por mascota salen en orden cronológico ascendente. | §3.2 define los métodos sin `async` ni unidad de trabajo; la historia clínica se lee de la más antigua a la más reciente. |
| A-28 | `Buscar` usa coincidencia parcial con `Contains` (LIKE de SQLite) e incluye inactivos; texto vacío devuelve todos. | P-03 y P-05 muestran la columna Estado; SQLite ignora mayúsculas solo en ASCII, así que "perez" no encuentra "Pérez". |
| A-29 | `CalculadoraEdad` agrega sobrecargas con la fecha de referencia (`EnMeses(nacimiento, hoy)`, `Legible(nacimiento, hoy)`) además de las firmas del diseño. | Permite probar casos de borde sin depender del reloj; las firmas originales usan `DateTime.Today`. |
| A-30 | Un mes se cumple el mismo día del mes, o el último día si ese día no existe (31 de enero, 29 de febrero). Una fecha de nacimiento futura lanza `ArgumentOutOfRangeException`; la Fase 7 debe rechazarla antes de calcular. | El diseño no define el cálculo; evita edades negativas o "Menos de 1 mes" para una mascota que no ha nacido. |
| A-31 | `GeneradorEnlaceWhatsApp.Construir` lanza `ArgumentException` si el teléfono no cumple `3` + 9 dígitos o el mensaje está vacío; el texto del mensaje lo arma `RecordatorioService` (Fase 9). | La utilidad solo codifica; el diseño le pasa el mensaje ya escrito (`Construir(telefono, mensaje)`). |
| A-32 | `Resultado` (éxito y mensaje) se define en `Neg/Servicios/Resultado.cs`. | Las interfaces de §3.2 devuelven `Resultado` pero el diseño no lo declara. |
| A-33 | `IAutenticacionService` agrega `SesionActiva`, `NombreUsuario` y `CrearUsuarioInicial`; `IUsuarioRepository` agrega `HayUsuarios`; `IVeterinarioService` agrega `ListarTodos` y `Editar`. | Hacen falta para que sin sesión no haya acceso (RN-13), para `--crear-usuario` con un único usuario (R-03) y para P-12. |
| A-34 | `--crear-usuario` sin más argumentos pide usuario y contraseña (dos veces) en una consola propia; con `--crear-usuario <usuario> <contraseña>` actúa sin interacción para scripts. No exige longitud mínima de contraseña. | Concreta A-06 sin agregar pantallas; los requisitos no definen política de contraseñas. |
| A-35 | Provisional hasta la Fase 10: `DbContext`, repositorios y servicios se registran como transitorios (un contexto por resolución) y `AutenticacionService` como único. | La sesión debe vivir mientras la app esté abierta; un contexto por resolución evita que un error al guardar contamine a los demás. |
| A-36 | La app crea o migra la BD en cada arranque (`Database.Migrate()`), incluido el de `--crear-usuario`. | El primer arranque debe crear `veterinaria.db` sin pasos manuales (RNF-03). |
| A-37 | Los servicios agregan `ObtenerPorId` (propietario y mascota) y `CalcularEdadLegible` (mascota) a las firmas de §3.2. | P-03, P-04, P-06 y P-07 necesitan cargar un registro y mostrar la edad en texto en vivo sin que el ViewModel use utilidades directamente. |
| A-38 | El teléfono se valida de forma estricta tras quitar espacios en los extremos: exactamente `3` + 9 dígitos, sin `+57` ni separadores internos. | RN-14 y el CHECK de la BD; la pantalla P-04 valida el formato `3#########` en vivo. |
| A-39 | Reglas de mascota: propietario existente, nombre, especie, fecha de nacimiento no futura (A-30), peso > 0 y finito, sexo vacío, Macho o Hembra (sin distinguir mayúsculas). Se permite registrar mascotas de un propietario inactivo y cambiar el propietario al editar. | Derivan de RN-03, RN-06, SUP-03, SUP-04 y de los CHECK del DDL; los requisitos no prohíben lo demás. |
| A-40 | `IMascotaService` agrega `ObtenerHistoriaClinica(mascotaId)`, que fusiona procedimientos y vacunaciones en un `HistoriaClinica` (orden ascendente por fecha; en la misma fecha, procedimiento antes que vacunación). Con esto el ViewModel no usa repositorios (§3.4 CU-06 los llamaba desde el ViewModel). | Mantiene la regla de capas y deja la fusión probada en Negocio. Sin registros, `FechaApertura` queda en `default`; el ViewModel debe revisar `Registros.Count`. |
| A-41 | `RegistroClinicoItem.Detalle` se arma así: procedimiento = `Tipo: Descripción` + `. Tratamiento: …` + `. Peso: N kg`; vacunación = `Vacuna` + `. Lote: …` + `. Próximo refuerzo: dd/MM/aaaa` + `. Observaciones: …`. `Origen` vale `Procedimiento` o `Vacunacion`. | El diseño define los campos pero no el texto que muestra la línea de tiempo de P-07. |
| A-42 | Reglas de los registros clínicos: mascota existente; veterinario existente y activo; fecha no futura (hoy vale); procedimiento exige tipo y descripción; vacunación exige nombre de la vacuna; peso informado > 0; la próxima fecha no puede ser anterior a la del registro. Al registrar se ignoran las propiedades de navegación recibidas. | Derivan de RN-07, RNF-08 y del selector de P-08/P-09 (solo veterinarios activos); un acto médico futuro o un refuerzo anterior a la aplicación son datos incoherentes. |
| A-43 | `ListarPorMascota` de `ProcedimientoRepository` y `VacunacionRepository` lee sin seguimiento de cambios (`AsNoTracking`). | Con seguimiento, 10 000 registros tardaron 1322 ms (falló RNF-09, A-14); sin seguimiento tardan ~100 ms. Es una lista de solo lectura. |
| A-44 | `IAlertaService.ProximasFechas` devuelve un modelo nuevo, `AlertaProximaFecha` (mascota, propietario, teléfono, tipo, detalle, fecha, vencida, origen), y `IRecordatorioService.Preparar` lo recibe; en §3.2 ambos usaban `RegistroClinicoItem`, que no trae mascota, propietario ni origen. | El propio CU-11 de §3.4 dice que la lista lleva "mascota, propietario, tipo, fecha" y P-11 muestra esas columnas; `RegistroClinicoItem` no puede darlas ni permite enlazar el recordatorio con su origen. |
| A-45 | `ICarnetService.Generar` y `ExportarPdf` devuelven `Resultado<T>` (nuevo, `ResultadoGenerico.cs`); `ExportarPdf` recibe la ruta elegida por el usuario (A-12); se agrega `NombreArchivoSugerido`. `MarcarEnviado` devuelve `Resultado`. | CU-10 y CU-12 piden informar errores (sin vacunas, ruta no escribible, recordatorio sin preparar) y §3.2 devolvía `string`/`void`. |
| A-46 | Alertas: próximas = hasta 30 días (`AlertaService.DiasDeAnticipacion`, A-08) y todas las vencidas. Se omiten las de mascotas inactivas, las que ya tienen un recordatorio Enviado y las dejadas sin efecto por una aplicación posterior (misma vacuna para la misma mascota; o una desparasitación posterior). `AlertaService` también depende de `IRecordatorioRepository` (nuevo `ListarEnviados`). | RF-14 solo dice "próxima a vencerse"; sin estas reglas la lista se llenaría de fechas vencidas que ya se atendieron. Está pendiente de tu confirmación. |
| A-47 | Texto del recordatorio: "Hola {propietario}, le saludamos de la Clínica Veterinaria Dr. Fabio. Le recordamos que a {mascota} le corresponde/correspondía {el refuerzo de la vacuna X / la desparasitación} el {dd/MM/aaaa}. Lo esperamos en la clínica." Preparar reutiliza el recordatorio Pendiente del mismo origen en lugar de duplicarlo. | Concreta A-10 y SUP-D06; evita la palabra "cita", fuera de alcance. |
| A-48 | Se agregan `IConectividad` y `ConectividadRed` (`NetworkInterface.GetIsNetworkAvailable()`) en `Neg/Utilidades`. `Preparar` valida primero el teléfono y luego la red; sin red no guarda nada. | Concreta A-11 y permite probar RNF-04 con una conectividad falsa. |
| A-49 | `GeneradorCarnetPdf.ConstruirDocumento` es interno (`InternalsVisibleTo` a las pruebas) para dibujar el carnet como imagen PNG y revisarlo sin un visor de PDF. | El visor PDF de Edge en modo sin interfaz no pinta; la imagen sale del mismo documento. |
| A-50 | El login (P-01) no es una ventana aparte: `LoginView` es un `UserControl` dentro de `MainWindow`, y `MainViewModel` muestra el login mientras no haya sesión y el shell (P-02) después. | §4.3 define P-01 y P-02 pero no cómo se encadenan; así sin sesión no existe ninguna vista a la que navegar (RN-13). |
| A-51 | `INavigationService` agrega `NavegarASeccion`, `Volver`/`PuedeVolver` y `Limpiar`, y `NavegarA` lanza `InvalidOperationException` sin sesión. La vista que se muestra se decide por el ViewModel actual; cada fase de pantallas (11 a 14) registra su ViewModel en la inyección de dependencias y su `DataTemplate` ViewModel→Vista. | §1.3 solo nombra el servicio; los formularios se abren desde la ficha y necesitan volver (Cancelar/Guardar). |
| A-52 | Mientras una sección no tiene pantalla, el área de contenido muestra el texto provisional "Esta sección se habilita en las siguientes fases del desarrollo." Se elimina cuando se implementen las Fases 11 a 14. | Sin ese texto el shell se vería vacío; no es una pantalla nueva ni forma parte del diseño final. |
| A-53 | El botón Ingresar se habilita solo con usuario y contraseña escritos; el error se muestra como "Credenciales inválidas" (texto de §3.4 CU-01) y vacía la contraseña. `PasswordBox` no admite binding, por lo que el código de la vista (`LoginView.xaml.cs`) solo sincroniza la contraseña con el ViewModel. | Es el patrón MVVM habitual para contraseñas; §3.4 CU-01 fija el mensaje. |
| A-54 | `INavigationService`, `IDialogService`, `LoginViewModel` y `MainViewModel` se registran como únicos; `IDialogService` ya incluye `MostrarError` y `PedirRutaDeGuardado` (A-12) aunque aún no se usen. | La sesión y la sección actual deben vivir mientras la app esté abierta; los diálogos se necesitan desde la Fase 11. |
| A-55 | El proyecto de pruebas usa `UseWPF` y un hilo STA único (`HiloUi`) que carga la aplicación y los recursos reales para probar el XAML fuera de pantalla: estados de cada vista, botones habilitados, color de la sección activa y ausencia de errores de binding. | Los errores de XAML y de binding solo aparecen al ejecutar; las pruebas de ViewModel no los detectan. Esto encontró y corrigió el resaltado del menú, que no se aplicaba. |
| A-56 | P-03 busca mientras se escribe, sin botón Buscar. Cada fila tiene Ver (selecciona y muestra las mascotas del propietario) y Editar; la lista de mascotas muestra Nombre, Especie y Estado. | RNF-01 pide pocos pasos; §4.3 pide "Ver/Editar" por fila y listar las mascotas al seleccionar, sin definir columnas. |
| A-57 | P-04 ofrece "Inactivar" y, si el propietario ya está inactivo, "Reactivar" con el mismo botón; inactivar pide confirmación y reactivar no. El aviso en vivo del celular aparece solo cuando hay texto con formato inválido; Guardar se habilita con nombre y teléfono escritos y el servicio aplica la regla completa. | §4.3 solo nombra Inactivar; sin Reactivar un propietario inactivo quedaría así para siempre. La validación de campos vacíos en la interfaz es la de §1.2. |
| A-58 | P-12 no abre otra pantalla: el alta y la edición usan un panel a la derecha de la lista, con casilla Activo solo al editar. | §4.3 dice "botón Nuevo veterinario y edición" sin pantalla propia para ello (y las pantallas están cerradas a P-01..P-12). |
| A-59 | `ComposicionDeServicios` (nuevo, `App/Infraestructura`) concentra el registro de dependencias para que las pruebas usen el mismo que la aplicación; `PropietarioService.EsCelularColombiano` es pública para la validación en vivo. | Evita duplicar el registro y la expresión regular del celular, y una prueba verifica que todo se puede crear. |
| A-60 | La asociación ViewModel→Vista se declara con `DataTemplate` en `MainWindow.xaml`. El menú abre Propietarios y Veterinarios; Mascotas y Alertas siguen con el texto provisional (A-52) hasta las Fases 12 y 14. | Cumple A-51; cada fase agrega su plantilla y su entrada en `MainViewModel`. |
| A-61 | P-05 agrega "Editar" por fila junto a "Abrir ficha", y la lista busca mientras se escribe. | CU-07 exige llegar a editar una mascota y §4.3 solo nombra "Abrir ficha"; P-06 es "nueva/editar". |
| A-62 | Los botones "Nuevo procedimiento" y "Registrar vacuna" de P-07 se agregan en la Fase 13 y "Generar carnet" en la Fase 14, con sus pantallas, para no dejar botones sin función. `MascotaDetalleViewModel.Recargar()` ya existe para refrescar la ficha al volver de esos formularios. La ficha no tiene botón Volver: se regresa con el menú Mascotas. | §4.3 los ubica en P-07 pero sus destinos (P-08, P-09, P-10) pertenecen a esas fases. |
| A-63 | La historia clínica se muestra de la más antigua a la más reciente (orden cronológico ascendente, A-27), con Tipo "Procedimiento" o "Vacunación" y el veterinario de cada línea. | RF-06 y RF-10 piden orden por fecha; el sentido no estaba definido. |
| A-64 | P-06: el selector ofrece los propietarios activos y, al editar, también el actual aunque esté inactivo; sin propietarios se avisa que hay que crear uno primero. El peso admite coma o punto decimal. Especie es un campo editable con sugerencias (Perro, Gato, Ave, Conejo, SUP-D11); Sexo ofrece "No especificado", Macho y Hembra; el calendario no permite fechas futuras. Inactivar/Reactivar funciona como en P-04 (A-57). | Concreta RN-03, RN-06, SUP-04 y SUP-D11; la coma decimal es la habitual en Colombia. |
| A-65 | La aplicación ajusta el idioma de WPF a la cultura del equipo al arrancar. | Sin esto WPF formatea fechas y números como en-US en las vistas. |
| A-66 | Los selectores desplegables (`ComboBox`) y el calendario (`DatePicker`) usan el aspecto estándar de Windows, distinto del estilo plano del resto. | Un estilo propio exige reescribir su plantilla completa y no cambia la funcionalidad; queda como ajuste visual pendiente, a tu decisión. |
| A-67 | La ficha (P-07) ahora tiene "Nuevo procedimiento" y "Registrar vacuna". Al guardar o cancelar se vuelve a la ficha, que se recarga si se guardó; "Generar carnet" se agrega en la Fase 14. | Cumple A-62. |
| A-68 | En P-08 y P-09 el veterinario no viene preseleccionado: hay que elegirlo, y solo se ofrecen los activos. La fecha parte en hoy y no admite fechas futuras; el peso de P-08 es opcional y admite coma o punto. Tipo y vacuna son campos editables con sugerencias (Consulta, Cirugía, Control, Desparasitación; Rabia, Parvovirus, Moquillo, Triple canina, Triple felina) y admiten texto libre (SUP-10, SUP-D11). Las sugerencias son orientativas y las puedes cambiar. | RN-07 exige atribuir cada registro a un veterinario; el diseño no fija las listas de sugerencias. "Desparasitación" se escribe así para que genere alertas (A-09). |
| A-69 | En P-08 los botones Guardar y Cancelar y el mensaje de error están en una barra fija bajo la tarjeta, porque con la ventana en su tamaño mínimo (680 px) el formulario no cabe y los botones quedaban fuera de la vista. | Un error de validación fuera de la vista pasaría desapercibido. |
| A-70 | P-10 agrega un botón "Volver" a la ficha junto a "Descargar PDF". Si la mascota no tiene vacunas o ya no existe, en lugar de la vista previa se muestra el aviso del servicio. Tras descargar se informa la ruta del archivo; si se cancela el diálogo de guardado no pasa nada. | §4.3 solo nombra "Descargar PDF" y el aviso sin vacunas; sin "Volver" habría que rehacer la búsqueda para regresar a la ficha. |
| A-71 | P-11 resalta cada alerta con una insignia: roja "Vencida" (error) y ámbar "Próxima" (advertencia). "Enviar recordatorio" prepara el mensaje, abre el enlace de WhatsApp de inmediato y deja a la vista un panel con el mensaje, "Marcar como enviado" y "Cerrar". Si no se puede abrir el navegador se avisa y el mensaje queda en el panel para enviarlo a mano. Marcar como enviado quita la alerta de la lista. | Concreta CU-12 (§4.3: "muestra el mensaje armado y abre el enlace"); la insignia garantiza contraste en ambos colores. |
| A-72 | Se agrega `IAbridorDeEnlaces` (`AbridorDeEnlaces`) en `App/Infraestructura`: abre solo direcciones http o https con el navegador predeterminado y rechaza cualquier otra (archivos, `javascript:`, correos). | Evita que una dirección armada con datos del usuario ejecute algo distinto de WhatsApp, y permite probar el envío sin abrir el navegador. |
| A-73 | Se eliminó el texto provisional "Esta sección se habilita en las siguientes fases del desarrollo." (A-52): las cuatro secciones del menú ya tienen pantalla. | Cumple lo previsto en A-52. |
| A-74 | El nombre "Villa de San Carlos" vive en `Clinica.Nombre` (Dominio) y se usa tal cual, sin prefijo como "Clínica" o "Veterinaria". Los nombres internos (solución `VeterinariaDrFabio`, carpeta de datos `%LocalAppData%\VeterinariaDrFabio`) no cambian. | Evita repetir el texto en siete lugares; renombrar la carpeta de datos dejaría sin acceso a la base ya creada. Si prefieres otro texto (por ejemplo "Veterinaria Villa de San Carlos") se cambia en un solo archivo. |
| A-75 | Intervalos de refuerzo propuestos: Rabia, Triple canina, Séxtuple canina, Parvovirus, Moquillo, Leptospirosis, Triple felina y Leucemia felina a 12 meses; Bordetella a 6 meses. El listado se filtra por especie (perro, gato; con otra especie se ofrecen todas) y admite texto libre. **Son valores iniciales propuestos por el desarrollador y deben ser revisados por los veterinarios (Fabio y William).** | El diseño no tenía listado ni intervalos (SUP-10 decía "texto o selección simple"). En pantalla la fecha siempre se puede cambiar. Sustituye la lista de A-68. |
| A-76 | Regla de la fecha de refuerzo: mientras el usuario no escriba la suya, se recalcula al cambiar la vacuna o la fecha de aplicación (aplicación + meses de la vacuna; una vacuna fuera del listado deja la fecha vacía). Desde que el usuario la cambia o la borra, ya no se sobrescribe. Sigue siendo opcional. | Es lo que hace ágil el registro sin pisar lo que el veterinario decidió. |
| A-77 | Cambiar la contraseña exige la contraseña actual, una nueva no vacía y distinta de la actual, y repetirla igual. No hay longitud mínima ni reglas de complejidad (igual que al crearla con `--crear-usuario`, A-34). La sesión sigue abierta tras el cambio. Si se olvida la contraseña actual no hay forma de recuperarla desde la aplicación. | El pedido es "modificar la contraseña en caso de ser necesario"; una política de contraseñas no estaba solicitada. |
| A-78 | "Configuración" es la quinta sección del menú lateral (P-13); el menú del diseño tenía cuatro. | Nueva pantalla pedida por el usuario; `CLAUDE.md` §7 la incluye como excepción a P-01..P-12. |
| A-79 | La ficha (P-07) ahora tiene "← Volver", que regresa a la lista de mascotas conservando la búsqueda. Reemplaza lo dicho en A-62 ("la ficha no tiene botón Volver"). | Pedido del usuario. |
