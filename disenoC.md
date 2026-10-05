Documento de Diseño de Software — Sistema de Gestión para Clínica Veterinaria (Dr. Fabio, Piedecuesta)
Fase: Diseño de software (insumo único para la fase de desarrollo). Fuente única: requisitosC.md. Stack obligatorio (ya decidido, no se modifica): C# · WPF · Entity Framework Core · SQLite (local) · patrón MVVM en capas. Objetivo: especificación completa, exacta y suficiente para que un desarrollador programe sin tomar ni una sola decisión abierta.


Convención de supuestos. Los requisitos ya traen supuestos propios (SUP-01…SUP-10). Para no colisionar con ellos, todo supuesto de diseño que se introduce aquí se numera SUP-D01, SUP-D02… (la "D" es de diseño). Cada decisión no explícita en requisitos queda registrada en la tabla de la sección 7.


________________


Índice
1. Arquitectura del software (MVVM en capas)
2. Modelo de datos completo + DDL SQLite
3. Diagramas UML (PlantUML)
   * 3.1 Casos de uso
   * 3.2 Clases
   * 3.3 Inventario de casos de uso y conteo de secuencias
   * 3.4 Diagramas de secuencia (uno por caso de uso)
   * 3.5 Diagrama relacional de la base de datos
4. Diseño de interfaces
5. Matriz de trazabilidad
6. Supuestos de diseño (SUP-D)
7. Autoverificación de criterios de aceptación


________________


1. Arquitectura del software
1.1 Enfoque general
La aplicación es MVVM (Model-View-ViewModel) organizado en tres capas físicas (tres proyectos .NET dentro de una misma solución), más un proyecto de arranque/UI:


Presentación (WPF + MVVM)  →  Lógica de negocio (Servicios)  →  Acceso a datos (EF Core + Repositorios)  →  SQLite


* Presentación: lo que el usuario ve y toca. Views en XAML, ViewModels con Binding y Commands. No contiene reglas de negocio ni consultas a base de datos.
* Lógica de negocio: Servicios que aplican las reglas de negocio (RN) y orquestan los repositorios. Aquí viven el cálculo de edad, el armado del enlace de WhatsApp, la generación del carnet, el hash de contraseña, el cálculo de alertas, etc.
* Acceso a datos: el DbContext de EF Core y los Repositorios. Traducen objetos ↔ tablas SQLite. Es lo único que "sabe" que la base de datos es SQLite.


Regla de dependencia (hacia adentro): Presentación conoce a Negocio; Negocio conoce a Datos; Datos no conoce a nadie hacia arriba. Las capas superiores dependen de interfaces (IPropietarioService, IMascotaRepository, etc.), no de implementaciones concretas, para poder sustituirlas y testearlas. (Responde a RNF-08, consistencia e integridad, y a mantenibilidad general.)
1.2 Flujo de un caso típico (Vista → ViewModel → Servicio → Repositorio → EF Core → SQLite)
Ejemplo "registrar propietario":


1. La View (PropietarioEdicionView.xaml) está enlazada por Binding a PropietarioEdicionViewModel. El botón "Guardar" dispara un Command (GuardarCommand).
2. El ViewModel valida a nivel de UI (campos no vacíos) y llama IPropietarioService.Registrar(propietario).
3. El Servicio aplica las reglas de negocio (RN-04 nombre+teléfono, RN-14 formato colombiano) y llama IPropietarioRepository.Agregar(propietario).
4. El Repositorio usa el DbContext de EF Core, que genera el INSERT y lo ejecuta contra SQLite.
5. El resultado sube de vuelta: Repositorio → Servicio → ViewModel → la View refleja el estado (éxito/error) por Binding.
1.3 Qué va exactamente en cada capa
Capa
	Tipo de clase
	Responsabilidad
	Presentación
	*View.xaml (+ .xaml.cs mínimo)
	Pantalla declarativa en XAML. Sin lógica salvo wiring del DataContext.
	Presentación
	*ViewModel
	Estado de la pantalla (propiedades con INotifyPropertyChanged), ICommand, llamadas a servicios, validación de entrada de UI.
	Presentación
	RelayCommand
	Implementación de ICommand (patrón command) para enlazar botones a métodos.
	Presentación
	BaseViewModel
	Base con INotifyPropertyChanged + helper SetProperty.
	Presentación
	INavigationService / IDialogService
	Navegación entre vistas y apertura de diálogos sin acoplar ViewModels a WPF.
	Negocio
	*Service (+ su interfaz I*Service)
	Reglas de negocio, orquestación, transformaciones.
	Negocio
	CalculadoraEdad
	Calcula la edad desde la fecha de nacimiento (RF-08, RN-05).
	Negocio
	GeneradorEnlaceWhatsApp
	Arma el mensaje y el enlace wa.me click-to-chat (RF-15, SUP-01).
	Negocio
	GeneradorCarnetPdf
	Produce el PDF del carnet (RF-13).
	Negocio
	HasherContrasena
	PBKDF2 para hashear/verificar credenciales (RNF-05).
	Datos
	VeterinariaDbContext
	DbContext de EF Core: DbSet<> por entidad, mapeos, OnModelCreating.
	Datos
	*Repository (+ interfaz I*Repository)
	CRUD y consultas específicas por entidad vía EF Core.
	Modelo (compartido)
	Usuario, Veterinario, Propietario, Mascota, Procedimiento, Vacunacion, Recordatorio
	Entidades de dominio (POCO) mapeadas a tablas.
	Modelo (compartido)
	CarnetDigital, RegistroClinicoItem, HistoriaClinica
	Modelos generados/agregados (no se persisten como tabla; ver SUP-D04, SUP-D05).
	1.4 Cómo se aplica MVVM sobre WPF y dónde encaja EF Core
* Views (XAML): solo layout y binding. Cada View declara su DataContext hacia su ViewModel (inyectado por DI).
* ViewModels: exponen ObservableCollection<> para listados y propiedades enlazables; exponen ICommand (vía RelayCommand) para acciones. Nunca referencian controles WPF directamente.
* Binding + Commands: {Binding Nombre}, {Binding GuardarCommand}. La UI reacciona sola por INotifyPropertyChanged.
* EF Core: vive solo en la capa de datos, detrás de los repositorios. Los ViewModels jamás ven el DbContext. Esto cumple la separación de capas y mantiene testeable la lógica.
1.5 Inyección de dependencias y arranque
Se usa Microsoft.Extensions.DependencyInjection (incluido en .NET, gratuito — SUP-D09). En App.xaml.cs se registra el contenedor: DbContext, repositorios, servicios, ViewModels y servicios de navegación. La ventana principal se resuelve del contenedor. (Respeta RNF-06, costo cero.)
1.6 Estructura de carpetas / proyectos de la solución (.NET)
Target framework: .NET 8 (LTS) con WPF (SUP-D13).


VeterinariaDrFabio.sln


│


├── VeterinariaDrFabio.Dominio/            (Class Library — Modelo compartido)


│   ├── Entidades/


│   │   ├── Usuario.cs


│   │   ├── Veterinario.cs


│   │   ├── Propietario.cs


│   │   ├── Mascota.cs


│   │   ├── Procedimiento.cs


│   │   ├── Vacunacion.cs


│   │   └── Recordatorio.cs


│   └── Modelos/                           (modelos generados / agregados)


│       ├── CarnetDigital.cs


│       ├── HistoriaClinica.cs


│       └── RegistroClinicoItem.cs


│


├── VeterinariaDrFabio.Datos/              (Class Library — Acceso a datos)


│   ├── Contexto/


│   │   └── VeterinariaDbContext.cs


│   ├── Configuracion/                     (IEntityTypeConfiguration por entidad)


│   ├── Migraciones/                       (EF Core Migrations)


│   └── Repositorios/


│       ├── IUsuarioRepository.cs        / UsuarioRepository.cs


│       ├── IVeterinarioRepository.cs    / VeterinarioRepository.cs


│       ├── IPropietarioRepository.cs    / PropietarioRepository.cs


│       ├── IMascotaRepository.cs        / MascotaRepository.cs


│       ├── IProcedimientoRepository.cs  / ProcedimientoRepository.cs


│       ├── IVacunacionRepository.cs     / VacunacionRepository.cs


│       └── IRecordatorioRepository.cs   / RecordatorioRepository.cs


│


├── VeterinariaDrFabio.Negocio/           (Class Library — Lógica de negocio)


│   ├── Servicios/


│   │   ├── IAutenticacionService.cs      / AutenticacionService.cs


│   │   ├── IPropietarioService.cs        / PropietarioService.cs


│   │   ├── IMascotaService.cs            / MascotaService.cs


│   │   ├── IProcedimientoService.cs      / ProcedimientoService.cs


│   │   ├── IVacunacionService.cs         / VacunacionService.cs


│   │   ├── ICarnetService.cs             / CarnetService.cs


│   │   ├── IAlertaService.cs             / AlertaService.cs


│   │   ├── IRecordatorioService.cs       / RecordatorioService.cs


│   │   └── IVeterinarioService.cs        / VeterinarioService.cs


│   └── Utilidades/


│       ├── CalculadoraEdad.cs


│       ├── GeneradorEnlaceWhatsApp.cs


│       ├── GeneradorCarnetPdf.cs


│       └── HasherContrasena.cs


│


└── VeterinariaDrFabio.App/               (WPF Application — Presentación, arranque)


    ├── App.xaml / App.xaml.cs            (DI, inicio)


    ├── MainWindow.xaml                    (shell + navegación)


    ├── Vistas/


    │   ├── LoginView.xaml


    │   ├── PropietariosView.xaml


    │   ├── PropietarioEdicionView.xaml


    │   ├── MascotasView.xaml


    │   ├── MascotaEdicionView.xaml


    │   ├── MascotaDetalleView.xaml


    │   ├── ProcedimientoEdicionView.xaml


    │   ├── VacunacionEdicionView.xaml


    │   ├── CarnetView.xaml


    │   ├── AlertasView.xaml


    │   └── VeterinariosView.xaml


    ├── ViewModels/


    │   ├── BaseViewModel.cs


    │   ├── LoginViewModel.cs


    │   ├── MainViewModel.cs


    │   ├── PropietariosViewModel.cs


    │   ├── PropietarioEdicionViewModel.cs


    │   ├── MascotasViewModel.cs


    │   ├── MascotaEdicionViewModel.cs


    │   ├── MascotaDetalleViewModel.cs


    │   ├── ProcedimientoEdicionViewModel.cs


    │   ├── VacunacionEdicionViewModel.cs


    │   ├── CarnetViewModel.cs


    │   ├── AlertasViewModel.cs


    │   └── VeterinariosViewModel.cs


    ├── Infraestructura/


    │   ├── RelayCommand.cs


    │   ├── INavigationService.cs / NavigationService.cs


    │   └── IDialogService.cs     / DialogService.cs


    └── Recursos/


        ├── Colores.xaml                   (paleta — sección 4)


        └── Estilos.xaml


Paquetes NuGet (todos gratuitos — RNF-06): Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design, Microsoft.Extensions.DependencyInjection, QuestPDF (licencia Community, gratuita para la clínica — SUP-D08).


________________


2. Modelo de datos completo
Notas de diseño transversales:


* Fechas: SQLite no tiene tipo fecha nativo; se guardan como TEXT en formato ISO-8601 (yyyy-MM-dd para fechas, yyyy-MM-ddTHH:mm:ss para marcas de tiempo). EF Core mapea DateTime a TEXT así por defecto. (SUP-D01)
* Banderas Activo/Inactivo (borrado lógico): INTEGER 1/0, DEFAULT 1. (SUP-D02, deriva de SUP-08/RN-08)
* Edad de la mascota: NO se almacena; es derivada (RF-08, RN-05). No existe columna Edad.
* Peso: REAL, siempre en Kg, > 0 (RN-06).
* Conservación de historia clínica: además de las FK RESTRICT, se instalan triggers BEFORE DELETE … RAISE(ABORT) sobre Propietario, Mascota, Procedimiento y Vacunacion para impedir el borrado en duro (SUP-D10, cumple RN-08/SUP-08/Ley 576).
2.1 Tabla Usuario — cuenta de acceso (RF-01, RNF-05, RN-01) · §4.1
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	NombreUsuario
	TEXT
	NOT NULL, UNIQUE
	Credencial de acceso (usuario)
	ContrasenaHash
	TEXT
	NOT NULL
	Hash PBKDF2 de la contraseña (SUP-D03)
	Salt
	TEXT
	NOT NULL
	Sal aleatoria (base64)
	Activo
	INTEGER
	NOT NULL, DEFAULT 1, CHECK IN (0,1)
	Estado
	2.2 Tabla Veterinario — profesional que atiende (R-03, RF-16, SUP-06) · §4.2
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	NombreCompleto
	TEXT
	NOT NULL
	Nombre que queda en cada procedimiento/vacuna
	RegistroProfesional
	TEXT
	NULL
	Matrícula profesional
	Activo
	INTEGER
	NOT NULL, DEFAULT 1, CHECK IN (0,1)
	Estado
	2.3 Tabla Propietario — dueño de mascotas (RF-02..RF-04, R-06, RN-04, RN-14) · §4.3
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	NombreCompleto
	TEXT
	NOT NULL
	Nombre y apellidos (mínimo R-06)
	Telefono
	TEXT
	NOT NULL, CHECK formato colombiano
	Celular 3######## (RN-14, SUP-02, SUP-D07)
	Documento
	TEXT
	NULL
	Cédula u otro
	Correo
	TEXT
	NULL
	Canal alterno
	Direccion
	TEXT
	NULL
	Dirección
	Activo
	INTEGER
	NOT NULL, DEFAULT 1, CHECK IN (0,1)
	Borrado lógico (SUP-08)
	

CHECK teléfono: Telefono GLOB '3[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]' → exactamente 10 dígitos que inician en 3 (celular colombiano, RN-14). Se almacena sin el prefijo +57; el enlace wa.me antepone 57 (SUP-D07).
2.4 Tabla Mascota — paciente (RF-05..RF-08, R-05, R-07, SUP-03/04) · §4.4
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	PropietarioId
	INTEGER
	NOT NULL, FK → Propietario(Id) ON DELETE RESTRICT
	Dueño; obligatorio (R-05, RN-03)
	Nombre
	TEXT
	NOT NULL
	Nombre de la mascota
	Especie
	TEXT
	NOT NULL
	Perro, gato, etc. (SUP-03)
	Raza
	TEXT
	NULL
	Raza
	Sexo
	TEXT
	NULL, CHECK IN ('Macho','Hembra') o NULL
	Sexo (SUP)
	FechaNacimiento
	TEXT
	NOT NULL
	ISO-8601; real o estimada (R-07, SUP-04)
	FechaNacimientoEstimada
	INTEGER
	NOT NULL, DEFAULT 0, CHECK IN (0,1)
	1 = fecha estimada (SUP-04)
	Peso
	REAL
	NOT NULL, CHECK Peso > 0
	Peso actual en Kg (R-07, RN-06)
	ColorSenas
	TEXT
	NULL
	Características físicas
	Activo
	INTEGER
	NOT NULL, DEFAULT 1, CHECK IN (0,1)
	Borrado lógico (SUP-08)
	

Relación: Propietario 1 —— N Mascota (un propietario varias mascotas; una mascota un único propietario · RN-02). FK obligatoria + ON DELETE RESTRICT ⇒ una mascota no puede existir sin propietario (RNF-08).
2.5 Tabla Procedimiento — registro clínico (RF-09, RF-10, R-03, RN-07) · §4.5
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	MascotaId
	INTEGER
	NOT NULL, FK → Mascota(Id) ON DELETE RESTRICT
	Mascota atendida
	VeterinarioId
	INTEGER
	NOT NULL, FK → Veterinario(Id) ON DELETE RESTRICT
	Profesional que evaluó (R-03, RN-07)
	Fecha
	TEXT
	NOT NULL
	Fecha del procedimiento (orden cronológico, R-10)
	TipoProcedimiento
	TEXT
	NOT NULL
	Consulta, cirugía, control, desparasitación…
	Descripcion
	TEXT
	NOT NULL
	Observación / diagnóstico (núcleo de la historia)
	Tratamiento
	TEXT
	NULL
	Indicaciones, medicamentos, notas
	PesoEnElMomento
	REAL
	NULL, CHECK (NULL o > 0)
	Peso al momento (seguimiento, SUP-05)
	ProximaFechaRecomendada
	TEXT
	NULL
	Próximo control/ciclo (alimenta alertas, RF-14)
	2.6 Tabla Vacunacion — aplicación de vacuna (RF-11, RF-12) · §4.6
Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	MascotaId
	INTEGER
	NOT NULL, FK → Mascota(Id) ON DELETE RESTRICT
	Mascota vacunada
	VeterinarioId
	INTEGER
	NOT NULL, FK → Veterinario(Id) ON DELETE RESTRICT
	Profesional que aplicó (R-03, RN-07)
	NombreVacuna
	TEXT
	NOT NULL
	Vacuna aplicada (texto/selección simple, SUP-10)
	FechaAplicacion
	TEXT
	NOT NULL
	Fecha de aplicación
	ProximaFecha
	TEXT
	NULL
	Refuerzo (alimenta alertas, RF-14/RF-15)
	Lote
	TEXT
	NULL
	Lote / laboratorio
	Observaciones
	TEXT
	NULL
	Notas adicionales
	2.7 Tabla Recordatorio — aviso preparado (RF-14, RF-15, SUP-01, SUP-09) · §4.9
Se persiste para controlar el estado Pendiente/Enviado y no repetir avisos. Referencia su origen con dos FK nulables y una regla de exactamente-una (SUP-D06).


Columna
	Tipo SQLite
	Restricciones
	Descripción
	Id
	INTEGER
	PK, AUTOINCREMENT
	Identificador interno
	MascotaId
	INTEGER
	NOT NULL, FK → Mascota(Id) ON DELETE RESTRICT
	A quién se le recuerda
	Tipo
	TEXT
	NOT NULL, CHECK IN ('Vacunacion','Desparasitacion')
	Tipo de ciclo (SUP-09)
	FechaObjetivo
	TEXT
	NOT NULL
	Próxima fecha que origina el aviso
	Mensaje
	TEXT
	NOT NULL
	Texto armado por el sistema
	Enlace
	TEXT
	NOT NULL
	Link wa.me click-to-chat (SUP-01)
	Estado
	TEXT
	NOT NULL, DEFAULT 'Pendiente', CHECK IN ('Pendiente','Enviado')
	Para no repetir avisos
	FechaEnvio
	TEXT
	NULL
	Marca de tiempo de envío
	VacunacionId
	INTEGER
	NULL, FK → Vacunacion(Id) ON DELETE RESTRICT
	Origen si es vacunación
	ProcedimientoId
	INTEGER
	NULL, FK → Procedimiento(Id) ON DELETE RESTRICT
	Origen si es desparasitación
	—
	—
	CHECK (VacunacionId IS NOT NULL) + (ProcedimientoId IS NOT NULL) = 1
	Exactamente un origen
	2.8 Objetos sin tabla física (generados / agregados)
* HistoriaClinica (§4.8): no es tabla. Es la agregación cronológica de Procedimiento + Vacunacion de una mascota, resuelta por consulta ordenada por fecha. Modelo en memoria HistoriaClinica { Mascota, FechaApertura, List<RegistroClinicoItem> }. (SUP-D04, RF-10)
* CarnetDigital (§4.7): no es tabla. Artefacto generado on-demand a partir de Vacunacion + datos de Mascota/Propietario, exportado a PDF. (SUP-D05, RF-12/RF-13)
2.9 Script DDL completo (SQLite) — corre tal cual
-- =========================================================================


-- Clínica Veterinaria Dr. Fabio — Esquema SQLite


-- Ejecutar completo. Requiere SQLite con soporte de llaves foráneas.


-- =========================================================================


PRAGMA foreign_keys = ON;


-- ----------------------------------------------------------------- USUARIO


CREATE TABLE Usuario (


    Id              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    NombreUsuario   TEXT    NOT NULL UNIQUE,


    ContrasenaHash  TEXT    NOT NULL,


    Salt            TEXT    NOT NULL,


    Activo          INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1))


);


-- ------------------------------------------------------------- VETERINARIO


CREATE TABLE Veterinario (


    Id                   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    NombreCompleto       TEXT    NOT NULL,


    RegistroProfesional  TEXT    NULL,


    Activo               INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1))


);


-- ------------------------------------------------------------- PROPIETARIO


CREATE TABLE Propietario (


    Id              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    NombreCompleto  TEXT    NOT NULL,


    Telefono        TEXT    NOT NULL


        CHECK (Telefono GLOB '3[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),


    Documento       TEXT    NULL,


    Correo          TEXT    NULL,


    Direccion       TEXT    NULL,


    Activo          INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1))


);


-- ----------------------------------------------------------------- MASCOTA


CREATE TABLE Mascota (


    Id                       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    PropietarioId            INTEGER NOT NULL,


    Nombre                   TEXT    NOT NULL,


    Especie                  TEXT    NOT NULL,


    Raza                     TEXT    NULL,


    Sexo                     TEXT    NULL CHECK (Sexo IS NULL OR Sexo IN ('Macho', 'Hembra')),


    FechaNacimiento          TEXT    NOT NULL,


    FechaNacimientoEstimada  INTEGER NOT NULL DEFAULT 0 CHECK (FechaNacimientoEstimada IN (0, 1)),


    Peso                     REAL    NOT NULL CHECK (Peso > 0),


    ColorSenas               TEXT    NULL,


    Activo                   INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),


    CONSTRAINT FK_Mascota_Propietario


        FOREIGN KEY (PropietarioId) REFERENCES Propietario (Id)


        ON DELETE RESTRICT ON UPDATE CASCADE


);


-- ----------------------------------------------------------- PROCEDIMIENTO


CREATE TABLE Procedimiento (


    Id                        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    MascotaId                 INTEGER NOT NULL,


    VeterinarioId             INTEGER NOT NULL,


    Fecha                     TEXT    NOT NULL,


    TipoProcedimiento         TEXT    NOT NULL,


    Descripcion               TEXT    NOT NULL,


    Tratamiento               TEXT    NULL,


    PesoEnElMomento           REAL    NULL CHECK (PesoEnElMomento IS NULL OR PesoEnElMomento > 0),


    ProximaFechaRecomendada   TEXT    NULL,


    CONSTRAINT FK_Procedimiento_Mascota


        FOREIGN KEY (MascotaId) REFERENCES Mascota (Id) ON DELETE RESTRICT,


    CONSTRAINT FK_Procedimiento_Veterinario


        FOREIGN KEY (VeterinarioId) REFERENCES Veterinario (Id) ON DELETE RESTRICT


);


-- -------------------------------------------------------------- VACUNACION


CREATE TABLE Vacunacion (


    Id               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    MascotaId        INTEGER NOT NULL,


    VeterinarioId    INTEGER NOT NULL,


    NombreVacuna     TEXT    NOT NULL,


    FechaAplicacion  TEXT    NOT NULL,


    ProximaFecha     TEXT    NULL,


    Lote             TEXT    NULL,


    Observaciones    TEXT    NULL,


    CONSTRAINT FK_Vacunacion_Mascota


        FOREIGN KEY (MascotaId) REFERENCES Mascota (Id) ON DELETE RESTRICT,


    CONSTRAINT FK_Vacunacion_Veterinario


        FOREIGN KEY (VeterinarioId) REFERENCES Veterinario (Id) ON DELETE RESTRICT


);


-- ------------------------------------------------------------- RECORDATORIO


CREATE TABLE Recordatorio (


    Id               INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,


    MascotaId        INTEGER NOT NULL,


    Tipo             TEXT    NOT NULL CHECK (Tipo IN ('Vacunacion', 'Desparasitacion')),


    FechaObjetivo    TEXT    NOT NULL,


    Mensaje          TEXT    NOT NULL,


    Enlace           TEXT    NOT NULL,


    Estado           TEXT    NOT NULL DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'Enviado')),


    FechaEnvio       TEXT    NULL,


    VacunacionId     INTEGER NULL,


    ProcedimientoId  INTEGER NULL,


    CONSTRAINT FK_Recordatorio_Mascota


        FOREIGN KEY (MascotaId) REFERENCES Mascota (Id) ON DELETE RESTRICT,


    CONSTRAINT FK_Recordatorio_Vacunacion


        FOREIGN KEY (VacunacionId) REFERENCES Vacunacion (Id) ON DELETE RESTRICT,


    CONSTRAINT FK_Recordatorio_Procedimiento


        FOREIGN KEY (ProcedimientoId) REFERENCES Procedimiento (Id) ON DELETE RESTRICT,


    CONSTRAINT CK_Recordatorio_UnOrigen


        CHECK ((VacunacionId IS NOT NULL) + (ProcedimientoId IS NOT NULL) = 1)


);


-- ------------------------------------------------------------------ ÍNDICES


CREATE INDEX IX_Mascota_PropietarioId       ON Mascota (PropietarioId);


CREATE INDEX IX_Mascota_Activo              ON Mascota (Activo);


CREATE INDEX IX_Propietario_NombreCompleto  ON Propietario (NombreCompleto);


CREATE INDEX IX_Propietario_Telefono        ON Propietario (Telefono);


CREATE INDEX IX_Procedimiento_MascotaFecha  ON Procedimiento (MascotaId, Fecha);


CREATE INDEX IX_Procedimiento_Veterinario   ON Procedimiento (VeterinarioId);


CREATE INDEX IX_Procedimiento_ProximaFecha  ON Procedimiento (ProximaFechaRecomendada);


CREATE INDEX IX_Vacunacion_MascotaFecha     ON Vacunacion (MascotaId, FechaAplicacion);


CREATE INDEX IX_Vacunacion_Veterinario      ON Vacunacion (VeterinarioId);


CREATE INDEX IX_Vacunacion_ProximaFecha     ON Vacunacion (ProximaFecha);


CREATE INDEX IX_Recordatorio_Estado         ON Recordatorio (Estado);


CREATE INDEX IX_Recordatorio_MascotaId      ON Recordatorio (MascotaId);


-- ----------------- TRIGGERS: impedir borrado en duro (Ley 576 / RN-08 / SUP-08)


CREATE TRIGGER TR_Propietario_NoBorrar


BEFORE DELETE ON Propietario


BEGIN


    SELECT RAISE(ABORT, 'Los propietarios no se eliminan; use inactivacion (SUP-08).');


END;


CREATE TRIGGER TR_Mascota_NoBorrar


BEFORE DELETE ON Mascota


BEGIN


    SELECT RAISE(ABORT, 'Las mascotas no se eliminan; use inactivacion (SUP-08).');


END;


CREATE TRIGGER TR_Procedimiento_NoBorrar


BEFORE DELETE ON Procedimiento


BEGIN


    SELECT RAISE(ABORT, 'La historia clinica no se elimina (Ley 576, art. 61 / RN-08).');


END;


CREATE TRIGGER TR_Vacunacion_NoBorrar


BEFORE DELETE ON Vacunacion


BEGIN


    SELECT RAISE(ABORT, 'La historia clinica no se elimina (Ley 576, art. 61 / RN-08).');


END;


-- --------------------------------------- SEED: veterinarios pre-registrados (SUP-06)


INSERT INTO Veterinario (NombreCompleto, RegistroProfesional, Activo)


VALUES ('Fabio', NULL, 1),


       ('William', NULL, 1);


-- Nota: el Usuario inicial (credenciales) se crea en la instalacion/puesta en marcha


-- con hash PBKDF2 (SUP-07, SUP-D03); por eso no se siembra aqui en texto plano.


________________


3. Diagramas UML (PlantUML)
3.1 Diagrama de casos de uso
@startuml CasosDeUso


left to right direction


skinparam packageStyle rectangle


actor "Veterinario\n(Fabio / William)" as Vet


actor "Sistema" as Sys


rectangle "Sistema de Gestión Clínica Veterinaria" {


  usecase "CU-01 Iniciar sesión" as CU01


  usecase "CU-02 Registrar propietario" as CU02


  usecase "CU-03 Consultar/buscar propietario" as CU03


  usecase "CU-04 Editar propietario" as CU04


  usecase "CU-05 Registrar mascota" as CU05


  usecase "CU-06 Consultar mascota e historia clínica" as CU06


  usecase "CU-07 Editar mascota" as CU07


  usecase "CU-08 Registrar procedimiento médico" as CU08


  usecase "CU-09 Registrar vacunación" as CU09


  usecase "CU-10 Generar y descargar carnet (PDF)" as CU10


  usecase "CU-11 Consultar alertas de próximas fechas" as CU11


  usecase "CU-12 Enviar recordatorio por WhatsApp" as CU12


  usecase "CU-13 Gestionar veterinarios" as CU13


}


Vet --> CU01


Vet --> CU02


Vet --> CU03


Vet --> CU04


Vet --> CU05


Vet --> CU06


Vet --> CU07


Vet --> CU08


Vet --> CU09


Vet --> CU10


Vet --> CU11


Vet --> CU12


Vet --> CU13


Sys --> CU11


Sys --> CU12


CU08 ..> CU06 : <<include>>


CU09 ..> CU06 : <<include>>


CU10 ..> CU06 : <<include>>


CU12 ..> CU11 : <<include>>


CU05 ..> CU02 : <<extend>>\n(si no existe propietario)


@enduml
3.2 Diagrama de clases
@startuml Clases


skinparam classAttributeIconSize 0


hide empty members


' ===================== MODELO / ENTIDADES =====================


package "Dominio.Entidades" {


  class Usuario {


    +int Id


    +string NombreUsuario


    +string ContrasenaHash


    +string Salt


    +bool Activo


  }


  class Veterinario {


    +int Id


    +string NombreCompleto


    +string RegistroProfesional


    +bool Activo


  }


  class Propietario {


    +int Id


    +string NombreCompleto


    +string Telefono


    +string Documento


    +string Correo


    +string Direccion


    +bool Activo


  }


  class Mascota {


    +int Id


    +int PropietarioId


    +string Nombre


    +string Especie


    +string Raza


    +string Sexo


    +DateTime FechaNacimiento


    +bool FechaNacimientoEstimada


    +double Peso


    +string ColorSenas


    +bool Activo


  }


  class Procedimiento {


    +int Id


    +int MascotaId


    +int VeterinarioId


    +DateTime Fecha


    +string TipoProcedimiento


    +string Descripcion


    +string Tratamiento


    +double? PesoEnElMomento


    +DateTime? ProximaFechaRecomendada


  }


  class Vacunacion {


    +int Id


    +int MascotaId


    +int VeterinarioId


    +string NombreVacuna


    +DateTime FechaAplicacion


    +DateTime? ProximaFecha


    +string Lote


    +string Observaciones


  }


  class Recordatorio {


    +int Id


    +int MascotaId


    +string Tipo


    +DateTime FechaObjetivo


    +string Mensaje


    +string Enlace


    +string Estado


    +DateTime? FechaEnvio


    +int? VacunacionId


    +int? ProcedimientoId


  }


}


package "Dominio.Modelos" {


  class HistoriaClinica {


    +Mascota Mascota


    +DateTime FechaApertura


    +List<RegistroClinicoItem> Registros


  }


  class RegistroClinicoItem {


    +DateTime Fecha


    +string Origen


    +string Detalle


    +string Veterinario


  }


  class CarnetDigital {


    +Mascota Mascota


    +Propietario Propietario


    +List<Vacunacion> Vacunas


    +DateTime FechaGeneracion


  }


}


Propietario "1" --> "0..*" Mascota


Mascota "1" --> "0..*" Procedimiento


Mascota "1" --> "0..*" Vacunacion


Veterinario "1" --> "0..*" Procedimiento


Veterinario "1" --> "0..*" Vacunacion


Mascota "1" --> "0..*" Recordatorio


Vacunacion "0..1" --> "0..*" Recordatorio


Procedimiento "0..1" --> "0..*" Recordatorio


' ===================== ACCESO A DATOS =====================


package "Datos" {


  class VeterinariaDbContext {


    +DbSet<Usuario> Usuarios


    +DbSet<Veterinario> Veterinarios


    +DbSet<Propietario> Propietarios


    +DbSet<Mascota> Mascotas


    +DbSet<Procedimiento> Procedimientos


    +DbSet<Vacunacion> Vacunaciones


    +DbSet<Recordatorio> Recordatorios


    +OnModelCreating(ModelBuilder)


  }


  interface IUsuarioRepository {


    +Usuario ObtenerPorNombre(string)


  }


  interface IVeterinarioRepository {


    +List<Veterinario> ListarActivos()


    +void Agregar(Veterinario)


  }


  interface IPropietarioRepository {


    +void Agregar(Propietario)


    +void Actualizar(Propietario)


    +List<Propietario> Buscar(string)


    +Propietario ObtenerPorId(int)


  }


  interface IMascotaRepository {


    +void Agregar(Mascota)


    +void Actualizar(Mascota)


    +List<Mascota> Buscar(string)


    +Mascota ObtenerPorId(int)


  }


  interface IProcedimientoRepository {


    +void Agregar(Procedimiento)


    +List<Procedimiento> ListarPorMascota(int)


    +List<Procedimiento> ListarConProximaFecha()


  }


  interface IVacunacionRepository {


    +void Agregar(Vacunacion)


    +List<Vacunacion> ListarPorMascota(int)


    +List<Vacunacion> ListarConProximaFecha()


  }


  interface IRecordatorioRepository {


    +void Agregar(Recordatorio)


    +void Actualizar(Recordatorio)


    +List<Recordatorio> ListarPendientes()


  }


  class UsuarioRepository


  class VeterinarioRepository


  class PropietarioRepository


  class MascotaRepository


  class ProcedimientoRepository


  class VacunacionRepository


  class RecordatorioRepository


}


IUsuarioRepository <|.. UsuarioRepository


IVeterinarioRepository <|.. VeterinarioRepository


IPropietarioRepository <|.. PropietarioRepository


IMascotaRepository <|.. MascotaRepository


IProcedimientoRepository <|.. ProcedimientoRepository


IVacunacionRepository <|.. VacunacionRepository


IRecordatorioRepository <|.. RecordatorioRepository


UsuarioRepository --> VeterinariaDbContext


VeterinarioRepository --> VeterinariaDbContext


PropietarioRepository --> VeterinariaDbContext


MascotaRepository --> VeterinariaDbContext


ProcedimientoRepository --> VeterinariaDbContext


VacunacionRepository --> VeterinariaDbContext


RecordatorioRepository --> VeterinariaDbContext


' ===================== NEGOCIO / SERVICIOS =====================


package "Negocio.Servicios" {


  interface IAutenticacionService {


    +bool IniciarSesion(string, string)


    +void CerrarSesion()


  }


  interface IPropietarioService {


    +Resultado Registrar(Propietario)


    +Resultado Editar(Propietario)


    +List<Propietario> Buscar(string)


  }


  interface IMascotaService {


    +Resultado Registrar(Mascota)


    +Resultado Editar(Mascota)


    +List<Mascota> Buscar(string)


    +int CalcularEdadMeses(Mascota)


  }


  interface IProcedimientoService {


    +Resultado Registrar(Procedimiento)


  }


  interface IVacunacionService {


    +Resultado Registrar(Vacunacion)


  }


  interface ICarnetService {


    +CarnetDigital Generar(int mascotaId)


    +string ExportarPdf(CarnetDigital)


  }


  interface IAlertaService {


    +List<RegistroClinicoItem> ProximasFechas()


  }


  interface IRecordatorioService {


    +Recordatorio Preparar(RegistroClinicoItem)


    +void MarcarEnviado(Recordatorio)


  }


  interface IVeterinarioService {


    +List<Veterinario> ListarActivos()


    +Resultado Registrar(Veterinario)


  }


  class AutenticacionService


  class PropietarioService


  class MascotaService


  class ProcedimientoService


  class VacunacionService


  class CarnetService


  class AlertaService


  class RecordatorioService


  class VeterinarioService


}


package "Negocio.Utilidades" {


  class CalculadoraEdad {


    +int EnMeses(DateTime nacimiento)


    +string Legible(DateTime nacimiento)


  }


  class GeneradorEnlaceWhatsApp {


    +string Construir(string telefono, string mensaje)


  }


  class GeneradorCarnetPdf {


    +string Generar(CarnetDigital, string ruta)


  }


  class HasherContrasena {


    +string Hashear(string clave, out string salt)


    +bool Verificar(string clave, string hash, string salt)


  }


}


IAutenticacionService <|.. AutenticacionService


IPropietarioService <|.. PropietarioService


IMascotaService <|.. MascotaService


IProcedimientoService <|.. ProcedimientoService


IVacunacionService <|.. VacunacionService


ICarnetService <|.. CarnetService


IAlertaService <|.. AlertaService


IRecordatorioService <|.. RecordatorioService


IVeterinarioService <|.. VeterinarioService


AutenticacionService --> IUsuarioRepository


AutenticacionService --> HasherContrasena


PropietarioService --> IPropietarioRepository


MascotaService --> IMascotaRepository


MascotaService --> CalculadoraEdad


ProcedimientoService --> IProcedimientoRepository


VacunacionService --> IVacunacionRepository


CarnetService --> IVacunacionRepository


CarnetService --> GeneradorCarnetPdf


AlertaService --> IProcedimientoRepository


AlertaService --> IVacunacionRepository


RecordatorioService --> IRecordatorioRepository


RecordatorioService --> GeneradorEnlaceWhatsApp


VeterinarioService --> IVeterinarioRepository


' ===================== PRESENTACIÓN / VIEWMODELS =====================


package "App.ViewModels" {


  abstract class BaseViewModel {


    +event PropertyChanged


    #SetProperty()


  }


  class RelayCommand {


    +Execute(object)


    +CanExecute(object)


  }


  class LoginViewModel


  class MainViewModel


  class PropietariosViewModel


  class PropietarioEdicionViewModel


  class MascotasViewModel


  class MascotaEdicionViewModel


  class MascotaDetalleViewModel


  class ProcedimientoEdicionViewModel


  class VacunacionEdicionViewModel


  class CarnetViewModel


  class AlertasViewModel


  class VeterinariosViewModel


}


BaseViewModel <|-- LoginViewModel


BaseViewModel <|-- MainViewModel


BaseViewModel <|-- PropietariosViewModel


BaseViewModel <|-- PropietarioEdicionViewModel


BaseViewModel <|-- MascotasViewModel


BaseViewModel <|-- MascotaEdicionViewModel


BaseViewModel <|-- MascotaDetalleViewModel


BaseViewModel <|-- ProcedimientoEdicionViewModel


BaseViewModel <|-- VacunacionEdicionViewModel


BaseViewModel <|-- CarnetViewModel


BaseViewModel <|-- AlertasViewModel


BaseViewModel <|-- VeterinariosViewModel


LoginViewModel --> IAutenticacionService


PropietariosViewModel --> IPropietarioService


PropietarioEdicionViewModel --> IPropietarioService


MascotasViewModel --> IMascotaService


MascotaEdicionViewModel --> IMascotaService


MascotaEdicionViewModel --> IPropietarioService


MascotaDetalleViewModel --> IMascotaService


MascotaDetalleViewModel --> IProcedimientoService


MascotaDetalleViewModel --> IVacunacionService


MascotaDetalleViewModel --> ICarnetService


ProcedimientoEdicionViewModel --> IProcedimientoService


ProcedimientoEdicionViewModel --> IVeterinarioService


VacunacionEdicionViewModel --> IVacunacionService


VacunacionEdicionViewModel --> IVeterinarioService


CarnetViewModel --> ICarnetService


AlertasViewModel --> IAlertaService


AlertasViewModel --> IRecordatorioService


VeterinariosViewModel --> IVeterinarioService


@enduml
3.3 Inventario de casos de uso y conteo de diagramas de secuencia
Casos de uso identificados en 3.1: CU-01, CU-02, CU-03, CU-04, CU-05, CU-06, CU-07, CU-08, CU-09, CU-10, CU-11, CU-12, CU-13 = 13 casos de uso.


Diagramas de secuencia en 3.4: 13 (uno por caso de uso).


✅ Conteo: 13 casos de uso = 13 diagramas de secuencia. No falta ninguno.
3.4 Diagramas de secuencia (uno por caso de uso)
CU-01 — Iniciar sesión
@startuml SEC_CU01_IniciarSesion


title CU-01 Iniciar sesión


actor Veterinario as V


participant "LoginView" as View


participant "LoginViewModel" as VM


participant "AutenticacionService" as Svc


participant "HasherContrasena" as Hash


participant "UsuarioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : ingresa usuario y contraseña


View -> VM : IniciarSesionCommand


VM -> Svc : IniciarSesion(usuario, clave)


Svc -> Repo : ObtenerPorNombre(usuario)


Repo -> DB : SELECT * FROM Usuario WHERE NombreUsuario = ?


DB --> Repo : Usuario (hash + salt) / null


Repo --> Svc : Usuario / null


alt usuario existe y activo


  Svc -> Hash : Verificar(clave, hash, salt)


  Hash --> Svc : true / false


  alt credenciales válidas


    Svc --> VM : éxito


    VM -> View : navega a MainWindow


  else credenciales inválidas


    Svc --> VM : fallo


    VM -> View : muestra "Credenciales inválidas"


  end


else usuario no existe


  Svc --> VM : fallo


  VM -> View : muestra "Credenciales inválidas"


end


@enduml
CU-02 — Registrar propietario
@startuml SEC_CU02_RegistrarPropietario


title CU-02 Registrar propietario


actor Veterinario as V


participant "PropietarioEdicionView" as View


participant "PropietarioEdicionViewModel" as VM


participant "PropietarioService" as Svc


participant "PropietarioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : selecciona "nuevo propietario" e ingresa nombre y teléfono


View -> VM : GuardarCommand


VM -> Svc : Registrar(propietario)


Svc -> Svc : valida nombre no vacío (RN-04)


Svc -> Svc : valida teléfono formato colombiano (RN-14)


alt datos válidos


  Svc -> Repo : Agregar(propietario)


  Repo -> DB : INSERT INTO Propietario(...)


  DB --> Repo : Id generado


  Repo --> Svc : ok


  Svc --> VM : éxito


  VM -> View : confirma y cierra


else datos inválidos


  Svc --> VM : error de validación


  VM -> View : marca campo inválido, no guarda


end


@enduml
CU-03 — Consultar/buscar propietario
@startuml SEC_CU03_BuscarPropietario


title CU-03 Consultar/buscar propietario


actor Veterinario as V


participant "PropietariosView" as View


participant "PropietariosViewModel" as VM


participant "PropietarioService" as Svc


participant "PropietarioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : escribe criterio (nombre/teléfono)


View -> VM : BuscarCommand(criterio)


VM -> Svc : Buscar(criterio)


Svc -> Repo : Buscar(criterio)


Repo -> DB : SELECT * FROM Propietario WHERE NombreCompleto LIKE ? OR Telefono LIKE ?


DB --> Repo : lista de propietarios


Repo --> Svc : lista


Svc --> VM : lista


alt hay resultados


  VM -> View : muestra coincidencias y, al seleccionar, sus mascotas


else sin resultados


  VM -> View : muestra "Sin resultados"


end


@enduml
CU-04 — Editar propietario
@startuml SEC_CU04_EditarPropietario


title CU-04 Editar propietario


actor Veterinario as V


participant "PropietarioEdicionView" as View


participant "PropietarioEdicionViewModel" as VM


participant "PropietarioService" as Svc


participant "PropietarioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : abre propietario existente (desde CU-03) y modifica datos


View -> VM : GuardarCommand


VM -> Svc : Editar(propietario)


Svc -> Svc : valida nombre y teléfono (RN-04, RN-14)


alt datos válidos


  Svc -> Repo : Actualizar(propietario)


  Repo -> DB : UPDATE Propietario SET ... WHERE Id = ?


  DB --> Repo : filas afectadas


  Repo --> Svc : ok


  Svc --> VM : éxito


  VM -> View : confirma y cierra


else datos inválidos


  Svc --> VM : error de validación


  VM -> View : no guarda, señala el error


end


@enduml
CU-05 — Registrar mascota
@startuml SEC_CU05_RegistrarMascota


title CU-05 Registrar mascota


actor Veterinario as V


participant "MascotaEdicionView" as View


participant "MascotaEdicionViewModel" as VM


participant "PropietarioService" as PropSvc


participant "MascotaService" as Svc


participant "CalculadoraEdad" as Edad


participant "MascotaRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : selecciona "nueva mascota"


View -> VM : CargarPropietariosCommand


VM -> PropSvc : Buscar("")


PropSvc --> VM : lista de propietarios existentes


V -> View : elige propietario, ingresa nombre, especie, fecha nac., peso (Kg)


View -> VM : GuardarCommand


VM -> Svc : Registrar(mascota)


Svc -> Svc : valida propietario seleccionado (RN-03) y peso > 0 (RN-06)


Svc -> Edad : EnMeses(FechaNacimiento)


Edad --> Svc : edad (solo para mostrar; no se persiste · RF-08)


alt datos válidos


  Svc -> Repo : Agregar(mascota)


  Repo -> DB : INSERT INTO Mascota(...)


  DB --> Repo : Id generado


  Repo --> Svc : ok


  Svc --> VM : éxito


  VM -> View : confirma


else falta propietario u otro dato


  Svc --> VM : error


  VM -> View : no guarda, señala el error


end


@enduml
CU-06 — Consultar mascota e historia clínica
@startuml SEC_CU06_ConsultarMascota


title CU-06 Consultar mascota e historia clínica


actor Veterinario as V


participant "MascotaDetalleView" as View


participant "MascotaDetalleViewModel" as VM


participant "MascotaService" as MSvc


participant "CalculadoraEdad" as Edad


participant "ProcedimientoRepository" as PRepo


participant "VacunacionRepository" as VRepo


database "SQLite (EF Core)" as DB


V -> View : busca mascota (nombre/propietario)


View -> VM : CargarCommand(mascotaId)


VM -> MSvc : ObtenerPorId(mascotaId)


MSvc --> VM : mascota


VM -> Edad : Legible(mascota.FechaNacimiento)


Edad --> VM : edad calculada


VM -> PRepo : ListarPorMascota(mascotaId)


PRepo -> DB : SELECT * FROM Procedimiento WHERE MascotaId = ? ORDER BY Fecha


DB --> PRepo : procedimientos


VM -> VRepo : ListarPorMascota(mascotaId)


VRepo -> DB : SELECT * FROM Vacunacion WHERE MascotaId = ? ORDER BY FechaAplicacion


DB --> VRepo : vacunaciones


VM -> VM : fusiona y ordena cronológicamente (HistoriaClinica)


VM -> View : muestra ficha + edad + historia clínica cronológica\n(con veterinario por registro)


@enduml
CU-07 — Editar mascota
@startuml SEC_CU07_EditarMascota


title CU-07 Editar mascota


actor Veterinario as V


participant "MascotaEdicionView" as View


participant "MascotaEdicionViewModel" as VM


participant "MascotaService" as Svc


participant "MascotaRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : abre mascota (desde CU-06) y actualiza datos (ej. peso)


View -> VM : GuardarCommand


VM -> Svc : Editar(mascota)


Svc -> Svc : valida peso > 0 en Kg (RN-06) y propietario presente (RN-03)


alt datos válidos


  Svc -> Repo : Actualizar(mascota)


  Repo -> DB : UPDATE Mascota SET ... WHERE Id = ?


  DB --> Repo : filas afectadas


  Repo --> Svc : ok


  Svc --> VM : éxito


  VM -> View : confirma


else datos inválidos


  Svc --> VM : error


  VM -> View : no guarda


end


@enduml
CU-08 — Registrar procedimiento médico
@startuml SEC_CU08_RegistrarProcedimiento


title CU-08 Registrar procedimiento médico


actor Veterinario as V


participant "ProcedimientoEdicionView" as View


participant "ProcedimientoEdicionViewModel" as VM


participant "VeterinarioService" as VetSvc


participant "ProcedimientoService" as Svc


participant "ProcedimientoRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : desde la ficha de la mascota, "nuevo procedimiento"


View -> VM : CargarVeterinariosCommand


VM -> VetSvc : ListarActivos()


VetSvc --> VM : [Fabio, William]


V -> View : ingresa fecha, tipo, descripción, próxima fecha; elige veterinario


View -> VM : GuardarCommand


VM -> Svc : Registrar(procedimiento)


Svc -> Svc : valida veterinario seleccionado (RN-07) y campos obligatorios


alt veterinario presente y datos válidos


  Svc -> Repo : Agregar(procedimiento)


  Repo -> DB : INSERT INTO Procedimiento(...)


  DB --> Repo : Id generado


  Repo --> Svc : ok


  Svc --> VM : éxito


  VM -> View : confirma; el registro entra a la historia clínica


else falta veterinario


  Svc --> VM : error (RN-07)


  VM -> View : no guarda


end


@enduml
CU-09 — Registrar vacunación
@startuml SEC_CU09_RegistrarVacunacion


title CU-09 Registrar vacunación


actor Veterinario as V


participant "VacunacionEdicionView" as View


participant "VacunacionEdicionViewModel" as VM


participant "VeterinarioService" as VetSvc


participant "VacunacionService" as Svc


participant "VacunacionRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : desde la ficha de la mascota, "registrar vacuna"


View -> VM : CargarVeterinariosCommand


VM -> VetSvc : ListarActivos()


VetSvc --> VM : [Fabio, William]


V -> View : ingresa nombre/tipo, fecha aplicación, próxima fecha; elige veterinario


View -> VM : GuardarCommand


VM -> Svc : Registrar(vacunacion)


Svc -> Svc : valida veterinario (RN-07) y campos obligatorios


alt datos válidos


  Svc -> Repo : Agregar(vacunacion)


  Repo -> DB : INSERT INTO Vacunacion(...)


  DB --> Repo : Id generado


  Repo --> Svc : ok


  Svc --> VM : éxito (disponible para carnet y alertas si hay próxima fecha)


  VM -> View : confirma


else datos inválidos


  Svc --> VM : error


  VM -> View : no guarda


end


@enduml
CU-10 — Generar y descargar carnet digital (PDF)
@startuml SEC_CU10_CarnetPdf


title CU-10 Generar y descargar carnet digital (PDF)


actor Veterinario as V


participant "CarnetView" as View


participant "CarnetViewModel" as VM


participant "CarnetService" as Svc


participant "VacunacionRepository" as Repo


participant "GeneradorCarnetPdf" as Pdf


database "SQLite (EF Core)" as DB


V -> View : desde la ficha de la mascota, "generar carnet"


View -> VM : GenerarCommand(mascotaId)


VM -> Svc : Generar(mascotaId)


Svc -> Repo : ListarPorMascota(mascotaId)


Repo -> DB : SELECT * FROM Vacunacion WHERE MascotaId = ? ORDER BY FechaAplicacion


DB --> Repo : vacunaciones


Repo --> Svc : vacunaciones


alt la mascota tiene vacunas


  Svc --> VM : CarnetDigital (mascota + propietario + vacunas)


  VM -> View : muestra vista previa del carnet


  V -> View : "descargar PDF"


  View -> VM : ExportarPdfCommand


  VM -> Svc : ExportarPdf(carnet)


  Svc -> Pdf : Generar(carnet, ruta)


  Pdf --> Svc : ruta del archivo PDF


  Svc --> VM : ruta


  VM -> View : PDF descargado, listo para enviar


else sin vacunas registradas


  Svc --> VM : aviso "sin vacunas"


  VM -> View : informa que no hay vacunas


end


@enduml
CU-11 — Consultar alertas de próximas vacunaciones/desparasitaciones
@startuml SEC_CU11_Alertas


title CU-11 Consultar alertas de próximas fechas


actor Veterinario as V


participant "Sistema" as Sys


participant "AlertasView" as View


participant "AlertasViewModel" as VM


participant "AlertaService" as Svc


participant "VacunacionRepository" as VRepo


participant "ProcedimientoRepository" as PRepo


database "SQLite (EF Core)" as DB


Sys -> VM : al abrir la sección, dispara CargarAlertasCommand


VM -> Svc : ProximasFechas()


Svc -> VRepo : ListarConProximaFecha()


VRepo -> DB : SELECT * FROM Vacunacion WHERE ProximaFecha IS NOT NULL


DB --> VRepo : vacunaciones con refuerzo


Svc -> PRepo : ListarConProximaFecha()


PRepo -> DB : SELECT * FROM Procedimiento WHERE ProximaFechaRecomendada IS NOT NULL


DB --> PRepo : procedimientos con próxima fecha


Svc -> Svc : filtra próximas/vencidas y ordena por fecha


Svc --> VM : lista (mascota, propietario, tipo, fecha)


alt hay pendientes


  VM -> View : muestra lista de alertas


else sin pendientes


  VM -> View : lista vacía


end


V -> View : revisa; puede pasar a CU-12


@enduml
CU-12 — Enviar recordatorio por WhatsApp
@startuml SEC_CU12_Recordatorio


title CU-12 Enviar recordatorio por WhatsApp


actor Veterinario as V


participant "Sistema" as Sys


participant "AlertasView" as View


participant "AlertasViewModel" as VM


participant "RecordatorioService" as Svc


participant "GeneradorEnlaceWhatsApp" as Link


participant "RecordatorioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : en una alerta, elige "enviar recordatorio"


View -> VM : PrepararRecordatorioCommand(item)


VM -> Svc : Preparar(item)


Svc -> Svc : valida teléfono colombiano del propietario (RN-14)


alt teléfono válido


  Sys -> Svc : arma el mensaje (mascota, tipo, fecha)


  Svc -> Link : Construir(telefono, mensaje)


  Link --> Svc : https://wa.me/57XXXXXXXXXX?text=...


  Svc -> Repo : Agregar(recordatorio Estado='Pendiente')


  Repo -> DB : INSERT INTO Recordatorio(...)


  DB --> Repo : Id


  Svc --> VM : recordatorio + enlace


  VM -> View : abre el enlace en el navegador (WhatsApp)


  V -> View : envía el mensaje; marca "enviado"


  View -> VM : MarcarEnviadoCommand


  VM -> Svc : MarcarEnviado(recordatorio)


  Svc -> Repo : Actualizar(Estado='Enviado', FechaEnvio=ahora)


  Repo -> DB : UPDATE Recordatorio SET ...


  DB --> Repo : ok


else teléfono inválido / sin internet


  Svc --> VM : aviso (se pospone; demás funciones siguen · RNF-04)


  VM -> View : informa


end


@enduml
CU-13 — Gestionar veterinarios
@startuml SEC_CU13_GestionarVeterinarios


title CU-13 Gestionar veterinarios


actor Veterinario as V


participant "VeterinariosView" as View


participant "VeterinariosViewModel" as VM


participant "VeterinarioService" as Svc


participant "VeterinarioRepository" as Repo


database "SQLite (EF Core)" as DB


V -> View : abre gestión de veterinarios


View -> VM : CargarCommand


VM -> Svc : ListarActivos()


Svc -> Repo : ListarActivos()


Repo -> DB : SELECT * FROM Veterinario WHERE Activo = 1


DB --> Repo : [Fabio, William]


Repo --> Svc : lista


Svc --> VM : lista


VM -> View : muestra veterinarios


opt registrar nuevo veterinario


  V -> View : ingresa nombre (y registro profesional)


  View -> VM : GuardarCommand


  VM -> Svc : Registrar(veterinario)


  Svc -> Repo : Agregar(veterinario)


  Repo -> DB : INSERT INTO Veterinario(...)


  DB --> Repo : Id


  Svc --> VM : éxito


  VM -> View : actualiza lista (disponible para CU-08 y CU-09)


end


@enduml
3.5 Diagrama relacional de la base de datos (físico, con PK/FK)
@startuml Relacional


skinparam linetype ortho


hide circle


hide empty members


entity Usuario {


  * Id : INTEGER <<PK>>


  --


  * NombreUsuario : TEXT <<UNIQUE>>


  * ContrasenaHash : TEXT


  * Salt : TEXT


  * Activo : INTEGER


}


entity Veterinario {


  * Id : INTEGER <<PK>>


  --


  * NombreCompleto : TEXT


    RegistroProfesional : TEXT


  * Activo : INTEGER


}


entity Propietario {


  * Id : INTEGER <<PK>>


  --


  * NombreCompleto : TEXT


  * Telefono : TEXT


    Documento : TEXT


    Correo : TEXT


    Direccion : TEXT


  * Activo : INTEGER


}


entity Mascota {


  * Id : INTEGER <<PK>>


  --


  * PropietarioId : INTEGER <<FK>>


  * Nombre : TEXT


  * Especie : TEXT


    Raza : TEXT


    Sexo : TEXT


  * FechaNacimiento : TEXT


  * FechaNacimientoEstimada : INTEGER


  * Peso : REAL


    ColorSenas : TEXT


  * Activo : INTEGER


}


entity Procedimiento {


  * Id : INTEGER <<PK>>


  --


  * MascotaId : INTEGER <<FK>>


  * VeterinarioId : INTEGER <<FK>>


  * Fecha : TEXT


  * TipoProcedimiento : TEXT


  * Descripcion : TEXT


    Tratamiento : TEXT


    PesoEnElMomento : REAL


    ProximaFechaRecomendada : TEXT


}


entity Vacunacion {


  * Id : INTEGER <<PK>>


  --


  * MascotaId : INTEGER <<FK>>


  * VeterinarioId : INTEGER <<FK>>


  * NombreVacuna : TEXT


  * FechaAplicacion : TEXT


    ProximaFecha : TEXT


    Lote : TEXT


    Observaciones : TEXT


}


entity Recordatorio {


  * Id : INTEGER <<PK>>


  --


  * MascotaId : INTEGER <<FK>>


  * Tipo : TEXT


  * FechaObjetivo : TEXT


  * Mensaje : TEXT


  * Enlace : TEXT


  * Estado : TEXT


    FechaEnvio : TEXT


    VacunacionId : INTEGER <<FK>>


    ProcedimientoId : INTEGER <<FK>>


}


Propietario ||--o{ Mascota : PropietarioId


Mascota ||--o{ Procedimiento : MascotaId


Mascota ||--o{ Vacunacion : MascotaId


Veterinario ||--o{ Procedimiento : VeterinarioId


Veterinario ||--o{ Vacunacion : VeterinarioId


Mascota ||--o{ Recordatorio : MascotaId


Vacunacion ||--o{ Recordatorio : VacunacionId


Procedimiento ||--o{ Recordatorio : ProcedimientoId


note bottom of Usuario


  Sin relación a las demás tablas:


  es la cuenta de acceso (RF-01), no


  un actor del dominio clínico.


end note


@enduml


________________


4. Diseño de interfaces
4.1 Lineamientos generales
* Estilo: escritorio limpio y minimalista. Mucho aire, poca densidad, tipografía legible (Segoe UI, nativa de Windows). Pocos campos obligatorios por pantalla (RNF-01: operar sin secretaria, mínima digitación).
* Responsividad en WPF: layouts con Grid + * (estrella) y DockPanel, nunca posiciones fijas; ScrollViewer en formularios largos; MinWidth/MinHeight en la ventana. Al redimensionar, los listados se expanden y los formularios reacomodan columnas. Ventana mínima sugerida 1024×680.
* Terminología (veterinarias en Colombia, respaldada por requisitos): "propietario/dueño", "paciente/mascota", "historia clínica", "procedimiento", "vacunación", "carnet de vacunación", "desparasitación", "recordatorio". No se usan términos sin respaldo en requisitos (no hay "cita", "peluquería", "inventario" ni "factura": están fuera de alcance).
* Navegación: MainWindow como shell con menú lateral fijo. Secciones: Propietarios, Mascotas, Alertas, Veterinarios. La ficha de mascota y los formularios de procedimiento/vacuna/carnet se abren desde el contexto de la mascota.
4.2 Paleta de colores (minimalista) — Recursos/Colores.xaml
Rol
	Hex
	Uso
	Primario
	#2A9D8F
	Barra lateral, botones principales, acentos
	Primario oscuro (hover)
	#218074
	Estado hover/pressed de botones primarios
	Secundario
	#457B9D
	Enlaces, acciones secundarias
	Fondo
	#F7F9FA
	Fondo general de la aplicación
	Superficie
	#FFFFFF
	Tarjetas, formularios, listados
	Texto principal
	#1D2733
	Títulos y texto de lectura
	Texto secundario
	#5B6B7B
	Etiquetas, ayudas, metadatos
	Borde / divisor
	#E1E8ED
	Separadores, bordes de campos
	Éxito
	#2E9E5B
	Confirmaciones, estados "al día"
	Advertencia (alerta)
	#E8A33D
	Próximas a vencer (alertas)
	Error
	#D64545
	Validaciones, vencidas, acciones destructivas
	4.3 Pantallas (cada una respaldada por requisito/CU — nada inventado)
P-01 · Login — (CU-01 · RF-01, RNF-05) Contenido: logo/título; campo Usuario; campo Contraseña (oculto); botón Ingresar; zona de mensaje de error. Sin opción de registro (único usuario, SUP-07). Botón Cerrar sesión disponible luego en el shell.


P-02 · Shell principal (MainWindow) — (transversal) Menú lateral fijo: Propietarios · Mascotas · Alertas · Veterinarios. Encabezado con nombre del sistema y Cerrar sesión. Área de contenido que hospeda las vistas.


P-03 · Propietarios (lista + búsqueda) — (CU-03 · RF-03) Barra de búsqueda (por nombre o teléfono); tabla de propietarios (Nombre, Teléfono, N.º de mascotas, Estado); botón Nuevo propietario; acciones por fila Ver/Editar. Al seleccionar un propietario se listan sus mascotas asociadas.


P-04 · Formulario de propietario (nuevo/editar) — (CU-02, CU-04 · RF-02, RF-04, RN-04, RN-14) Campos: Nombre completo (obligatorio), Teléfono celular colombiano (obligatorio, validación en vivo del formato 3#########), Documento, Correo, Dirección (opcionales). Botones Guardar / Cancelar. Opción Inactivar (no eliminar, SUP-08) en modo edición.


P-05 · Mascotas (búsqueda) — (entrada a CU-06 · RF-06) Búsqueda por nombre de mascota o propietario; tabla (Mascota, Especie, Propietario, Edad calculada, Estado); botón Nueva mascota; acción Abrir ficha.


P-06 · Formulario de mascota (nueva/editar) — (CU-05, CU-07 · RF-05, RF-07, R-05, R-07) Propietario (selector obligatorio de uno existente, RN-03), Nombre (obligatorio), Especie (obligatoria, SUP-03), Raza, Sexo, Fecha de nacimiento (obligatoria) con casilla "fecha estimada" (SUP-04), Peso (Kg) (obligatorio, > 0), Color/Señas. Muestra la edad calculada en vivo (RF-08). Botones Guardar/Cancelar; Inactivar en edición.


P-07 · Ficha de mascota + historia clínica — (CU-06 · RF-06, RF-08, RF-10, RNF-09) Cabecera: datos de la mascota + edad calculada + propietario. Línea de tiempo cronológica unificada (procedimientos + vacunaciones) mostrando fecha, tipo, detalle y veterinario de cada registro. Botones de acción: Nuevo procedimiento, Registrar vacuna, Generar carnet.


P-08 · Formulario de procedimiento — (CU-08 · RF-09, RN-07) Fecha (obligatoria), Tipo de procedimiento (obligatorio: consulta/cirugía/control/desparasitación…), Descripción/diagnóstico (obligatoria), Tratamiento/observaciones, Peso en el momento (Kg, opcional), Próxima fecha recomendada (opcional, alimenta alertas), Veterinario (selector obligatorio, RN-07). Guardar/Cancelar.


P-09 · Formulario de vacunación — (CU-09 · RF-11, RN-07) Nombre/tipo de vacuna (obligatorio, texto/selección simple, SUP-10), Fecha de aplicación (obligatoria), Próxima fecha/refuerzo (opcional), Lote/laboratorio, Observaciones, Veterinario (selector obligatorio). Guardar/Cancelar.


P-10 · Carnet digital — (CU-10 · RF-12, RF-13, R-08) Vista previa del carnet: datos de mascota y propietario + tabla de vacunas (nombre, fecha, refuerzo, veterinario) + fecha de generación. Botón Descargar PDF. Si no hay vacunas, muestra aviso.


P-11 · Alertas — (CU-11, CU-12 · RF-14, RF-15) Lista de próximas/vencidas (Mascota, Propietario, Tipo [vacunación/desparasitación], Fecha objetivo), resaltadas en advertencia (próximas) o error (vencidas). Acción por fila Enviar recordatorio: muestra el mensaje armado y abre el enlace de WhatsApp (click-to-chat); luego permite marcar como enviado (Estado).


P-12 · Veterinarios — (CU-13 · RF-16) Lista de veterinarios (Fabio, William pre-registrados, SUP-06) con Nombre, Registro profesional, Estado; botón Nuevo veterinario y edición. Alimenta los selectores de P-08 y P-09.


________________


5. Matriz de trazabilidad
(a) Cada requisito → artefactos de diseño que lo cubren (ningún requisito queda sin cubrir):


Requisito
	Tablas
	Clases (servicio/VM/util)
	Caso(s) de uso
	Pantalla(s)
	RF-01
	Usuario
	AutenticacionService, HasherContrasena, LoginViewModel
	CU-01
	P-01
	RF-02
	Propietario
	PropietarioService, PropietarioEdicionViewModel
	CU-02
	P-04
	RF-03
	Propietario
	PropietarioService, PropietariosViewModel
	CU-03
	P-03
	RF-04
	Propietario
	PropietarioService, PropietarioEdicionViewModel
	CU-04
	P-04
	RF-05
	Mascota, Propietario
	MascotaService, CalculadoraEdad, MascotaEdicionViewModel
	CU-05
	P-06
	RF-06
	Mascota, Procedimiento, Vacunacion
	MascotaService, MascotaDetalleViewModel
	CU-06
	P-05, P-07
	RF-07
	Mascota
	MascotaService, MascotaEdicionViewModel
	CU-07
	P-06
	RF-08
	(deriva de Mascota.FechaNacimiento)
	CalculadoraEdad, MascotaService
	CU-05, CU-06
	P-06, P-07
	RF-09
	Procedimiento, Veterinario
	ProcedimientoService, ProcedimientoEdicionViewModel
	CU-08
	P-08
	RF-10
	Procedimiento, Vacunacion
	MascotaDetalleViewModel, HistoriaClinica
	CU-06, CU-08, CU-09
	P-07
	RF-11
	Vacunacion, Veterinario
	VacunacionService, VacunacionEdicionViewModel
	CU-09
	P-09
	RF-12
	Vacunacion, Mascota, Propietario
	CarnetService, CarnetViewModel, CarnetDigital
	CU-10
	P-10
	RF-13
	(genera desde Vacunacion)
	GeneradorCarnetPdf, CarnetService
	CU-10
	P-10
	RF-14
	Procedimiento, Vacunacion
	AlertaService, AlertasViewModel
	CU-11
	P-11
	RF-15
	Recordatorio
	RecordatorioService, GeneradorEnlaceWhatsApp, AlertasViewModel
	CU-12
	P-11
	RF-16
	Veterinario
	VeterinarioService, VeterinariosViewModel
	CU-13, CU-08, CU-09
	P-12
	RNF-01
	—
	ViewModels con validación mínima
	Transversal
	P-04, P-06, P-08, P-09
	RNF-02
	—
	App WPF de escritorio (1 proyecto ejecutable)
	Transversal
	Todas
	RNF-03
	(archivo SQLite único)
	VeterinariaDbContext
	Transversal
	—
	RNF-04
	—
	Funciones locales; solo CU-12 usa internet
	CU-12 y demás en local
	P-11
	RNF-05
	Usuario (hash+salt)
	AutenticacionService, HasherContrasena
	CU-01
	P-01
	RNF-06
	—
	GeneradorEnlaceWhatsApp (wa.me gratuito), NuGet gratuitos
	CU-12
	P-11
	RNF-07
	Procedimiento, Vacunacion + triggers NoBorrar
	(restricción de datos)
	CU-08, CU-09, CU-10
	P-07, P-10
	RNF-08
	FKs NOT NULL + CHECK + triggers
	Servicios con validación
	CU-05, CU-08
	P-06, P-08
	RNF-09
	Índices (MascotaId, Fecha)
	MascotaDetalleViewModel
	CU-06
	P-07
	RN-01
	Usuario
	AutenticacionService
	CU-01
	P-01
	RN-02
	Mascota.PropietarioId (FK)
	—
	CU-05
	P-06
	RN-03
	FK NOT NULL Mascota→Propietario
	MascotaService
	CU-05
	P-06
	RN-04
	Propietario (NOT NULL nombre/tel.)
	PropietarioService
	CU-02
	P-04
	RN-05
	(sin columna Edad)
	CalculadoraEdad
	CU-06
	P-07
	RN-06
	Mascota.Peso REAL>0
	MascotaService
	CU-05, CU-07
	P-06
	RN-07
	FK NOT NULL → Veterinario
	Procedimiento/VacunacionService
	CU-08, CU-09
	P-08, P-09
	RN-08
	triggers NoBorrar + Activo
	—
	CU-08, CU-09
	P-07
	RN-09
	Vacunacion
	CarnetService, GeneradorCarnetPdf
	CU-10
	P-10
	RN-10
	Recordatorio.Enlace
	GeneradorEnlaceWhatsApp
	CU-12
	P-11
	RN-11
	Procedimiento/Vacunacion.ProximaFecha
	AlertaService
	CU-11
	P-11
	RN-12
	(1 archivo SQLite)
	VeterinariaDbContext
	Transversal
	—
	RN-13
	Usuario
	AutenticacionService
	CU-01
	P-01
	RN-14
	Propietario.Telefono CHECK GLOB
	PropietarioService, GeneradorEnlaceWhatsApp
	CU-02, CU-12
	P-04, P-11
	RN-15
	—
	Arquitectura local; solo CU-12 requiere red
	CU-12
	P-11
	

(b) Cada artefacto → su requisito de origen (nada agregado de más): cada tabla (2.1–2.7), clase (3.2), caso de uso (3.4) y pantalla (4.3) lleva entre paréntesis el/los ID(s) de requisito que lo originan. No existe ninguna tabla, clase o pantalla sin requisito que la respalde (p. ej., no hay módulos de inventario, facturación, citas ni peluquería — están fuera de alcance por requisitos §1.2).


________________


6. Supuestos de diseño (SUP-D)
ID
	Decisión tomada
	Justificación
	Requisito(s) relacionado(s)
	SUP-D01
	Las fechas se almacenan como TEXT ISO-8601 (yyyy-MM-dd / yyyy-MM-ddTHH:mm:ss).
	SQLite no tiene tipo fecha nativo; EF Core mapea DateTime a TEXT así por defecto, conservando orden lexicográfico = cronológico.
	R-07, R-10, RF-10
	SUP-D02
	Estado Activo/Inactivo se modela como INTEGER (1/0), DEFAULT 1.
	Borrado lógico simple y consistente en todas las tablas.
	SUP-08, RN-08
	SUP-D03
	Contraseña con PBKDF2 (Rfc2898DeriveBytes, HMAC-SHA256, 100 000 iteraciones), guardando hash + salt en base64.
	RNF-05 exige credenciales no en texto plano ni quemadas; PBKDF2 es nativo de .NET, sin dependencias de pago.
	RNF-05, R-04, RN-01
	SUP-D04
	La historia clínica no es tabla física; es agregación por consulta de Procedimiento + Vacunacion ordenada por fecha.
	§4.8 la define como agregado, no como formulario aparte.
	RF-10, RNF-07
	SUP-D05
	El carnet digital no se persiste como tabla; se genera on-demand y se exporta a PDF.
	§4.7 lo define como artefacto generado, no capturado a mano.
	RF-12, RF-13
	SUP-D06
	Recordatorio se persiste con Estado Pendiente/Enviado y origen vía dos FK nulables (Vacunacion/Procedimiento) + CHECK de exactamente-una.
	§4.9 pide Estado para no repetir avisos; el origen puede ser vacuna o desparasitación.
	RF-15, SUP-09
	SUP-D07
	Teléfono se guarda como 10 dígitos TEXT validados 3#########; el enlace wa.me antepone 57.
	RN-14 define formato celular colombiano; wa.me requiere código país.
	RN-14, SUP-02, RF-15
	SUP-D08
	Generación de PDF con QuestPDF (licencia Community, gratuita para empresas bajo el umbral de ingresos; la clínica califica).
	RF-13 exige PDF; RNF-06 exige costo cero.
	RF-13, RNF-06
	SUP-D09
	Inyección de dependencias con Microsoft.Extensions.DependencyInjection.
	Incluido en .NET, gratuito; desacopla capas (RNF-08).
	RNF-06, RNF-08
	SUP-D10
	Triggers SQLite BEFORE DELETE … RAISE(ABORT) en Propietario, Mascota, Procedimiento y Vacunacion.
	Garantiza conservación (Ley 576); el borrado solo es lógico.
	RN-08, SUP-08, RNF-07
	SUP-D11
	Especie y Tipo de procedimiento como texto libre con sugerencias, sin catálogo cerrado.
	RNF-01 pide mínima digitación/simplicidad; SUP-03/SUP-10 no exigen catálogo.
	RNF-01, SUP-03, SUP-10
	SUP-D12
	Único usuario del sistema; Fabio y William se precargan como veterinarios en el seed.
	SUP-06/SUP-07: un login, dos profesionales atribuibles.
	SUP-06, SUP-07, R-03
	SUP-D13
	Target: .NET 8 (LTS) con WPF.
	Versión soportada a largo plazo; evita ambigüedad de framework.
	RNF-02
	SUP-D14
	Peso en REAL (Kg), CHECK > 0, tanto en Mascota como en Procedimiento.
	R-07/RN-06 fijan Kg; evita pesos inválidos.
	R-07, RN-06, SUP-05
	SUP-D15
	La edad se calcula en memoria (CalculadoraEdad); no existe columna Edad.
	RF-08/RN-05 exigen cálculo dinámico, no valor fijo.
	RF-08, RN-05
	

________________


7. Autoverificación de criterios de aceptación
* Toda decisión no explícita en requisitos quedó resuelta y registrada como SUPUESTO (SUP-D01…SUP-D15, sección 6).
* No quedó ninguna decisión técnica abierta ni delegada al desarrollador (sin "TBD", sin "a criterio de…").
* El DDL de SQLite corre tal cual, sin edición (sección 2.9: PRAGMA, CREATE TABLE, índices, triggers y seed).
* El modelo de datos incluye tipos SQLite exactos, PK, FK, NOT NULL, UNIQUE, DEFAULT y las reglas de integridad de los requisitos (teléfono colombiano, mascota con propietario obligatorio, no-borrado de historia clínica).
* El diagrama de casos de uso incluye TODOS los casos de uso (CU-01…CU-13).
* El diagrama de clases incluye TODAS las clases (entidades, modelos generados, DbContext, repositorios, servicios, utilidades, ViewModels).
* Hay un diagrama de secuencia por cada caso de uso: 13 CU = 13 secuencias (verificado en 3.3).
* Está el diagrama relacional (físico con PK/FK), no el entidad-relación conceptual (sección 3.5).
* Todas las pantallas (P-01…P-12) están respaldadas por requisitos/casos de uso; ninguna inventada (no hay inventario, facturación, citas ni peluquería).
* Cada tabla, clase y pantalla está trazada a su(s) requisito(s) (RF/RNF/RN).
* La matriz de trazabilidad (sección 5) no deja requisitos sin cubrir ni artefactos sin origen.


Afirmación final: un desarrollador puede implementar este sistema sin hacer ni una sola pregunta adicional.