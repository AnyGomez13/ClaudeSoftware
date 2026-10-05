# CLAUDE.md — Sistema de Gestión para Clínica Veterinaria

## 1. Proyecto
- Cliente: clínica veterinaria del Dr. Fabio (Piedecuesta, Santander); veterinarios Fabio y William.
- Producto: aplicación de escritorio Windows para historia clínica, vacunación, carnet PDF y recordatorios por WhatsApp.
- Objetivo: control clínico y fidelización, simple de operar sin secretaria, a costo cero.
- Fuentes de verdad: `requisitosC.md` (qué) y `disenoC.md` (cómo). Planificación: `fases.md`, `trazabilidad.md`.

## 2. Restricciones duras (no negociables)
- R-01 Un solo computador y una sola base de datos interna (archivo SQLite).
- R-02 Internet solo para enviar recordatorios; todo lo demás funciona sin conexión.
- R-03 Un único usuario del sistema; dos veterinarios reales. Todo procedimiento y vacunación guarda el veterinario.
- R-04 Login obligatorio; credenciales validadas contra la BD, nunca escritas en el código ni en texto plano.
- R-05 Un propietario tiene varias mascotas; una mascota exige un propietario existente.
- R-06 Propietario: mínimo nombre completo y celular colombiano (10 dígitos, inicia en 3).
- R-07 Edad calculada desde la fecha de nacimiento (no se almacena); peso siempre en Kg.
- R-08 Carnet de vacunación en PDF descargable.
- R-09 Recordatorios por enlace gratuito `wa.me` (click-to-chat); prohibida la API de pago de WhatsApp.
- R-10 Historia clínica cronológica, trazable y conservada (Ley 576 de 2000, art. 61).
- SUP-08 Sin borrado en duro de información clínica; propietarios y mascotas solo se inactivan.
- Fuera de alcance: inventario, facturación, contabilidad, peluquería, proveedores, multiusuario, varios equipos.
- Todo costo operativo y de licencias debe ser cero.

## 3. Stack cerrado
| Tecnología | Versión exacta | Licencia | Costo |
|---|---|---|---|
| C# / .NET (WPF) | TFM `net8.0` / `net8.0-windows` (SDK instalado 9.0.313, runtime 8.0.26) | MIT | 0 |
| Entity Framework Core + provider SQLite (+ `.Design`) | 8.0.31 | MIT | 0 |
| SQLite (SQLitePCLRaw bundle_e_sqlite3, vía el provider) | 2.1.12 | Dominio público / Apache-2.0 | 0 |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | MIT | 0 |
| QuestPDF | 2026.9.1 | Community (gratuita para la clínica, SUP-D08) | 0 |
| PBKDF2 (`Rfc2898DeriveBytes`, HMAC-SHA256, 100 000 iteraciones) | incluido en .NET 8 | MIT | 0 |
| xUnit (supuesto A-04 de `fases.md`) + Microsoft.NET.Test.Sdk | 2.5.3 + 17.8.0 | Apache-2.0 / MIT | 0 |
- Versiones fijadas en la Fase 1; cambiarlas solo con aprobación del usuario.

## 4. Arquitectura en síntesis
- MVVM en capas, 4 proyectos .NET en una solución: `Dominio`, `Datos`, `Negocio`, `App` (+ `Pruebas`).
- Flujo: Presentación (WPF) → Negocio (servicios) → Datos (EF Core + repositorios) → SQLite.
- Dependencias solo hacia adentro; las capas hablan por interfaces (`I*Service`, `I*Repository`) con DI.
- `Dominio`: entidades POCO y modelos agregados. `Datos`: `VeterinariaDbContext`, repositorios, DDL.
- `Negocio`: reglas, cálculo de edad, enlace WhatsApp, PDF, hash. `App`: Views XAML, ViewModels, navegación.
- EF Core vive solo en `Datos`; los ViewModels nunca ven el DbContext ni reglas de BD.
- Detalle completo (clases, tablas, secuencias): `disenoC.md` §1 a §3.

## 5. Convenciones
- Idioma: código, UI, comentarios y commits en español, como el diseño. Términos propios de .NET en inglés.
- Commits: `Fase N: <resumen en imperativo>`; un commit por entrega coherente.
- Solución y proyectos: `VeterinariaDrFabio.sln`, `VeterinariaDrFabio.{Dominio,Datos,Negocio,App,Pruebas}`.
- Namespaces: igual que la carpeta (`VeterinariaDrFabio.Negocio.Servicios`).
- Carpetas y archivos: exactamente los de `disenoC.md` §1.6; un tipo por archivo, archivo = nombre del tipo.
- Sufijos: `*View`, `*ViewModel`, `*Service`, `*Repository`; interfaces con prefijo `I`.
- Tablas y campos: los del DDL de `disenoC.md` §2.9 (PascalCase, sin tildes).
- Entidad = clase POCO con el mismo nombre que su tabla; propiedad = columna con el mismo nombre.
- Mapeos: `DateTime` ↔ TEXT ISO-8601; `bool Activo` ↔ INTEGER 1/0; `decimal`/`double` Kg ↔ REAL.
- `Edad`, `HistoriaClinica` y `CarnetDigital` no se persisten; son calculados o generados.
- Requisitos en código: comentario XML `/// RF-02, CU-02` en servicios, ViewModels y utilidades.
- Requisitos en pruebas: `[Trait("Req", "RF-02")]` y método `RF02_<Descripcion>`.
- Pruebas en `VeterinariaDrFabio.Pruebas`, espejo de las carpetas del proyecto probado.

## 6. Reglas de trabajo
- Por cada tarea: leer solo la sección relevante de `requisitosC.md` y `disenoC.md`.
- Ejecutar únicamente la fase marcada "en curso" en `fases.md`; no adelantar fases.
- Verificar el criterio de terminado ejecutando su comando o prueba y mostrando el resultado.
- Al cerrar: actualizar el estado en `fases.md` y en la tabla de la sección 9; luego detenerse.
- Pedir aprobación del usuario antes de pasar a la siguiente fase.
- Vacío o ambigüedad: registrar un supuesto (una línea de justificación) en `fases.md`, sección final.
- Contradicción técnica real: preguntar al usuario antes de seguir.
- NO inventar requisitos, pantallas, campos, módulos, tablas ni tecnologías fuera de las fuentes.
- NO agregar dependencias de pago ni servicios externos; NO guardar secretos en el código.
- NO hacer `git push`, force ni reescribir historial sin pedirlo el usuario.
- Reportar con honestidad: si una prueba falla o un paso se omite, decirlo con la salida.

## 7. Reglas de UI
- Implementar solo las pantallas P-01 a P-12 de la sección 4 de `disenoC.md`.
- Prohibido agregar pantallas, campos, botones o funciones que no estén ahí.
- Usar la paleta de `disenoC.md` §4.2 en `Recursos/Colores.xaml` y la tipografía Segoe UI.
- Layouts con `Grid` y `*` o `DockPanel`; ventana mínima 1024×680; sin posiciones fijas.
- Terminología permitida: propietario, mascota, historia clínica, procedimiento, vacunación, carnet, recordatorio.
- No usar los términos cita, peluquería, inventario ni factura.
- Pocos campos obligatorios por pantalla (RNF-01).

## 8. Comandos
- Build: `dotnet build VeterinariaDrFabio.sln`
- Test: `dotnet test VeterinariaDrFabio.sln` (un requisito: `--filter "Req=RF-02"`)
- Lint: `dotnet format VeterinariaDrFabio.sln --verify-no-changes` (corregir con `dotnet format VeterinariaDrFabio.sln`)
- Ejecución: `dotnet run --project VeterinariaDrFabio.App`
- Los comandos se ejecutan desde la raíz del repositorio.

## 9. Índice de fases (detalle en `fases.md`)
| Fase | Nombre | Estado |
|---|---|---|
| 1 | Solución, proyectos y tooling | terminada |
| 2 | Dominio: entidades y modelos | terminada |
| 3 | Datos: DbContext, DDL y migración inicial | terminada |
| 4 | Repositorios | terminada |
| 5 | Utilidades: edad, hash y enlace WhatsApp | pendiente |
| 6 | Autenticación, veterinarios y usuario inicial | pendiente |
| 7 | Servicios de propietario y mascota | pendiente |
| 8 | Servicios de procedimiento, vacunación e historia clínica | pendiente |
| 9 | Carnet PDF, alertas y recordatorios | pendiente |
| 10 | Infraestructura UI, login y shell | pendiente |
| 11 | UI Propietarios y Veterinarios | pendiente |
| 12 | UI Mascotas y ficha clínica | pendiente |
| 13 | UI Procedimiento y vacunación | pendiente |
| 14 | UI Carnet y Alertas | pendiente |
| 15 | Empaquetado y revisión final | pendiente |
