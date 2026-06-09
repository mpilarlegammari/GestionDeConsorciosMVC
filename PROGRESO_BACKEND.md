# Progreso Backend

Este archivo resume el estado actual del backend tomando como guía:

- `docs/backend-notes.md`
- `C:\Users\Usuario\Desktop\PLANIFICACIÓN (backend).docx`
- `C:\Users\Usuario\Desktop\PLANIFICACIÓN.docx`

## Estado general

- La solución `GestionDeConsorciosMVC.sln` compila correctamente con `dotnet build` en `0 errores`.
- La base local `GestionConsorciosDataBase` existe en `localhost\SQLEXPRESS01` y está `ONLINE`.
- La migración aplicada en SQL Server es `20260606022518_InicialGestionConsorcios`.
- El `DbContext` activo está en `Context/GestionDeConsorciosContext.cs`.
- Hay backend real para Consorcios + Unidades Funcionales, backend administrativo reforzado para gastos, y backend parcial para expensas y pagos.
- Hay servicios y ViewModel creados para gastos, y `IGastoService`/`IFileStorageService` ya están registrados en el `Program.cs` real.
- `dotnet ef migrations list --context GestionDeConsorciosContext` funciona usando `GESTION_CONSORCIOS_CONNECTION_STRING` como variable de entorno temporal.
- La solución `.sln` y `GestionDeConsorciosMVC.slnLaunch` existen para Visual Studio. En el árbol actual no aparece `Properties/launchSettings.json`, por lo que conviene verificar los perfiles `http`/`https` antes de dar por cerrada la ejecución desde el botón verde.

## Tabla de seguimiento

| Número de paso | Módulo o funcionalidad | Prioridad | Estado | Evidencia en el código | Qué falta hacer | Próxima acción recomendada |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | Base técnica inicial | Alta | Parcial | `GestionDeConsorciosMVC.csproj`, `GestionDeConsorciosMVC.sln`, carpetas `Models`, `Controllers`, `Context`, `ViewModels`, `Services`, `Migrations`, `wwwroot/uploads`; build en `0 errores`; servicios de gastos registrados en `Program.cs`; EF CLI funciona con variable de entorno. | Revisar `Properties/launchSettings.json`; decidir qué hacer con archivos heredados en `Models/Program.cs` y `Models/DbContext.cs`; resolver advertencias no bloqueantes. | Mantener la limpieza técnica pendiente documentada y avanzar con las prioridades funcionales. |
| 1 | Modelos | Alta | Hecho | Existen `Usuario`, `Consorcio`, `UnidadFuncional`, `Gasto`, `Expensa`, `Pago`, `Comunicado`, `Reclamo`, `Amenity` y `Reserva` en `Models`. | Agregar o ajustar validaciones de dominio y atributos de formulario donde corresponda. | Mantener los modelos actuales y agregar validaciones al avanzar en cada módulo. |
| 2 | DbContext | Alta | Hecho | `Context/GestionDeConsorciosContext.cs` tiene `DbSet` para las entidades principales, relaciones, precisión decimal e índice único para `Gasto.NumeroFactura`. | Revisar el TODO interno sobre nombres/restricciones y limpiar contextos heredados si el equipo lo confirma. | Usar siempre `GestionDeConsorciosMVC.Context.GestionDeConsorciosContext` como contexto válido. |
| 3 | Migraciones | Alta | Parcial | `Migrations/20260606022518_InicialGestionConsorcios.cs` crea las tablas actuales; `__EFMigrationsHistory` contiene `20260606022518_InicialGestionConsorcios`; `dotnet ef migrations list --context GestionDeConsorciosContext` funciona con `GESTION_CONSORCIOS_CONNECTION_STRING`; existe una migración previa vacía `20260528011107_primermigracin` marcada como `Pending`. | Decidir si conservar, aplicar o retirar la migración vacía; mantener documentada la forma de pasar la connection string sin trackear `appsettings.json`. | Resolver la migración vacía cuando el equipo decida limpieza de migraciones. |
| 4 | Consorcios | Prioridad 1 | Hecho | `Controllers/ConsorciosController.cs` lista, muestra detalle con `ConsorcioDetailsViewModel`, crea con `ConsorcioViewModel` y edita datos generales; vistas `Index`, `Create`, `Details` y `Edit`; build en `0 errores`. | Queda para una etapa posterior proteger por rol/autenticación real y decidir baja lógica. | Usar el módulo como base para probar gastos y expensas por consorcio. |
| 5 | Unidades funcionales | Prioridad 1 | Hecho | `UnidadFuncionalViewModel`; alta de UFs dentro de `POST Consorcios/Create`; edición de UFs existentes desde `POST Consorcios/Edit`; validaciones de UF requerida, DNI requerido, email válido y duplicados por consorcio. | Queda para una etapa posterior asociar UF con `Usuario` propietario real y evaluar un índice único compuesto si se decide tocar migraciones. | Continuar con módulos dependientes: gastos, expensas y pagos vinculados a consorcios/UF. |
| 6 | Gastos | Prioridad 2 | Parcial | Admin reforzado: `GastosIndexViewModel`, filtros reales por consorcio/mes/año/categoría/búsqueda, alta con factura, edición, eliminación, detalle completo, `GastoService`, `FileStorageService`, índice único `NumeroFactura`; build en `0 errores`. | Falta trabajar `MisGastos` propietario con datos reales y definir autorización por rol; delete sigue siendo físico porque no se tocaron migraciones para baja lógica. | Próxima tanda: cerrar `MisGastos` propietario o avanzar a Expensas según prioridad del TP. |
| 7 | Expensas | Prioridad 3 y 5 | Parcial | `Controllers/ExpensasController.cs` tiene `Index`, `MisExpensas`, `Details`, `Generar` GET/POST; genera expensas desde gastos y evita duplicados por período. | Falta servicio dedicado; criterios de distribución más realistas; PDF/liquidación; autorización real; pruebas de generación. | Extraer la lógica de generación a un servicio y cubrir validaciones críticas. |
| 8 | Pagos | Prioridad 4 | Parcial | `Controllers/PagosController.cs` tiene `InformarPago`, `MisPagos` y `Details`; guarda comprobantes en `wwwroot/uploads/pagos`; `Models/Pago.cs` contempla estado y revisión. | Falta revisión administrativa aprobar/rechazar; actualizar estado de expensa al aprobar; ViewModels dedicados; autorización por rol. | Implementar flujo admin de revisión de pagos. |
| 9 | Comunicados | Prioridad 7 y 8 | Parcial | `Models/Comunicado.cs`, `DbSet<Comunicado>`, vistas y `Controllers/ComunicadosController.cs`. | El controlador todavía es principalmente visual/mock; falta POST real, persistencia, adjuntos y filtros por consorcio/propietario. | Convertir `ComunicadosController` a datos reales con ViewModels. |
| 10 | Reclamos | Prioridad 9 | Pendiente | `Models/Reclamo.cs`, `DbSet<Reclamo>` y tabla prevista en migración inicial. | Faltan controlador, vistas, carga de reclamos, seguimiento y gestión administrativa. | Crear `ReclamosController` con flujos propietario/admin. |
| 11 | Reservas | Prioridad 10 | Pendiente | `Models/Amenity.cs`, `Models/Reserva.cs`, `DbSet` y tablas previstas en migración inicial. | Faltan controladores, vistas y reglas de negocio: deuda, superposición horaria y límite mensual. | Definir reglas exactas de reserva antes de implementar. |
| 12 | Login/autenticación | Alta | Parcial | `Controllers/AuthController.cs`, `Models/Usuario.cs`; `Program.cs` habilita sesión con `AddSession()` y `UseSession()`. | Falta autenticación real con password hash, cookies/claims, `[Authorize]`, roles y protección de rutas. | Definir estrategia de autenticación y reemplazar login parcial por flujo real. |
| 13 | Datos de prueba | Media | Parcial | `Data/DevDataSeeder.cs` existe y se invoca en desarrollo desde `Program.cs`; hay comprobantes seed en `wwwroot/uploads/pagos`. | Ampliar seed para consorcios, UFs, gastos, expensas, pagos y comunicados; asegurar que sea idempotente. | Ajustar seed después de cerrar configuración de EF y servicios. |
| 14 | Testing completo | Alta antes de entrega | Pendiente | Verificación actual manual: `dotnet build GestionDeConsorciosMVC.sln` en `0 errores`. | Falta proyecto de tests, pruebas de servicios, pruebas de controladores y recorrido manual completo. | Crear checklist de prueba manual y luego agregar tests unitarios para servicios críticos. |

## Funcionando o encaminado especialmente

- SQL Server local: base `GestionConsorciosDataBase` confirmada `ONLINE`.
- Migración inicial limpia: `20260606022518_InicialGestionConsorcios` aplicada en `__EFMigrationsHistory`.
- DbContext: `GestionDeConsorciosContext` existe y está conectado al modelo actual.
- Consorcios + Unidades Funcionales: Prioridad 1 cerrada con ViewModels, alta, detalle, edición y validaciones backend.
- Backend de gastos: lado administrador reforzado con filtros reales, resumen, alta, detalle, edición, eliminación, almacenamiento de archivos y registro DI en `Program.cs`.
- Servicios y ViewModels: existen y están registrados para gastos (`GastoVM`, `IGastoService`, `GastoService`, `IFileStorageService`, `FileStorageService`).
- Visual Studio con `.sln`: `GestionDeConsorciosMVC.sln` y `GestionDeConsorciosMVC.slnLaunch` existen; falta revisar/restaurar `launchSettings.json` si los perfiles no aparecen.

## Bloqueos o decisiones humanas

- Decidir cómo documentar la configuración local: por ahora EF CLI funciona con la variable temporal `GESTION_CONSORCIOS_CONNECTION_STRING`.
- Decidir si se limpian o excluyen archivos heredados `Models/Program.cs` y `Models/DbContext.cs`.
- Definir estrategia de autenticación: sesión simple para TP, cookies/claims o ASP.NET Identity.
- Definir si las expensas se reparten en partes iguales o por porcentaje de participación de UF.
- Definir reglas de reservas: deuda permitida, límite mensual y superposición de horarios.
- Definir si los pagos aprobados cambian automáticamente la expensa a `Pagada`.

## Próximo foco recomendado

Consorcios + Unidades Funcionales quedó cerrado como Prioridad 1 del backend: alta, detalle, edición, ViewModels y validaciones principales.

El próximo foco lógico es completar `MisGastos` para propietario o avanzar sobre `Expensas`, dejando autorización real para una etapa posterior.
