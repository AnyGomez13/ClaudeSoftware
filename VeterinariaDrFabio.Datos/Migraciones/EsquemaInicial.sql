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
