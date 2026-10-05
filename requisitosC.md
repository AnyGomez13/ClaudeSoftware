Levantamiento de Requisitos — Software de Gestión para Clínica Veterinaria
Proyecto: Sistema de gestión clínica para la veterinaria del Dr. Fabio (Piedecuesta). Fase: Levantamiento y organización de requisitos (insumo único para la fase de diseño). Fuente de verdad: Caso de estudio "Informe de levantamiento de información – Clínica veterinaria" (§3.1.1 a §3.1.5) + restricciones específicas definidas para el caso. Norma de redacción de requisitos: IEEE 830.


Nota de lectura: este documento describe qué hace el software, por qué lo hace y bajo qué restricciones opera. No incluye decisiones de diseño (motor de base de datos, arquitectura, framework, interfaz, tipos de datos físicos): eso corresponde a la siguiente fase. Donde el caso no fue explícito, se resolvió de la forma más razonable y se marcó como SUPUESTO (SUP-XX).


________________


1. Prioridades del cliente
El cliente reconoce que sistematizar toda la operación es inviable en una primera versión, porque hoy no hay sistematización de ningún tipo. Por eso enfoca el alcance en el control clínico y la fidelización. (Caso §3.1.1 y §3.1.3).
1.1. Lo que el cliente quiere AHORA (dentro del alcance)
* Gestión y control del historial médico de los pacientes (registro de mascotas y propietarios).
* Registro cronológico y detallado de los procedimientos médicos.
* Control de esquemas de vacunación y generación de carnet digital descargable.
* Envío de recordatorios por WhatsApp a los clientes para fidelización, sin costo operativo adicional.
* Inicio de sesión obligatorio para proteger la información clínica (restricción del caso).
1.2. Lo que NO es prioridad ahora (fuera del alcance, se deja registrado)
* Módulo de inventario (medicamentos, alimentos, accesorios) con alertas de vencimiento y desabastecimiento. (El cliente lo mencionó como problema, pero lo dejó explícitamente fuera — Caso §3.1.3).
* Módulo de facturación electrónica y contabilidad.
* Gestión de citas y recordatorios de peluquería y relación con el peluquero.
* Gestión de proveedores y pedidos de mercancía.
* Multiusuario / múltiples roles / múltiples computadores (el sistema es de un solo equipo y un solo usuario).
* Envío 100% automático de recordatorios mediante API de pago de WhatsApp (ver SUP-01).


________________


2. Restricciones del caso (obligatorias)
ID
	Restricción
	Origen
	R-01
	El software opera desde un único computador, con una sola base de datos interna.
	Caso §3.1.3 + Restricción del caso
	R-02
	Requiere conexión a internet exclusivamente para enviar recordatorios por WhatsApp; el resto de funciones operan localmente.
	Caso §3.1.3
	R-03
	Hay un único usuario del sistema (rol: veterinario), pero dos veterinarios reales (Fabio y William). Cada procedimiento debe registrar el nombre del veterinario que lo evaluó.
	Caso §3.1.1 / §3.1.3 + Restricción del caso
	R-04
	El login es estrictamente necesario. Las credenciales se validan contra la base de datos, nunca quemadas (hardcodeadas) en el código.
	Restricción del caso
	R-05
	Un propietario puede tener varias mascotas. Al crear una mascota debe poder seleccionarse un propietario ya existente.
	Restricción del caso
	R-06
	El propietario requiere como mínimo nombre completo y teléfono en formato colombiano (para poder comunicarse / enviar recordatorios).
	Restricción del caso
	R-07
	La edad de la mascota no es fija: se calcula a partir de la fecha de nacimiento y se actualiza con el tiempo. El peso se maneja en kilogramos (Kg).
	Restricción del caso
	R-08
	El carnet digital lleva el registro de vacunación de la mascota y debe poder generarse en PDF y descargarse para enviárselo al cliente.
	Restricción del caso
	R-09
	Los recordatorios deben enviarse por un canal gratuito mediante un link (sin costo). Se admite cualquier otra alternativa también gratuita.
	Restricción del caso
	R-10
	La historia clínica debe registrarse de forma cronológica, garantizar trazabilidad y conservarse (sustituye el cuaderno).
	Caso §3.1.4 — Ley 576 de 2000, art. 61
	

________________


3. Supuestos (SUP)
Resoluciones razonables donde el caso no fue explícito. Nada de esto se presenta como pedido directo del cliente.


ID
	Supuesto
	Justificación
	SUP-01
	El "canal gratuito mediante un link" se implementa como un enlace de WhatsApp tipo click-to-chat (enlace wa.me que abre WhatsApp con el mensaje ya escrito hacia el contacto). El sistema arma el mensaje y el enlace; el veterinario lo envía con un clic. Esto reconcilia el caso (WhatsApp) con la restricción (link gratuito) y mantiene costo cero porque no usa la API de pago de WhatsApp. El envío es semiautomático, no 100% automático.
	Caso §3.1.3 + Restricción R-09
	SUP-02
	El teléfono del propietario es un celular colombiano (no fijo), porque debe ser utilizable por WhatsApp.
	Deriva de R-06 + R-09
	SUP-03
	La mascota maneja especie (perro, gato, etc.), dato clínico mínimo necesario para la atención aunque el caso no lo liste.
	Sentido clínico del dominio
	SUP-04
	Si no se conoce la fecha de nacimiento exacta, se permite registrar una fecha estimada, para poder calcular la edad igualmente.
	Deriva de R-07; el cuaderno hoy anota "edad" aproximada
	SUP-05
	El peso puede capturarse también por procedimiento/atención (no solo al crear la mascota), para dar seguimiento clínico a su evolución.
	El cuaderno hoy registra el peso en cada consulta (Caso §3.1.1)
	SUP-06
	Los dos veterinarios (Fabio, William) quedan pre-registrados como profesionales seleccionables, de modo que cada procedimiento pueda atribuirse a uno de ellos. Sigue habiendo un solo login.
	Deriva de R-03
	SUP-07
	El usuario del sistema inicial (credenciales) se crea durante la instalación/puesta en marcha, ya que el cliente no pidió un módulo de administración de usuarios.
	Deriva de R-04 + alcance de un solo usuario
	SUP-08
	No se permite el borrado definitivo de información clínica (historia, procedimientos, vacunaciones). Propietarios y mascotas pueden inactivarse, no eliminarse en duro.
	Deriva de R-10 (Ley 576: conservación de la historia clínica)
	SUP-09
	Los recordatorios cubren vacunación y desparasitación (ciclos clínicos con próxima fecha). Los recordatorios de peluquería quedan fuera, por estar fuera del alcance clínico.
	Deriva de §3.1.3
	SUP-10
	El nombre/tipo de vacuna se captura como texto o selección simple; no se exige un catálogo completo de vacunas.
	Simplicidad pedida por el cliente (Caso §3.1.1)
	

________________


4. Objetos del dominio y atributos
Para cada objeto se define la lista de atributos que maneja. Se describe el contenido lógico de cada campo (no su tipo de dato físico, que es de la fase de diseño).
4.1. Usuario (cuenta de acceso al sistema)
Cuenta única con la que se inicia sesión. Es distinto de "Veterinario".


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del usuario
	Sí
	Uso interno
	Nombre de usuario
	Credencial de acceso (usuario)
	Sí
	Se valida contra la BD (R-04)
	Contraseña
	Credencial de acceso (clave)
	Sí
	Nunca hardcodeada; debe almacenarse de forma protegida (ver RNF-05)
	Estado
	Activo / inactivo
	No
	Único usuario operativo
	4.2. Veterinario (profesional que atiende)
Profesional cuyo nombre se atribuye a cada procedimiento. Son dos: Fabio y William.


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del veterinario
	Sí
	Uso interno
	Nombre completo
	Nombre del veterinario
	Sí
	Es el nombre que queda en cada procedimiento (R-03)
	Registro / tarjeta profesional
	Número de matrícula profesional
	No
	Útil para la historia clínica legal (SUP)
	Estado
	Activo / inactivo
	No
	

	4.3. Propietario
Dueño de una o varias mascotas.


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del propietario
	Sí
	Uso interno
	Nombre completo
	Nombre y apellidos del propietario
	Sí
	Mínimo exigido (R-06)
	Teléfono
	Número de contacto en formato colombiano
	Sí
	Mínimo exigido (R-06); celular para WhatsApp (SUP-02)
	Documento de identidad
	Cédula u otro documento
	No
	SUP, para identificación
	Correo electrónico
	Correo de contacto
	No
	SUP, canal alterno
	Dirección
	Dirección de residencia
	No
	SUP
	Estado
	Activo / inactivo
	No
	No se elimina en duro (SUP-08)
	4.4. Mascota / Paciente
Animal atendido. Pertenece a un único propietario.


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno de la mascota
	Sí
	Uso interno
	Propietario
	Propietario al que pertenece
	Sí
	Debe seleccionarse uno existente (R-05)
	Nombre
	Nombre de la mascota
	Sí
	

	Especie
	Tipo de animal (perro, gato, etc.)
	Sí
	SUP-03
	Raza
	Raza de la mascota
	No
	

	Sexo
	Macho / hembra
	No
	SUP
	Fecha de nacimiento
	Fecha de nacimiento (real o estimada)
	Sí
	Base para calcular la edad (R-07, SUP-04)
	Edad
	Edad actual
	Derivado
	No se almacena fija; se calcula desde la fecha de nacimiento (R-07)
	Peso
	Peso actual de la mascota
	Sí
	Siempre en kilogramos (Kg) (R-07)
	Color / señas
	Características físicas
	No
	SUP
	Estado
	Activo / inactivo
	No
	No se elimina en duro (SUP-08)
	4.5. Procedimiento (registro clínico)
Atención médica registrada en la historia clínica de la mascota (consulta, cirugía, control, desparasitación, etc.).


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del procedimiento
	Sí
	Uso interno
	Mascota
	Mascota a la que se le realiza
	Sí
	

	Veterinario que lo evaluó
	Profesional que atendió
	Sí
	Exigido por R-03
	Fecha
	Fecha del procedimiento
	Sí
	Alimenta el orden cronológico (R-10)
	Tipo de procedimiento
	Clase de atención (consulta, cirugía, control, desparasitación, etc.)
	Sí
	

	Descripción / diagnóstico
	Qué se observó o diagnosticó
	Sí
	Núcleo de la historia clínica
	Tratamiento / observaciones
	Indicaciones, medicamentos suministrados, notas
	No
	

	Peso en el momento
	Peso de la mascota al momento de la atención (Kg)
	No
	Seguimiento de evolución (SUP-05)
	Próxima fecha recomendada
	Fecha sugerida para el siguiente control/ciclo
	No
	Alimenta alertas y recordatorios (ej. desparasitación)
	4.6. Registro de vacunación / Vacuna
Aplicación de una vacuna a una mascota. Es la base del carnet digital y de las alertas de vacunación.


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del registro
	Sí
	Uso interno
	Mascota
	Mascota vacunada
	Sí
	

	Nombre / tipo de vacuna
	Vacuna aplicada
	Sí
	Texto o selección simple (SUP-10)
	Fecha de aplicación
	Fecha en que se aplicó
	Sí
	

	Próxima fecha / refuerzo
	Fecha del siguiente refuerzo
	No
	Alimenta alertas y recordatorios (RF-14, RF-15)
	Veterinario que aplicó
	Profesional que aplicó la vacuna
	Sí
	Consistente con R-03
	Lote / laboratorio
	Identificación del lote
	No
	SUP
	Observaciones
	Notas adicionales
	No
	

	4.7. Carnet digital de vacunación
Documento generado que consolida el registro de vacunación de una mascota. Es un artefacto generado (no se captura a mano).


Atributo / Contenido
	Descripción
	Obligatorio
	Observaciones
	Mascota
	Mascota a la que corresponde el carnet
	Sí
	

	Propietario
	Dueño de la mascota
	Sí
	Para identificar y contactar
	Lista de registros de vacunación
	Vacunas aplicadas con sus fechas, refuerzos y veterinario
	Sí
	Se arma desde el objeto 4.6
	Fecha de generación
	Fecha en que se generó el carnet
	Sí
	

	Formato de salida
	Documento PDF descargable
	Sí
	Exigido por R-08
	4.8. Historia clínica (agregado cronológico)
No es un formulario aparte: es el conjunto cronológico de procedimientos (4.5) y vacunaciones (4.6) de una mascota. Se define aquí porque la Ley 576 la exige como documento obligatorio y conservable.


Atributo / Contenido
	Descripción
	Obligatorio
	Observaciones
	Mascota
	Mascota titular de la historia
	Sí
	Una historia por mascota
	Fecha de apertura
	Fecha del primer registro
	Sí
	

	Registros clínicos
	Procedimientos + vacunaciones en orden cronológico
	Sí
	Agregación de 4.5 y 4.6
	Conservación
	La información no se elimina
	Sí
	Ley 576 (R-10, SUP-08)
	4.9. Recordatorio
Aviso que se prepara para enviar al propietario por WhatsApp a partir de una próxima fecha de vacunación/desparasitación.


Atributo
	Descripción
	Obligatorio
	Observaciones
	Identificador
	Identificador interno del recordatorio
	Sí
	Uso interno
	Mascota / Propietario
	A quién se le recuerda
	Sí
	

	Tipo
	Vacunación / desparasitación
	Sí
	SUP-09
	Fecha objetivo
	Fecha próxima que origina el aviso
	Sí
	Tomada de 4.5 o 4.6
	Mensaje
	Texto del recordatorio
	Sí
	Lo arma el sistema
	Enlace de envío
	Link gratuito de WhatsApp (click-to-chat)
	Sí
	SUP-01
	Estado
	Pendiente / enviado
	No
	Para no repetir avisos (SUP)
	

________________


5. Requisitos Funcionales (RF)
Una tabla por requisito, según IEEE 830.
RF-01 — Autenticación de usuario
Campo
	Contenido
	ID
	RF-01
	Nombre
	Inicio de sesión
	Descripción
	El sistema debe exigir inicio de sesión con usuario y contraseña, validando las credenciales contra la base de datos antes de permitir el acceso a cualquier función.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-04
	Precondiciones
	Existe un usuario registrado en la base de datos (SUP-07)
	Criterios de aceptación
	Con credenciales válidas se concede acceso; con credenciales inválidas se niega y se informa; las credenciales no están escritas en el código; existe opción de cerrar sesión.
	RF-02 — Registrar propietario
Campo
	Contenido
	ID
	RF-02
	Nombre
	Registrar propietario
	Descripción
	El sistema debe permitir registrar un propietario con, como mínimo, nombre completo y teléfono en formato colombiano.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-06; Caso §3.1.3
	Precondiciones
	Usuario autenticado
	Criterios de aceptación
	No se guarda el propietario si falta nombre o teléfono; el teléfono se valida en formato colombiano (RN-14); al guardar queda disponible para asociarlo a mascotas.
	RF-03 — Consultar y buscar propietario
Campo
	Contenido
	ID
	RF-03
	Nombre
	Consultar/buscar propietario
	Descripción
	El sistema debe permitir buscar y consultar propietarios (por nombre o teléfono) y ver sus mascotas asociadas.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Deriva de R-05 y del problema de acceso a información (Caso §3.1.1)
	Precondiciones
	Usuario autenticado; existen propietarios registrados
	Criterios de aceptación
	La búsqueda retorna el propietario correcto y lista sus mascotas.
	RF-04 — Editar propietario
Campo
	Contenido
	ID
	RF-04
	Nombre
	Editar propietario
	Descripción
	El sistema debe permitir actualizar los datos de un propietario existente (ej. cambio de teléfono).
	Tipo
	Funcional
	Prioridad
	Media
	Origen
	Deriva de R-06 (contactabilidad vigente)
	Precondiciones
	Usuario autenticado; propietario existente
	Criterios de aceptación
	Los cambios se guardan y conservan las reglas mínimas (nombre y teléfono válidos).
	RF-05 — Registrar mascota asociada a un propietario existente
Campo
	Contenido
	ID
	RF-05
	Nombre
	Registrar mascota
	Descripción
	El sistema debe permitir registrar una mascota seleccionando obligatoriamente un propietario ya existente, capturando sus datos (nombre, especie, fecha de nacimiento, peso en Kg, etc.).
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-05, R-07; Caso §3.1.3
	Precondiciones
	Usuario autenticado; existe al menos un propietario
	Criterios de aceptación
	No se crea la mascota sin propietario seleccionado; el peso se captura en Kg; se exige fecha de nacimiento (real o estimada).
	RF-06 — Consultar mascota e historia clínica
Campo
	Contenido
	ID
	RF-06
	Nombre
	Consultar mascota / historia clínica
	Descripción
	El sistema debe permitir buscar una mascota y consultar su historia clínica completa (procedimientos y vacunaciones en orden cronológico), sin depender de llamadas telefónicas entre veterinarios.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Caso §3.1.1 (problema central del historial compartido)
	Precondiciones
	Usuario autenticado; mascota registrada
	Criterios de aceptación
	Se muestra la historia ordenada por fecha, con el veterinario que atendió cada registro.
	RF-07 — Editar mascota
Campo
	Contenido
	ID
	RF-07
	Nombre
	Editar mascota
	Descripción
	El sistema debe permitir actualizar los datos de una mascota (ej. peso, señas).
	Tipo
	Funcional
	Prioridad
	Media
	Origen
	Deriva de R-07 (el peso cambia en el tiempo)
	Precondiciones
	Usuario autenticado; mascota existente
	Criterios de aceptación
	Los cambios se guardan; el peso permanece en Kg.
	RF-08 — Calcular edad de la mascota
Campo
	Contenido
	ID
	RF-08
	Nombre
	Cálculo automático de edad
	Descripción
	El sistema debe calcular y mostrar la edad actual de la mascota a partir de su fecha de nacimiento, actualizándola con el paso del tiempo (sin guardarla como valor fijo).
	Tipo
	Funcional
	Prioridad
	Media
	Origen
	Restricción R-07
	Precondiciones
	La mascota tiene fecha de nacimiento registrada
	Criterios de aceptación
	La edad mostrada corresponde a la diferencia entre la fecha actual y la de nacimiento; no requiere edición manual.
	RF-09 — Registrar procedimiento médico
Campo
	Contenido
	ID
	RF-09
	Nombre
	Registrar procedimiento
	Descripción
	El sistema debe permitir registrar un procedimiento médico a una mascota, incluyendo fecha, tipo, descripción/diagnóstico y el veterinario que lo evaluó.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-03; Caso §3.1.3 (registro cronológico de procedimientos)
	Precondiciones
	Usuario autenticado; mascota registrada; existen veterinarios para seleccionar
	Criterios de aceptación
	No se guarda el procedimiento sin veterinario asignado; el registro queda en la historia clínica en orden cronológico.
	RF-10 — Consultar historia clínica cronológica
Campo
	Contenido
	ID
	RF-10
	Nombre
	Historia clínica cronológica
	Descripción
	El sistema debe presentar, por mascota, el consolidado cronológico de procedimientos y vacunaciones (la historia clínica).
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Caso §3.1.4 (Ley 576, art. 61)
	Precondiciones
	Usuario autenticado; mascota con registros
	Criterios de aceptación
	Los registros se muestran ordenados por fecha; la información es trazable y no editable al punto de perder el histórico.
	RF-11 — Registrar vacunación
Campo
	Contenido
	ID
	RF-11
	Nombre
	Registrar vacunación
	Descripción
	El sistema debe permitir registrar la aplicación de una vacuna a una mascota (nombre/tipo, fecha de aplicación, próxima fecha de refuerzo y veterinario que aplicó).
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Caso §3.1.3 (control de esquemas de vacunación)
	Precondiciones
	Usuario autenticado; mascota registrada
	Criterios de aceptación
	El registro queda disponible para el carnet digital y, si tiene próxima fecha, alimenta las alertas y recordatorios.
	RF-12 — Generar carnet digital
Campo
	Contenido
	ID
	RF-12
	Nombre
	Generar carnet digital
	Descripción
	El sistema debe generar el carnet digital de vacunación de una mascota, consolidando sus registros de vacunación con los datos de mascota y propietario.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-08; Caso §3.1.3
	Precondiciones
	Usuario autenticado; la mascota tiene al menos un registro de vacunación
	Criterios de aceptación
	El carnet refleja todas las vacunas registradas con sus fechas; es consultable en pantalla.
	RF-13 — Descargar carnet en PDF
Campo
	Contenido
	ID
	RF-13
	Nombre
	Exportar carnet a PDF
	Descripción
	El sistema debe permitir generar el carnet digital en PDF y descargarlo para enviárselo al cliente.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-08
	Precondiciones
	Carnet digital generado (RF-12)
	Criterios de aceptación
	Se obtiene un archivo PDF descargable con el contenido del carnet.
	RF-14 — Generar alertas de vacunación/desparasitación próximas
Campo
	Contenido
	ID
	RF-14
	Nombre
	Alertas de próximas fechas
	Descripción
	El sistema debe identificar y mostrar (dentro de la aplicación) las mascotas con vacunación o desparasitación próxima a vencerse, según la próxima fecha registrada.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Objetivo general (generación de alertas); Caso §3.1.3; SUP-09
	Precondiciones
	Existen registros con próxima fecha
	Criterios de aceptación
	El sistema lista las próximas fechas pendientes; permite pasar de la alerta a preparar el recordatorio (RF-15).
	RF-15 — Generar recordatorio por WhatsApp (link gratuito)
Campo
	Contenido
	ID
	RF-15
	Nombre
	Recordatorio WhatsApp
	Descripción
	El sistema debe preparar el recordatorio al propietario mediante un enlace gratuito de WhatsApp (click-to-chat) con el mensaje ya escrito, para que el veterinario lo envíe sin costo.
	Tipo
	Funcional
	Prioridad
	Alta
	Origen
	Restricción R-09; Caso §3.1.3; SUP-01
	Precondiciones
	El propietario tiene teléfono celular válido; hay conexión a internet (R-02)
	Criterios de aceptación
	El enlace abre WhatsApp con el número del propietario y el mensaje precargado; no genera costo (no usa API de pago).
	RF-16 — Gestionar y seleccionar veterinario
Campo
	Contenido
	ID
	RF-16
	Nombre
	Gestión de veterinarios
	Descripción
	El sistema debe mantener el registro de los veterinarios (Fabio y William) para poder seleccionar, en cada procedimiento y vacunación, quién lo realizó.
	Tipo
	Funcional
	Prioridad
	Media
	Origen
	Restricción R-03; SUP-06
	Precondiciones
	Usuario autenticado
	Criterios de aceptación
	Al registrar un procedimiento/vacunación se puede escoger el veterinario; su nombre queda guardado en el registro.
	

________________


6. Requisitos No Funcionales (RNF)
RNF-01 — Simplicidad y mínima digitación
Campo
	Contenido
	ID
	RNF-01
	Nombre
	Facilidad de uso / operación sin personal dedicado
	Descripción
	El sistema debe ser simple de operar, con la menor cantidad de digitación posible, de modo que el veterinario lo use sin necesidad de una secretaria o personal administrativo.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Caso §3.1.1 (abandonó VeteSoft por exceso de formularios; "que no requiera una secretaria")
	Precondiciones
	—
	Criterios de aceptación
	Los flujos de registro (mascota, procedimiento, vacuna) se completan con pocos campos obligatorios y pocos pasos.
	RNF-02 — Operación en un único computador (escritorio)
Campo
	Contenido
	ID
	RNF-02
	Nombre
	Despliegue en un solo equipo
	Descripción
	El sistema debe funcionar como aplicativo de escritorio en un único computador.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Restricción R-01; Caso §3.1.3
	Precondiciones
	—
	Criterios de aceptación
	El sistema opera completo en un equipo, sin requerir red de varios puestos.
	RNF-03 — Una única base de datos interna
Campo
	Contenido
	ID
	RNF-03
	Nombre
	Base de datos única y local
	Descripción
	Toda la información se almacena en una única base de datos interna.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Restricción R-01
	Precondiciones
	—
	Criterios de aceptación
	No hay dependencia de múltiples bases de datos ni de servicios externos para operar localmente.
	RNF-04 — Operación local sin conexión (salvo recordatorios)
Campo
	Contenido
	ID
	RNF-04
	Nombre
	Independencia de internet para funciones clínicas
	Descripción
	Las funciones clínicas deben operar sin internet; la conexión solo se requiere para enviar recordatorios por WhatsApp.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Restricción R-02; Caso §3.1.3
	Precondiciones
	—
	Criterios de aceptación
	Registrar/consultar historia, vacunas y carnet funciona sin conexión; solo el envío de recordatorios la necesita.
	RNF-05 — Seguridad y reserva de la información
Campo
	Contenido
	ID
	RNF-05
	Nombre
	Seguridad de acceso y confidencialidad
	Descripción
	El acceso debe estar protegido por autenticación; las credenciales se validan contra la base de datos y se almacenan de forma protegida (no en texto plano, no quemadas en código). La información clínica es privada y bajo reserva.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Restricción R-04; Caso §3.1.4 (Ley 576: documento sometido a reserva)
	Precondiciones
	—
	Criterios de aceptación
	Sin autenticación no hay acceso a datos clínicos; las credenciales no aparecen en el código fuente.
	RNF-06 — Sin costo operativo adicional
Campo
	Contenido
	ID
	RNF-06
	Nombre
	Costo cero de operación
	Descripción
	Las funciones del sistema, en especial el envío de recordatorios, no deben generar costos operativos adicionales al cliente.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Restricción R-09; Caso §3.1.5 ("entre más pueda ahorrar, mejor")
	Precondiciones
	—
	Criterios de aceptación
	El canal de recordatorios es gratuito (link WhatsApp); no se contratan servicios de pago para operar.
	RNF-07 — Cumplimiento legal (Ley 576 de 2000)
Campo
	Contenido
	ID
	RNF-07
	Nombre
	Trazabilidad y conservación de la historia clínica
	Descripción
	La historia clínica debe registrarse de forma cronológica, ser trazable y conservarse en el tiempo, sustituyendo el cuaderno manual.
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Caso §3.1.4 — Ley 576 de 2000, art. 61
	Precondiciones
	—
	Criterios de aceptación
	Cada registro clínico queda fechado, atribuido a un veterinario y no se elimina (SUP-08).
	RNF-08 — Consistencia e integridad de los datos
Campo
	Contenido
	ID
	RNF-08
	Nombre
	Consistencia de datos
	Descripción
	El sistema debe mantener la integridad de los datos (ej. no permitir mascotas sin propietario, ni procedimientos sin veterinario).
	Tipo
	No funcional
	Prioridad
	Alta
	Origen
	Objetivo específico 3; Restricciones R-03 y R-05
	Precondiciones
	—
	Criterios de aceptación
	No se guardan registros que rompan las relaciones obligatorias del dominio.
	RNF-09 — Acceso rápido a la historia clínica
Campo
	Contenido
	ID
	RNF-09
	Nombre
	Consulta ágil
	Descripción
	La consulta de la historia clínica de una mascota debe ser rápida, para resolver en el momento de la atención lo que hoy exige una llamada telefónica entre socios.
	Tipo
	No funcional
	Prioridad
	Media
	Origen
	Derivado del problema operativo (Caso §3.1.1)
	Precondiciones
	—
	Criterios de aceptación
	La historia de un paciente se obtiene en pocos segundos durante la atención.
	

________________


7. Reglas de negocio (RN)
ID
	Regla
	RN-01
	El acceso al sistema requiere autenticación; las credenciales se validan contra la base de datos y nunca están quemadas en el código.
	RN-02
	Un propietario puede tener una o varias mascotas; cada mascota pertenece a un único propietario.
	RN-03
	No se puede registrar una mascota sin seleccionar un propietario ya existente.
	RN-04
	Todo propietario debe tener, como mínimo, nombre completo y teléfono.
	RN-05
	La edad de la mascota no se almacena fija: se calcula desde la fecha de nacimiento y se actualiza con el tiempo.
	RN-06
	El peso de la mascota se registra siempre en kilogramos (Kg).
	RN-07
	Todo procedimiento y toda vacunación deben quedar registrados con el nombre del veterinario que los realizó, aun con un único usuario del sistema.
	RN-08
	La historia clínica se registra cronológicamente y debe conservarse; no se permite su eliminación definitiva (Ley 576 de 2000, art. 61).
	RN-09
	El carnet digital debe reflejar el registro de vacunación de la mascota y poder generarse y descargarse en PDF.
	RN-10
	Los recordatorios a clientes se envían por un canal gratuito mediante un link de WhatsApp (click-to-chat), sin costo operativo adicional.
	RN-11
	El sistema genera alertas de vacunación/desparasitación próximas con base en la próxima fecha registrada.
	RN-12
	El sistema opera sobre un único computador y una única base de datos.
	RN-13
	La información clínica es privada y bajo reserva; solo es accesible tras autenticación.
	RN-14
	El teléfono del propietario debe estar en formato colombiano (celular, código país +57, 10 dígitos iniciando en 3) para poder usarse en el link de WhatsApp.
	RN-15
	Las funciones clínicas funcionan sin internet; la conexión solo se exige para enviar recordatorios.
	

________________


8. Casos de uso (CU)
Actores: Veterinario (único usuario del sistema; lo operan Fabio o William) y Sistema (procesos automáticos de cálculo y generación de alertas/enlaces).
CU-01 — Iniciar sesión
* Actor(es): Veterinario
* Precondiciones: Existe un usuario registrado en la base de datos.
* Flujo principal: 1) El veterinario abre el sistema. 2) Ingresa usuario y contraseña. 3) El sistema valida las credenciales contra la base de datos. 4) Concede el acceso.
* Flujos alternativos: 3a) Credenciales inválidas → el sistema informa el error y solicita reintentar. / El veterinario cierra sesión al terminar.
* Postcondiciones: Sesión activa; acceso habilitado a las funciones.
CU-02 — Registrar propietario
* Actor(es): Veterinario
* Precondiciones: Sesión activa.
* Flujo principal: 1) Selecciona "nuevo propietario". 2) Ingresa nombre completo y teléfono (mínimo). 3) El sistema valida el teléfono (formato colombiano). 4) Guarda el propietario.
* Flujos alternativos: 3a) Falta un dato obligatorio o el teléfono es inválido → el sistema lo indica y no guarda.
* Postcondiciones: Propietario disponible para asociar mascotas.
CU-03 — Consultar/buscar propietario
* Actor(es): Veterinario
* Precondiciones: Sesión activa; propietarios registrados.
* Flujo principal: 1) Ingresa un criterio de búsqueda (nombre/teléfono). 2) El sistema muestra coincidencias. 3) Selecciona uno y ve sus datos y mascotas.
* Flujos alternativos: 2a) Sin resultados → el sistema lo informa.
* Postcondiciones: Información del propietario visible.
CU-04 — Editar propietario
* Actor(es): Veterinario
* Precondiciones: Sesión activa; propietario existente.
* Flujo principal: 1) Busca el propietario (CU-03). 2) Modifica datos. 3) El sistema valida y guarda.
* Flujos alternativos: 2a) Deja inválido un dato mínimo → no guarda.
* Postcondiciones: Datos actualizados.
CU-05 — Registrar mascota
* Actor(es): Veterinario
* Precondiciones: Sesión activa; existe el propietario.
* Flujo principal: 1) Selecciona "nueva mascota". 2) Selecciona el propietario existente. 3) Ingresa nombre, especie, fecha de nacimiento, peso (Kg) y demás datos. 4) El sistema calcula la edad desde la fecha de nacimiento. 5) Guarda la mascota.
* Flujos alternativos: 2a) No existe el propietario → primero lo registra (CU-02). / 3a) No se conoce la fecha exacta → ingresa fecha estimada (SUP-04).
* Postcondiciones: Mascota registrada y asociada a su propietario.
CU-06 — Consultar mascota e historia clínica
* Actor(es): Veterinario
* Precondiciones: Sesión activa; mascota registrada.
* Flujo principal: 1) Busca la mascota (por nombre/propietario). 2) El sistema muestra sus datos y la edad calculada. 3) Muestra la historia clínica cronológica (procedimientos + vacunaciones) con el veterinario de cada registro.
* Flujos alternativos: 1a) Sin resultados → lo informa.
* Postcondiciones: Historia clínica consultada sin recurrir a llamadas entre socios.
CU-07 — Editar mascota
* Actor(es): Veterinario
* Precondiciones: Sesión activa; mascota existente.
* Flujo principal: 1) Busca la mascota (CU-06). 2) Actualiza datos (ej. peso). 3) El sistema guarda.
* Postcondiciones: Datos de la mascota actualizados.
CU-08 — Registrar procedimiento médico
* Actor(es): Veterinario
* Precondiciones: Sesión activa; mascota registrada; veterinarios disponibles.
* Flujo principal: 1) Abre la mascota (CU-06). 2) Selecciona "nuevo procedimiento". 3) Ingresa fecha, tipo, descripción/diagnóstico y, si aplica, próxima fecha recomendada. 4) Selecciona el veterinario que lo evaluó. 5) Guarda; el registro entra a la historia clínica.
* Flujos alternativos: 4a) No selecciona veterinario → no guarda (RN-07).
* Postcondiciones: Procedimiento registrado en la historia cronológica.
CU-09 — Registrar vacunación
* Actor(es): Veterinario
* Precondiciones: Sesión activa; mascota registrada.
* Flujo principal: 1) Abre la mascota (CU-06). 2) Selecciona "registrar vacuna". 3) Ingresa nombre/tipo de vacuna, fecha de aplicación, próxima fecha de refuerzo (si aplica) y veterinario. 4) Guarda.
* Flujos alternativos: 3a) Con próxima fecha → se habilita para alertas/recordatorios (CU-11/CU-12).
* Postcondiciones: Vacuna registrada; disponible para el carnet y las alertas.
CU-10 — Generar y descargar carnet digital (PDF)
* Actor(es): Veterinario
* Precondiciones: Sesión activa; la mascota tiene vacunas registradas.
* Flujo principal: 1) Abre la mascota (CU-06). 2) Selecciona "generar carnet". 3) El sistema arma el carnet con los registros de vacunación. 4) El veterinario lo descarga en PDF.
* Flujos alternativos: 2a) Sin vacunas registradas → el sistema lo informa.
* Postcondiciones: Carnet en PDF disponible para enviarlo al cliente.
CU-11 — Consultar alertas de próximas vacunaciones/desparasitaciones
* Actor(es): Veterinario, Sistema
* Precondiciones: Sesión activa; existen registros con próxima fecha.
* Flujo principal: 1) El Sistema identifica mascotas con próxima fecha cercana. 2) El veterinario abre la sección de alertas. 3) Ve la lista de pendientes (mascota, propietario, tipo, fecha).
* Flujos alternativos: 3a) Sin pendientes → lista vacía.
* Postcondiciones: Veterinario enterado de los próximos vencimientos; puede pasar a CU-12.
CU-12 — Enviar recordatorio por WhatsApp
* Actor(es): Veterinario, Sistema
* Precondiciones: Hay una alerta pendiente (CU-11); el propietario tiene celular válido; hay internet.
* Flujo principal: 1) Desde la alerta, el veterinario elige "enviar recordatorio". 2) El Sistema arma el mensaje y el enlace gratuito de WhatsApp (click-to-chat) con el número del propietario. 3) El veterinario abre el enlace y envía el mensaje. 4) (Opcional) Marca el recordatorio como enviado.
* Flujos alternativos: 1a) Sin internet → se pospone el envío (las demás funciones siguen operando). / 2a) Teléfono inválido → el sistema lo indica.
* Postcondiciones: Recordatorio enviado al propietario sin costo.
CU-13 — Gestionar veterinarios
* Actor(es): Veterinario
* Precondiciones: Sesión activa.
* Flujo principal: 1) Abre la gestión de veterinarios. 2) Consulta/registra los veterinarios (Fabio, William). 3) Guarda.
* Flujos alternativos: — (quedan pre-registrados según SUP-06).
* Postcondiciones: Veterinarios disponibles para atribuir procedimientos y vacunas.


________________


9. Matriz de trazabilidad
Conecta cada requisito con su origen en el caso de estudio / restricción / supuesto y con los casos de uso que lo realizan.


Requisito
	Origen
	Caso(s) de uso
	RF-01
	R-04
	CU-01
	RF-02
	R-06; Caso §3.1.3
	CU-02
	RF-03
	R-05; Caso §3.1.1
	CU-03
	RF-04
	R-06
	CU-04
	RF-05
	R-05, R-07; Caso §3.1.3
	CU-05
	RF-06
	Caso §3.1.1
	CU-06
	RF-07
	R-07
	CU-07
	RF-08
	R-07
	CU-05, CU-06
	RF-09
	R-03; Caso §3.1.3
	CU-08
	RF-10
	Caso §3.1.4 (Ley 576)
	CU-06, CU-08, CU-09
	RF-11
	Caso §3.1.3
	CU-09
	RF-12
	R-08; Caso §3.1.3
	CU-10
	RF-13
	R-08
	CU-10
	RF-14
	Objetivo general; §3.1.3; SUP-09
	CU-11
	RF-15
	R-09; §3.1.3; SUP-01
	CU-12
	RF-16
	R-03; SUP-06
	CU-13, CU-08, CU-09
	RNF-01
	Caso §3.1.1 (VeteSoft abandonado)
	Transversal (CU-02, CU-05, CU-08, CU-09)
	RNF-02
	R-01; Caso §3.1.3
	Transversal
	RNF-03
	R-01
	Transversal
	RNF-04
	R-02; Caso §3.1.3
	CU-12 (y demás en local)
	RNF-05
	R-04; Caso §3.1.4
	CU-01, transversal
	RNF-06
	R-09; Caso §3.1.5
	CU-12
	RNF-07
	Caso §3.1.4 (Ley 576)
	CU-08, CU-09, CU-10
	RNF-08
	Objetivo específico 3; R-03, R-05
	CU-05, CU-08
	RNF-09
	Caso §3.1.1 (problema operativo)
	CU-06
	

________________


10. Marco normativo aplicable
* Ley 576 de 2000 — Código de ética para el ejercicio de la Medicina Veterinaria (art. 61): obliga a la apertura y conservación de la historia clínica de los animales tratados. Es un documento privado, obligatorio y sometido a reserva, con registro cronológico de las condiciones de salud, los actos médicos y los procedimientos. El software debe garantizar su trazabilidad y conservación, sustituyendo el cuaderno manual. (Impacta RF-09, RF-10, RNF-05, RNF-07 y RN-08).