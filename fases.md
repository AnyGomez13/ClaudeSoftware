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
- Estado: pendiente

## Fase 4 — Repositorios
- Objetivo: interfaces y repositorios EF Core de las 7 entidades con las consultas del diseño.
- Archivos: `Dat/Repositorios/I*Repository.cs` y `*Repository.cs` (Usuario, Veterinario, Propietario, Mascota, Procedimiento, Vacunacion, Recordatorio); `Pru/Datos/*RepositoryTests.cs`.
- Dependencias: Fase 3.
- Cubre: RF-03, RF-06, RF-10, RNF-08, RNF-09 (índices).
- Criterio de terminado: `dotnet test --filter "FullyQualifiedName~RepositoryTests"` verde: alta/edición/inactivación, búsqueda de propietarios por nombre y por teléfono con sus mascotas, historial de mascota ordenado por fecha, `ListarConProximaFecha` en Vacunacion y Procedimiento.
- Estado: pendiente

## Fase 5 — Utilidades: edad, hash y enlace WhatsApp
- Objetivo: utilidades puras de Negocio sin dependencia de BD ni UI.
- Archivos: `Neg/Utilidades/{CalculadoraEdad,HasherContrasena,GeneradorEnlaceWhatsApp}.cs`; `Pru/Utilidades/*Tests.cs`.
- Dependencias: Fase 2.
- Cubre: RF-08, RF-15, RNF-05, RNF-06, RN-05, RN-10, RN-14, SUP-D03, SUP-D07, SUP-D15.
- Criterio de terminado: `dotnet test --filter "FullyQualifiedName~Utilidades"` verde: edad correcta en casos de borde (cumpleaños hoy, 29 feb, menor de un mes); hash verifica la contraseña correcta, rechaza la incorrecta y genera sal distinta cada vez; enlace `https://wa.me/57<10 dígitos>?text=…` con mensaje codificado y rechazo de teléfonos no válidos.
- Estado: pendiente

## Fase 6 — Autenticación, veterinarios y usuario inicial
- Objetivo: login contra BD, gestión de veterinarios y mecanismo de puesta en marcha para crear el usuario inicial.
- Archivos: `Neg/Servicios/{IAutenticacionService,AutenticacionService,IVeterinarioService,VeterinarioService}.cs`; arranque en `App/App.xaml.cs` con argumento `--crear-usuario`; `Pru/Servicios/{Autenticacion,Veterinario}ServiceTests.cs`.
- Dependencias: Fases 4 y 5.
- Cubre: RF-01, RF-16, RNF-05, RN-01, RN-13, CU-01, CU-13, SUP-06, SUP-07, SUP-D12.
- Criterio de terminado: pruebas verdes: credenciales válidas ok, inválidas rechazadas, usuario inactivo rechazado, cierre de sesión limpia el estado, veterinarios pre-registrados listados y alta de uno nuevo; `grep -rniE "password|contrasena\s*=\s*\"" --include=*.cs` sin credenciales literales fuera de pruebas; `App.exe --crear-usuario` crea un `Usuario` con hash y sal.
- Estado: pendiente

## Fase 7 — Servicios de propietario y mascota
- Objetivo: reglas de negocio de propietarios y mascotas.
- Archivos: `Neg/Servicios/{IPropietarioService,PropietarioService,IMascotaService,MascotaService}.cs`; `Pru/Servicios/{Propietario,Mascota}ServiceTests.cs`.
- Dependencias: Fases 4 y 5.
- Cubre: RF-02, RF-03, RF-04, RF-05, RF-07, RF-08, RN-02, RN-03, RN-04, RN-06, RN-14, CU-02, CU-03, CU-04, CU-05, CU-07, RNF-08, SUP-04.
- Criterio de terminado: pruebas verdes: sin nombre o sin teléfono no guarda; teléfono inválido rechazado; mascota sin propietario rechazada; peso ≤ 0 rechazado; fecha estimada aceptada; edad calculada en la consulta; inactivar propietario o mascota no borra datos; búsqueda sin resultados devuelve lista vacía.
- Estado: pendiente

## Fase 8 — Servicios de procedimiento, vacunación e historia clínica
- Objetivo: registrar procedimientos y vacunaciones y armar la historia clínica cronológica.
- Archivos: `Neg/Servicios/{IProcedimientoService,ProcedimientoService,IVacunacionService,VacunacionService}.cs`; construcción de `HistoriaClinica` y `RegistroClinicoItem` (en `MascotaService`); `Pru/Servicios/{Procedimiento,Vacunacion,HistoriaClinica}Tests.cs`.
- Dependencias: Fase 7.
- Cubre: RF-06, RF-09, RF-10, RF-11, RN-07, RN-08, RNF-07, RNF-08, RNF-09, CU-06, CU-08, CU-09.
- Criterio de terminado: pruebas verdes: sin veterinario no guarda (procedimiento y vacunación); historia mezcla ambos tipos ordenada por fecha con veterinario en cada ítem; no existe operación de borrado ni de edición de registros clínicos; consulta de historia < 1 s con 10 000 registros en BD de prueba.
- Estado: pendiente

## Fase 9 — Carnet PDF, alertas y recordatorios
- Objetivo: carnet digital en PDF, alertas de próximas fechas y recordatorios persistidos.
- Archivos: `Neg/Utilidades/GeneradorCarnetPdf.cs`; `Neg/Servicios/{ICarnetService,CarnetService,IAlertaService,AlertaService,IRecordatorioService,RecordatorioService}.cs`; `Pru/Servicios/{Carnet,Alerta,Recordatorio}Tests.cs`.
- Dependencias: Fases 5 y 8.
- Cubre: RF-12, RF-13, RF-14, RF-15, RN-09, RN-10, RN-11, RNF-04, RNF-06, CU-10, CU-11, CU-12, SUP-01, SUP-09, SUP-D05, SUP-D06, SUP-D08.
- Criterio de terminado: pruebas verdes: carnet sin vacunas informa y no genera; el PDF existe, no está vacío y empieza con `%PDF`; alertas listan próximas (30 días) y vencidas ordenadas por fecha; desparasitación solo desde procedimientos de ese tipo; recordatorio se guarda `Pendiente` con origen único y pasa a `Enviado` con `FechaEnvio`; teléfono inválido se informa.
- Estado: pendiente

## Fase 10 — Infraestructura UI, login y shell
- Objetivo: base MVVM, estilos, navegación, DI de la app, P-01 y P-02.
- Archivos: `App/Infraestructura/{RelayCommand,INavigationService,NavigationService,IDialogService,DialogService}.cs`; `App/ViewModels/{BaseViewModel,LoginViewModel,MainViewModel}.cs`; `App/Vistas/LoginView.xaml`; `App/MainWindow.xaml`; `App/Recursos/{Colores,Estilos}.xaml`; `App/App.xaml.cs` (registro completo de DI); `Pru/ViewModels/LoginViewModelTests.cs`.
- Dependencias: Fases 6 y 9 (servicios registrables).
- Cubre: RF-01, RNF-01, RNF-02, RNF-05, CU-01, P-01, P-02.
- Criterio de terminado: `dotnet build` ok; `LoginViewModelTests` verde; ejecución manual con `dotnet run --project VeterinariaDrFabio.App`: credencial incorrecta muestra error, correcta abre el shell con menú Propietarios · Mascotas · Alertas · Veterinarios y Cerrar sesión vuelve al login; sin sesión no hay acceso a vistas.
- Estado: pendiente

## Fase 11 — UI Propietarios y Veterinarios
- Objetivo: pantallas P-03, P-04 y P-12.
- Archivos: `App/Vistas/{PropietariosView,PropietarioEdicionView,VeterinariosView}.xaml`; `App/ViewModels/{PropietariosViewModel,PropietarioEdicionViewModel,VeterinariosViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 10.
- Cubre: RF-02, RF-03, RF-04, RF-16, RNF-01, CU-02, CU-03, CU-04, CU-13, P-03, P-04, P-12.
- Criterio de terminado: pruebas de ViewModel verdes (validación en vivo del teléfono, guardar bloqueado sin nombre o teléfono); checklist manual: registrar, buscar por nombre y por teléfono, ver mascotas del propietario, editar, inactivar, ver Fabio y William y agregar un veterinario.
- Estado: pendiente

## Fase 12 — UI Mascotas y ficha clínica
- Objetivo: pantallas P-05, P-06 y P-07.
- Archivos: `App/Vistas/{MascotasView,MascotaEdicionView,MascotaDetalleView}.xaml`; `App/ViewModels/{MascotasViewModel,MascotaEdicionViewModel,MascotaDetalleViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 11.
- Cubre: RF-05, RF-06, RF-07, RF-08, RF-10, RNF-01, RNF-09, CU-05, CU-06, CU-07, P-05, P-06, P-07.
- Criterio de terminado: pruebas de ViewModel verdes (guardar bloqueado sin propietario, peso > 0, edad en vivo); checklist manual: crear mascota con propietario existente y con fecha estimada, editar peso, abrir ficha con edad y línea de tiempo con veterinario de cada registro.
- Estado: pendiente

## Fase 13 — UI Procedimiento y vacunación
- Objetivo: pantallas P-08 y P-09 abiertas desde la ficha.
- Archivos: `App/Vistas/{ProcedimientoEdicionView,VacunacionEdicionView}.xaml`; `App/ViewModels/{ProcedimientoEdicionViewModel,VacunacionEdicionViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fase 12.
- Cubre: RF-09, RF-11, RN-07, RNF-01, CU-08, CU-09, P-08, P-09.
- Criterio de terminado: pruebas de ViewModel verdes (sin veterinario no guarda); checklist manual: registrar procedimiento y vacunación con y sin próxima fecha y comprobar que aparecen ordenados en la historia y sin opción de editar ni borrar.
- Estado: pendiente

## Fase 14 — UI Carnet y Alertas
- Objetivo: pantallas P-10 y P-11 con descarga de PDF y envío de recordatorio.
- Archivos: `App/Vistas/{CarnetView,AlertasView}.xaml`; `App/ViewModels/{CarnetViewModel,AlertasViewModel}.cs`; `Pru/ViewModels/*Tests.cs`.
- Dependencias: Fases 9 y 13.
- Cubre: RF-12, RF-13, RF-14, RF-15, RNF-04, RNF-06, CU-10, CU-11, CU-12, P-10, P-11.
- Criterio de terminado: pruebas de ViewModel verdes; checklist manual: vista previa del carnet y PDF guardado que se abre con vacunas, mascota y propietario; alertas resaltadas (advertencia próximas, error vencidas); "Enviar recordatorio" muestra el mensaje y abre el enlace `wa.me` correcto; sin red se avisa y no se marca enviado; marcar como enviado oculta el aviso repetido.
- Estado: pendiente

## Fase 15 — Empaquetado y revisión final
- Objetivo: publicar el ejecutable y cerrar la codificación con una revisión estática. Las pruebas de sistema y de usuario quedan para la fase de pruebas del SDLC (A-17).
- Archivos: instrucciones de puesta en marcha en `README.md`; salida de `dotnet publish`; `trazabilidad.md` (marcar archivos implementados).
- Dependencias: Fases 1 a 14.
- Cubre: RNF-02, RNF-03, RNF-05, RNF-06.
- Criterio de terminado: `dotnet test` completo verde (suite del desarrollador); `dotnet format --verify-no-changes` limpio; `dotnet publish -c Release -r win-x64 --self-contained` genera un `.exe` que arranca, crea la BD en `%LocalAppData%\VeterinariaDrFabioeterinaria.db` y permite `--crear-usuario`; búsqueda en el código sin credenciales literales; revisión de que no existe ninguna pantalla fuera de P-01 a P-12.
- Estado: pendiente

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
