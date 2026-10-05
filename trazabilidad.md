# trazabilidad.md — Matriz de cobertura

Prioridad según `requisitosC.md`: Alta = Must, Media = Should (supuesto A-02 de `fases.md`). Prefijos de ruta: `Dom`, `Dat`, `Neg`, `App`, `Pru` como en `fases.md`. `*` = todas las vistas o pruebas de ese módulo. La Fase 15 solo empaqueta y revisa; la verificación de sistema y de usuario es de la fase de pruebas del SDLC.

## Requisitos funcionales
| ID | Prior. | Módulo | Archivos | Fase |
|---|---|---|---|---|
| RF-01 | Alta (Must) | Autenticación | `Neg/Servicios/AutenticacionService.cs`, `Neg/Utilidades/HasherContrasena.cs`, `Dat/Repositorios/UsuarioRepository.cs`, `App/ViewModels/LoginViewModel.cs`, `App/Vistas/LoginView.xaml` | 5, 6, 10 |
| RF-02 | Alta (Must) | Propietarios | `Neg/Servicios/PropietarioService.cs`, `Dat/Repositorios/PropietarioRepository.cs`, `App/ViewModels/PropietarioEdicionViewModel.cs`, `App/Vistas/PropietarioEdicionView.xaml` | 7, 11 |
| RF-03 | Alta (Must) | Propietarios | `Neg/Servicios/PropietarioService.cs`, `Dat/Repositorios/PropietarioRepository.cs`, `App/ViewModels/PropietariosViewModel.cs`, `App/Vistas/PropietariosView.xaml` | 4, 7, 11 |
| RF-04 | Media (Should) | Propietarios | `Neg/Servicios/PropietarioService.cs`, `App/ViewModels/PropietarioEdicionViewModel.cs` | 7, 11 |
| RF-05 | Alta (Must) | Mascotas | `Neg/Servicios/MascotaService.cs`, `Dat/Repositorios/MascotaRepository.cs`, `App/ViewModels/MascotaEdicionViewModel.cs`, `App/Vistas/MascotaEdicionView.xaml` | 7, 12 |
| RF-06 | Alta (Must) | Mascotas / historia | `Neg/Servicios/MascotaService.cs`, `Dom/Modelos/HistoriaClinica.cs`, `App/ViewModels/MascotasViewModel.cs`, `App/ViewModels/MascotaDetalleViewModel.cs`, `App/Vistas/MascotasView.xaml`, `App/Vistas/MascotaDetalleView.xaml` | 4, 8, 12 |
| RF-07 | Media (Should) | Mascotas | `Neg/Servicios/MascotaService.cs`, `App/ViewModels/MascotaEdicionViewModel.cs` | 7, 12 |
| RF-08 | Media (Should) | Mascotas | `Neg/Utilidades/CalculadoraEdad.cs`, `Neg/Servicios/MascotaService.cs`, `App/ViewModels/MascotaEdicionViewModel.cs`, `App/ViewModels/MascotaDetalleViewModel.cs` | 5, 7, 12 |
| RF-09 | Alta (Must) | Procedimientos | `Neg/Servicios/ProcedimientoService.cs`, `Dat/Repositorios/ProcedimientoRepository.cs`, `App/ViewModels/ProcedimientoEdicionViewModel.cs`, `App/Vistas/ProcedimientoEdicionView.xaml` | 8, 13 |
| RF-10 | Alta (Must) | Historia clínica | `Dom/Modelos/HistoriaClinica.cs`, `Dom/Modelos/RegistroClinicoItem.cs`, `Neg/Servicios/MascotaService.cs`, `App/ViewModels/MascotaDetalleViewModel.cs`, `App/Vistas/MascotaDetalleView.xaml` | 4, 8, 12 |
| RF-11 | Alta (Must) | Vacunación | `Neg/Servicios/VacunacionService.cs`, `Dat/Repositorios/VacunacionRepository.cs`, `App/ViewModels/VacunacionEdicionViewModel.cs`, `App/Vistas/VacunacionEdicionView.xaml` | 8, 13 |
| RF-12 | Alta (Must) | Carnet | `Dom/Modelos/CarnetDigital.cs`, `Neg/Servicios/CarnetService.cs`, `App/ViewModels/CarnetViewModel.cs`, `App/Vistas/CarnetView.xaml` | 9, 14 |
| RF-13 | Alta (Must) | Carnet | `Neg/Utilidades/GeneradorCarnetPdf.cs`, `Neg/Servicios/CarnetService.cs`, `App/ViewModels/CarnetViewModel.cs` | 9, 14 |
| RF-14 | Alta (Must) | Alertas | `Neg/Servicios/AlertaService.cs`, `App/ViewModels/AlertasViewModel.cs`, `App/Vistas/AlertasView.xaml` | 9, 14 |
| RF-15 | Alta (Must) | Recordatorios | `Neg/Utilidades/GeneradorEnlaceWhatsApp.cs`, `Neg/Servicios/RecordatorioService.cs`, `Dat/Repositorios/RecordatorioRepository.cs`, `App/ViewModels/AlertasViewModel.cs` | 5, 9, 14 |
| RF-11 (ampliado, CC-02) | Alta (Must) | Vacunación: listado y refuerzo propuesto | `Neg/Utilidades/CatalogoVacunas.cs`, `Neg/Utilidades/TextoComparable.cs`, `App/ViewModels/VacunacionEdicionViewModel.cs`, `App/Vistas/VacunacionEdicionView.xaml` | post-14 |
| RF-16 | Media (Should) | Veterinarios | `Neg/Servicios/VeterinarioService.cs`, `Dat/Repositorios/VeterinarioRepository.cs`, `App/ViewModels/VeterinariosViewModel.cs`, `App/Vistas/VeterinariosView.xaml` | 6, 11 |

## Requisito agregado a pedido del usuario (después de la Fase 14)
| ID | Prior. | Módulo | Archivos | Fase |
|---|---|---|---|---|
| RF-17 — Cambiar la contraseña de la cuenta (CU-14, P-13) | Solicitado (post-diseño) | Configuración / autenticación | `Neg/Servicios/AutenticacionService.cs` (`CambiarContrasena`), `Dat/Repositorios/UsuarioRepository.cs` (`Actualizar`), `App/ViewModels/ConfiguracionViewModel.cs`, `App/Vistas/ConfiguracionView.xaml`, `App/ViewModels/MainViewModel.cs` (sección Configuración) | post-14 |
| CC-01 — Nombre "Villa de San Carlos" | Solicitado | Transversal | `Dom/Clinica.cs` y sus usos en App y Negocio | post-14 |
| CC-04 — Volver en la ficha de la mascota (P-07) | Solicitado | Mascotas | `App/ViewModels/MascotaDetalleViewModel.cs`, `App/Vistas/MascotaDetalleView.xaml` | post-14 |

## Requisitos no funcionales
| ID | Prior. | Módulo | Archivos | Fase |
|---|---|---|---|---|
| RNF-01 | Alta (Must) | UI transversal | `App/ViewModels/*EdicionViewModel.cs`, `App/Vistas/*EdicionView.xaml` | 10–14 |
| RNF-02 | Alta (Must) | Solución | `VeterinariaDrFabio.sln`, `App/App.xaml.cs` | 1, 15 |
| RNF-03 | Alta (Must) | Datos | `Dat/Contexto/VeterinariaDbContext.cs`, `Dat/Migraciones/*` | 3, 15 |
| RNF-04 | Alta (Must) | Transversal / recordatorios | `Neg/Servicios/RecordatorioService.cs`, `App/ViewModels/AlertasViewModel.cs` | 9, 14 |
| RNF-05 | Alta (Must) | Seguridad | `Neg/Utilidades/HasherContrasena.cs`, `Neg/Servicios/AutenticacionService.cs`, `Dat/Migraciones/*` (tabla Usuario) | 3, 5, 6, 10, 15 |
| RNF-06 | Alta (Must) | Transversal | `*.csproj`, `Neg/Utilidades/GeneradorEnlaceWhatsApp.cs` | 1, 5, 9, 15 |
| RNF-07 | Alta (Must) | Datos / historia | `Dat/Migraciones/*` (triggers NoBorrar), `Neg/Servicios/ProcedimientoService.cs`, `Neg/Servicios/VacunacionService.cs` | 3, 8 |
| RNF-08 | Alta (Must) | Datos / servicios | `Dat/Migraciones/*` (FK, CHECK), `Neg/Servicios/*Service.cs` | 3, 4, 7, 8 |
| RNF-09 | Media (Should) | Historia clínica | `Dat/Migraciones/*` (índices), `Dat/Repositorios/{Procedimiento,Vacunacion}Repository.cs`, `App/ViewModels/MascotaDetalleViewModel.cs` | 3, 8, 12 |

## Reglas de negocio
| ID | Módulo | Archivos | Fase |
|---|---|---|---|
| RN-01 | Autenticación | `Neg/Servicios/AutenticacionService.cs` | 6 |
| RN-02 | Dominio / datos | `Dom/Entidades/Mascota.cs`, `Dat/Configuracion/MascotaConfiguration.cs` | 2, 3 |
| RN-03 | Mascotas | `Neg/Servicios/MascotaService.cs`, FK `Mascota.PropietarioId` | 3, 7 |
| RN-04 | Propietarios | `Neg/Servicios/PropietarioService.cs` | 7 |
| RN-05 | Mascotas | `Neg/Utilidades/CalculadoraEdad.cs` | 5 |
| RN-06 | Mascotas | `Neg/Servicios/MascotaService.cs`, CHECK `Peso > 0` | 3, 7 |
| RN-07 | Procedimientos / vacunación | `Neg/Servicios/{Procedimiento,Vacunacion}Service.cs` | 8 |
| RN-08 | Datos / historia | triggers NoBorrar en `Dat/Migraciones/*` | 3, 8 |
| RN-09 | Carnet | `Neg/Servicios/CarnetService.cs`, `Neg/Utilidades/GeneradorCarnetPdf.cs` | 9 |
| RN-10 | Recordatorios | `Neg/Utilidades/GeneradorEnlaceWhatsApp.cs` | 5, 9 |
| RN-11 | Alertas | `Neg/Servicios/AlertaService.cs` | 9 |
| RN-12 | Datos | `Dat/Contexto/VeterinariaDbContext.cs` | 3 |
| RN-13 | Autenticación | `Neg/Servicios/AutenticacionService.cs`, `App/ViewModels/MainViewModel.cs` | 6, 10 |
| RN-14 | Propietarios / WhatsApp | `Neg/Servicios/PropietarioService.cs`, `Neg/Utilidades/GeneradorEnlaceWhatsApp.cs`, CHECK `Telefono` | 3, 5, 7 |
| RN-15 | Transversal | `Neg/Servicios/RecordatorioService.cs` | 9 |

## Casos de uso
| ID | Servicio | ViewModel / Vista | Pantalla | Fase |
|---|---|---|---|---|
| CU-01 | `AutenticacionService` | `LoginViewModel` / `LoginView` | P-01 | 6, 10 |
| CU-02 | `PropietarioService` | `PropietarioEdicionViewModel` / `PropietarioEdicionView` | P-04 | 7, 11 |
| CU-03 | `PropietarioService` | `PropietariosViewModel` / `PropietariosView` | P-03 | 7, 11 |
| CU-04 | `PropietarioService` | `PropietarioEdicionViewModel` / `PropietarioEdicionView` | P-04 | 7, 11 |
| CU-05 | `MascotaService`, `CalculadoraEdad` | `MascotaEdicionViewModel` / `MascotaEdicionView` | P-06 | 7, 12 |
| CU-06 | `MascotaService` | `MascotasViewModel`, `MascotaDetalleViewModel` / `MascotasView`, `MascotaDetalleView` | P-05, P-07 | 8, 12 |
| CU-07 | `MascotaService` | `MascotaEdicionViewModel` / `MascotaEdicionView` | P-06 | 7, 12 |
| CU-08 | `ProcedimientoService` | `ProcedimientoEdicionViewModel` / `ProcedimientoEdicionView` | P-08 | 8, 13 |
| CU-09 | `VacunacionService` | `VacunacionEdicionViewModel` / `VacunacionEdicionView` | P-09 | 8, 13 |
| CU-10 | `CarnetService`, `GeneradorCarnetPdf` | `CarnetViewModel` / `CarnetView` | P-10 | 9, 14 |
| CU-11 | `AlertaService` | `AlertasViewModel` / `AlertasView` | P-11 | 9, 14 |
| CU-12 | `RecordatorioService`, `GeneradorEnlaceWhatsApp` | `AlertasViewModel` / `AlertasView` | P-11 | 9, 14 |
| CU-13 | `VeterinarioService` | `VeterinariosViewModel` / `VeterinariosView` | P-12 | 6, 11 |

## Verificación de cobertura
- RF-01 a RF-16: 16 de 16 con módulo, archivos y fase.
- RNF-01 a RNF-09: 9 de 9 con módulo, archivos y fase.
- RN-01 a RN-15: 15 de 15. CU-01 a CU-13: 13 de 13.
- Requisitos Must (Alta) sin cobertura: ninguno.
- Requisitos Should (Media) sin cobertura: ninguno.
- Pantallas P-01 a P-12: P-01 (F10), P-02 (F10), P-03/P-04/P-12 (F11), P-05/P-06/P-07 (F12), P-08/P-09 (F13), P-10/P-11 (F14).
