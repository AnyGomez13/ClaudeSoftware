# ClaudeSoftware — Sistema de Gestión para Clínica Veterinaria

Software de escritorio para la gestión clínica de la veterinaria **Villa de San Carlos (Piedecuesta, Santander)**. Reemplaza el cuaderno manual con un registro digital de propietarios, mascotas, historia clínica y vacunación. Incluye un carnet de vacunación en PDF y recordatorios gratuitos por WhatsApp.

> **Estado del proyecto:** fases de **requisitos** y **diseño** terminadas. El repositorio todavía no contiene código fuente; la siguiente fase es el desarrollo, a partir de los documentos de este repositorio.

---

## Contenido del repositorio

| Archivo | Descripción |
|---|---|
| [`requisitosC.md`](requisitosC.md) | Levantamiento de requisitos (IEEE 830): prioridades del cliente, restricciones (R-xx), supuestos (SUP-xx), objetos del dominio, requisitos funcionales (RF) y no funcionales (RNF), reglas de negocio (RN), casos de uso (CU), matriz de trazabilidad y marco normativo. |
| [`disenoC.md`](disenoC.md) | Documento de diseño: arquitectura MVVM en capas, modelo de datos con el DDL completo de SQLite, diagramas UML en PlantUML (casos de uso, clases, secuencia y relacional), diseño de interfaces, matriz de trazabilidad y supuestos de diseño (SUP-D). |

---

## Alcance

### Incluido en la primera versión
- **Inicio de sesión** obligatorio; las credenciales se validan contra la base de datos.
- **Propietarios**: registro, búsqueda y edición (nombre completo y celular colombiano).
- **Mascotas**: registro asociado a un propietario existente. La edad se calcula a partir de la fecha de nacimiento y el peso se registra en kg.
- **Historia clínica cronológica**: procedimientos médicos atribuidos al veterinario que atendió (Fabio o William).
- **Vacunación**: registro de vacunas y desparasitaciones, con la próxima fecha programada.
- **Carnet digital**: generación y descarga en **PDF** para enviárselo al cliente.
- **Alertas** de próximas vacunaciones y desparasitaciones.
- **Recordatorios por WhatsApp** con enlace gratuito `wa.me` (click-to-chat), sin API de pago.

### Fuera del alcance
Inventario, facturación electrónica y contabilidad, citas de peluquería, proveedores y pedidos, uso multiusuario o en varios equipos, y envío 100 % automático por la API de pago de WhatsApp.

---

## Requisitos funcionales

| ID | Requisito |
|---|---|
| RF-01 | Autenticación de usuario |
| RF-02 | Registrar propietario |
| RF-03 | Consultar y buscar propietario |
| RF-04 | Editar propietario |
| RF-05 | Registrar mascota asociada a un propietario existente |
| RF-06 | Consultar mascota e historia clínica |
| RF-07 | Editar mascota |
| RF-08 | Calcular edad de la mascota |
| RF-09 | Registrar procedimiento médico |
| RF-10 | Consultar historia clínica cronológica |
| RF-11 | Registrar vacunación |
| RF-12 | Generar carnet digital |
| RF-13 | Descargar carnet en PDF |
| RF-14 | Generar alertas de vacunación/desparasitación próximas |
| RF-15 | Generar recordatorio por WhatsApp (link gratuito) |
| RF-16 | Gestionar y seleccionar veterinario |

**Requisitos no funcionales:** simplicidad y mínima digitación, un solo computador, una única base de datos interna, operación sin conexión (internet solo para WhatsApp), seguridad y reserva de la información, cero costo operativo, cumplimiento de la Ley 576 de 2000, integridad de los datos y acceso rápido a la historia clínica (RNF-01 a RNF-09).

---

## Stack tecnológico

| Componente | Tecnología |
|---|---|
| Lenguaje / plataforma | C# · .NET 8 (LTS) |
| Interfaz | WPF con patrón **MVVM** |
| Acceso a datos | Entity Framework Core |
| Base de datos | SQLite (archivo local) |
| Inyección de dependencias | Microsoft.Extensions.DependencyInjection |
| Generación de PDF | QuestPDF (licencia Community) |
| Seguridad de credenciales | Hash PBKDF2 |

Todas las dependencias son gratuitas (RNF-06).

---

## Arquitectura

La aplicación usa MVVM organizado en capas, con dependencias que solo apuntan hacia adentro:

```
Presentación (WPF + MVVM) → Lógica de negocio (Servicios) → Acceso a datos (EF Core + Repositorios) → SQLite
```

Estructura de la solución prevista:

```
VeterinariaDrFabio.sln
├── VeterinariaDrFabio.Dominio/   Entidades (Usuario, Veterinario, Propietario, Mascota,
│                                 Procedimiento, Vacunacion, Recordatorio) y modelos agregados
├── VeterinariaDrFabio.Datos/     DbContext, configuraciones, migraciones y repositorios
├── VeterinariaDrFabio.Negocio/   Servicios y utilidades (CalculadoraEdad,
│                                 GeneradorEnlaceWhatsApp, GeneradorCarnetPdf, HasherContrasena)
└── VeterinariaDrFabio.App/       Vistas XAML, ViewModels, navegación y recursos de estilo
```

El detalle completo de clases, tablas, diagramas y pantallas está en [`disenoC.md`](disenoC.md).

---

## Marco normativo

- **Ley 576 de 2000 (art. 61)**, Código de Ética de la Medicina Veterinaria: obliga a abrir y conservar la historia clínica, que es un documento privado, cronológico y sometido a reserva. Por esto el sistema no permite borrar información clínica; los propietarios y las mascotas solo se pueden inactivar.

---

## Próximos pasos

1. Crear la solución .NET con la estructura definida en el diseño.
2. Crear la base de datos con el script DDL de la sección 2.9 de `disenoC.md`.
3. Implementar los casos de uso CU-01 a CU-13.
4. Comprobar el resultado con la matriz de trazabilidad y los criterios de aceptación del diseño.

---

## Contexto académico

Proyecto de grado — Universidad de Investigación y Desarrollo (UDI).
