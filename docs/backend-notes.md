# Notas para Backend - Sistema de Gestión de Consorcios

## 1. Objetivo del backend

El backend del Sistema de Gestión de Consorcios deberá proveer la capa funcional completa para que el frontend Razor pueda dejar de trabajar con datos mock y comenzar a operar con información real. Más adelante deberá implementar modelos de dominio, controladores MVC, validaciones de entrada, reglas de negocio, persistencia en base de datos y ViewModels específicos para cada pantalla.

La idea es que el frontend quede preparado para recibir datos desde controladores y ViewModels sin depender de lógica de base de datos dentro de las vistas. El backend deberá encargarse de obtener, validar, guardar y actualizar la información relacionada con consorcios, unidades funcionales, usuarios, expensas, pagos, reclamos, comunicados, amenities y reservas.

También deberá definir el flujo de autenticación y autorización. En esta primera etapa no existe login real ni validación de credenciales, pero el sistema visual ya contempla dos perfiles principales: Administrador y Propietario.

## 2. Entidades principales esperadas

### Usuario

Entidad base para representar a cualquier persona que pueda acceder al sistema. Debería contener datos comunes como identificador, nombre, apellido, email, teléfono, documento, contraseña hasheada, estado activo/inactivo y rol asociado. Será la base para distinguir permisos y navegación.

### Administrador

Representa al usuario encargado de gestionar uno o más consorcios. Puede administrar unidades funcionales, gastos, expensas, comunicados, reclamos, reservas y configuración general. Puede modelarse como entidad propia relacionada con Usuario o como un rol dentro de Usuario, según la arquitectura elegida.

### Propietario

Representa al dueño o responsable de una unidad funcional. Deberá poder consultar sus expensas, pagos, gastos del consorcio, comunicados, reclamos y reservas. Puede tener una o más unidades funcionales asociadas.

### Consorcio

Representa un edificio, complejo o conjunto administrado. Debería incluir nombre, dirección, CUIT si corresponde, datos de contacto, estado, administrador responsable y colección de unidades funcionales. Será una entidad central para filtrar gastos, expensas, comunicados, amenities y reservas.

### UnidadFuncional

Representa cada departamento, local, cochera u otra unidad dentro de un consorcio. Debería incluir número o código, piso, descripción, porcentaje de participación, propietario asociado, estado y consorcio al que pertenece. Es clave para calcular y asignar expensas.

### Gasto

Representa un gasto del consorcio. Debería incluir descripción, categoría, monto, fecha, comprobante opcional, período, consorcio asociado y estado. Más adelante podrá usarse para generar expensas.

### Expensa

Representa el importe que debe pagar una unidad funcional para un período determinado. Debería incluir período, unidad funcional, monto total, fecha de vencimiento, estado de pago, detalle de conceptos y relación con pagos. Puede generarse a partir de gastos y porcentajes de participación.

### Pago

Representa un pago realizado o informado por un propietario. Debería incluir expensa asociada, propietario, monto, fecha de pago, medio de pago, comprobante, estado de validación y observaciones. El administrador debería poder validar o rechazar pagos informados.

### Comunicado

Representa una comunicación enviada por la administración a propietarios o a todo un consorcio. Debería incluir título, contenido, fecha de publicación, autor, destinatarios, consorcio asociado y estado de visibilidad.

### Reclamo

Representa un reclamo cargado por un propietario o gestionado por administración. Debería incluir asunto, descripción, fecha de creación, estado, prioridad, propietario, unidad funcional, consorcio, comentarios o historial de seguimiento.

### Amenity

Representa un espacio común reservable, como SUM, parrilla, pileta o salón. Debería incluir nombre, descripción, capacidad, reglas de uso, disponibilidad, consorcio asociado y estado activo/inactivo.

### Reserva

Representa la reserva de un amenity por parte de un propietario. Debería incluir amenity, propietario, unidad funcional, fecha, horario de inicio, horario de fin, estado, observaciones y reglas de validación para evitar superposición de turnos.

## 3. Controladores esperados

- `AuthController`: login, logout y futura autenticación de usuarios. En frontend ya existe una acción mock para visualizar `/Auth/Login`, pero todavía no valida credenciales ni crea sesión real.
- `HomeController`: dashboards principales para administrador y propietario.
- `ConsorciosController`: listado, alta, edición, detalle y administración de consorcios.
- `UnidadesFuncionalesController`: gestión de unidades funcionales asociadas a consorcios.
- `GastosController`: listado, carga y administración de gastos.
- `ExpensasController`: consulta, generación y detalle de expensas.
- `PagosController`: consulta de pagos, registro o informe de pagos y validación administrativa.
- `ComunicadosController`: publicación, listado y consulta de comunicados.
- `ReclamosController`: creación, seguimiento y gestión de reclamos.
- `ReservasController`: creación, consulta y administración de reservas.
- `AmenitiesController`: administración de espacios comunes reservables.

## 4. Rutas/vistas que frontend va a necesitar

El frontend espera, como mínimo, las siguientes rutas para construir las pantallas principales del sistema:

- `/Auth/Login`
- `/Home/AdminDashboard`
- `/Home/PropietarioDashboard`
- `/Consorcios`
- `/Consorcios/Create`
- `/Consorcios/Details/{id}`
- `/Gastos`
- `/Gastos/Create`
- `/Expensas`
- `/Expensas/MisExpensas`
- `/Expensas/Generar`
- `/Pagos/MisPagos`
- `/Pagos/InformarPago`
- `/Comunicados`
- `/Comunicados/Create`
- `/Comunicados/MisComunicados`
- `/Reclamos`
- `/Reclamos/Create`
- `/Reservas`
- `/Reservas/Create`

Estas rutas todavía no necesitan estar implementadas en esta primera etapa, salvo que se quiera crear controladores o acciones mínimas más adelante para visualizar pantallas frontend específicas.

## 5. Decisiones tomadas desde frontend

- Por ahora se trabaja con datos mock/hardcodeados únicamente para visualizar el diseño.
- El login todavía no valida credenciales.
- La pantalla `/Auth/Login` existe como vista mock. El formulario permite elegir rol visual, pero el backend futuro deberá reemplazar este comportamiento por autenticación real.
- El menú se puede adaptar por rol más adelante desde sesión, claims, ViewData, ViewModel o un servicio de usuario actual.
- El diseño queda preparado para conectar con ViewModels del backend.
- El layout principal ya contempla sidebar lateral, header superior y área principal de contenido.
- La navegación incluye opciones para Administrador y Propietario, aunque por ahora se muestra una vista mock.
- La base visual fue ajustada tomando como referencia el diseño publicado en Figma, implementada manualmente en Razor y CSS reutilizable, sin exportaciones automáticas.
- No se agregó lógica de base de datos nueva desde frontend.
- No se crearon migraciones ni Entity Framework nuevo en esta etapa.
- Se creó un `AuthController` mínimo únicamente para poder visualizar la pantalla mock de login.

## 6. Notas específicas para AuthController

Más adelante `AuthController` deberá reemplazar el flujo mock por autenticación real. Como mínimo debería contemplar:

- Acción `GET /Auth/Login` para mostrar el formulario de ingreso.
- Acción `POST /Auth/Login` para recibir credenciales, validar modelo y autenticar.
- Validación de email y contraseña del lado servidor.
- Búsqueda de usuario activo en la base de datos.
- Verificación de contraseña usando hash seguro, no texto plano.
- Creación de sesión, cookie de autenticación o mecanismo equivalente de ASP.NET Core.
- Carga del rol del usuario autenticado: Administrador o Propietario.
- Redirección según rol: administrador hacia `/Home/AdminDashboard` y propietario hacia `/Home/PropietarioDashboard`.
- Acción `POST /Auth/Logout` o `GET /Auth/Logout`, según la decisión del equipo, para cerrar sesión.
- Manejo de mensajes de error sin revelar si falló el email o la contraseña.
- Protección de rutas internas con autorización por rol.
- ViewModel sugerido `LoginViewModel` con `Email`, `Password`, `Role` solo si el rol se mantiene para testing, `RememberMe` si se decide soportarlo y campos de validación.
- El frontend espera poder mostrar errores de validación cerca del formulario cuando backend implemente reglas reales.

## 7. Próximos pasos sugeridos para backend

- Crear modelos definitivos de dominio.
- Crear o ajustar el `DbContext` principal.
- Crear migración inicial cuando el modelo esté acordado.
- Crear datos semilla para poder probar el sistema con consorcios, propietarios, unidades, gastos y expensas de ejemplo.
- Crear controladores MVC para las rutas esperadas.
- Crear ViewModels por pantalla para evitar exponer entidades directamente en las vistas.
- Implementar validaciones de formularios del lado servidor.
- Implementar autenticación y autorización por rol.
- Reemplazar los datos mock del frontend por datos reales provenientes de controladores y ViewModels.
- Definir reglas de negocio para generación de expensas, imputación de pagos, reservas de amenities y estados de reclamos.

# Requerimientos Backend - Dashboards

## Objetivo

El frontend ya cuenta con dos dashboards visuales mock:

- `/Home/AdminDashboard`
- `/Home/PropietarioDashboard`

Por ahora estas vistas solo muestran datos hardcodeados. Backend debera reemplazar esos datos por ViewModels reales cuando implemente autenticacion, roles y persistencia.

## Dashboard Administrador

El backend debera proveer un ViewModel para administradores con, como minimo:

- Cantidad de consorcios activos.
- Cantidad de expensas emitidas por periodo.
- Cantidad de pagos pendientes de validacion.
- Cantidad de reclamos abiertos.
- Actividad administrativa reciente.
- Estado general de cobranzas, morosidad, comunicados y amenities.

Los datos deberan estar filtrados por el administrador autenticado si el sistema permite que un administrador gestione solo ciertos consorcios.

## Dashboard Propietario

El backend debera proveer un ViewModel para propietarios con, como minimo:

- Expensa actual de la unidad funcional.
- Fecha de vencimiento.
- Pagos informados o validados.
- Comunicados no leidos.
- Reservas activas.
- Movimientos recientes.
- Datos de la unidad funcional: consorcio, unidad, porcentaje de participacion y estado.

Los datos deberan estar filtrados por el propietario autenticado y por sus unidades funcionales asociadas.

## Controladores y acciones

Se espera que `HomeController` mantenga:

- `GET /Home/AdminDashboard`
- `GET /Home/PropietarioDashboard`

Actualmente estas acciones existen solo para renderizar frontend mock. Mas adelante deberan:

- Verificar que el usuario este autenticado.
- Verificar que el rol coincida con la vista solicitada.
- Construir ViewModels con datos reales.
- Redirigir o bloquear acceso cuando el rol no corresponda.

## Vistas que deberan protegerse

Cuando exista autenticacion real, deberan protegerse:

- `/Home/AdminDashboard`
- `/Home/PropietarioDashboard`
- Todas las vistas bajo `/Consorcios`
- Todas las vistas bajo `/Gastos`
- Todas las vistas bajo `/Expensas`
- Todas las vistas bajo `/Pagos`
- Todas las vistas bajo `/Comunicados`
- Todas las vistas bajo `/Reclamos`
- Todas las vistas bajo `/Reservas`
- Todas las vistas bajo `/Amenities`

Las vistas administrativas deberan permitir acceso solo a Administrador. Las vistas de propietario deberan permitir acceso solo a Propietario o a usuarios con permisos equivalentes definidos por backend.

## Datos requeridos segun frontend actual

### Dashboard Administrador

El frontend necesitara recibir:

- Cantidad de consorcios.
- Cantidad de unidades funcionales.
- Cantidad de expensas pendientes.
- Cantidad de reclamos abiertos.
- Actividad reciente.
- Consorcios recientes.

### Dashboard Propietario

El frontend necesitara recibir:

- Expensa actual.
- Pagos informados.
- Comunicados nuevos.
- Reclamos abiertos.
- Reservas proximas.
- Ultimos comunicados.
- Ultimos pagos.

## Endpoints o acciones sugeridas

- `GET /Home/AdminDashboard`
- `GET /Home/PropietarioDashboard`

Alternativamente, backend puede mantener estas acciones MVC y delegar la composicion de datos a servicios internos que armen ViewModels especificos para cada rol.

## ViewModels sugeridos

- `AdminDashboardViewModel`
- `PropietarioDashboardViewModel`
- `DashboardMetricViewModel`
- `ActividadRecienteViewModel`

`AdminDashboardViewModel` podria incluir colecciones para metricas, actividad reciente y consorcios recientes. `PropietarioDashboardViewModel` podria incluir la expensa actual, resumen de indicadores, ultimos comunicados y ultimos pagos.

# Requerimientos Backend - Consorcios

## 1. Modelos esperados

- `Consorcio`
- `UnidadFuncional`

## 2. Relaciones esperadas

- Un `Consorcio` tiene muchas `UnidadFuncional`.
- Cada `UnidadFuncional` pertenece a un unico `Consorcio`.
- Una unidad funcional debera poder asociarse luego a un propietario o usuario propietario.

## 3. Validaciones necesarias

- No repetir numero de UF dentro del mismo consorcio.
- Validar formato de email del propietario.
- DNI propietario requerido.
- Numero UF requerido.
- Nombre del consorcio obligatorio.
- Direccion del consorcio obligatoria.
- CUIT requerido o validado segun criterio del equipo.
- Cantidad de pisos valida.

## 4. Acciones/controladores esperados

- `GET /Consorcios`
- `GET /Consorcios/Create`
- `POST /Consorcios/Create`
- `GET /Consorcios/Details/{id}`
- `GET /Consorcios/Edit/{id}`
- `POST /Consorcios/Edit`

Actualmente solo existen acciones GET mock para visualizar frontend. Backend debera implementar POST, persistencia, validaciones y manejo de errores.

## 5. ViewModels sugeridos

- `ConsorcioViewModel`
- `UnidadFuncionalViewModel`
- `ConsorcioDetailsViewModel`

`ConsorcioViewModel` podria contener datos generales del consorcio y una coleccion de `UnidadFuncionalViewModel`. `ConsorcioDetailsViewModel` podria incluir datos generales, metricas resumen, listado UF y ultimos movimientos.

## 6. Que espera frontend

- Listado de consorcios.
- Detalle de consorcio por id.
- Listado de unidades funcionales.
- Metricas resumen del consorcio.
- Estados de consorcio y UF.
- Errores de validacion por campo.
- Mensajes de exito o error para altas y ediciones.

# Requerimientos Backend - Gastos

## 1. Modelo esperado

### Gasto

Campos minimos:

- `Id`
- `ConsorcioId`
- `NumeroFactura`
- `Fecha`
- `Monto`
- `Concepto`
- `Categoria`
- `Descripcion`
- `ArchivoFacturaPath`
- `FechaCreacion`

## 2. Relacion

- Un `Consorcio` tiene muchos `Gasto`.
- Cada `Gasto` pertenece a un unico `Consorcio`.

## 3. Validaciones backend

- Consorcio obligatorio.
- Numero de factura obligatorio.
- No repetir numero de factura.
- Monto mayor a 0.
- Fecha valida.
- Categoria obligatoria.
- Validar tipo y tamaño de archivo.
- Validar que el archivo se guarde en una ubicacion segura si se implementa upload.

## 4. Acciones esperadas

- `GET /Gastos`
- `GET /Gastos/Create`
- `POST /Gastos/Create`
- `GET /Gastos/Details/{id}`
- `GET /Gastos/Edit/{id}`
- `POST /Gastos/Edit`
- `POST /Gastos/Delete/{id}`

Actualmente solo existen acciones GET mock para visualizar frontend. Backend debera implementar POST, persistencia, validaciones y manejo de archivos.

## 5. ViewModels sugeridos

- `GastoViewModel`
- `GastoCreateViewModel`
- `GastoDetailsViewModel`

`GastoCreateViewModel` deberia contemplar archivo adjunto mediante `IFormFile`. `GastoDetailsViewModel` podria incluir datos del consorcio, datos del gasto, estado del comprobante e historial.

## 6. Que espera frontend

- Listado de gastos.
- Filtros por consorcio, mes, año, categoria y busqueda por factura/concepto.
- Resumen mensual.
- Detalle de gasto.
- Carga de archivo.
- Errores de validacion por campo.
- Mensajes de exito o error para altas y ediciones.

# Requerimientos Backend - Gastos Propietario

## 1. Accion esperada

- `GET /Gastos/MisGastos`

Esta accion debera renderizar la pantalla de consulta de gastos para usuarios con rol Propietario.

## 2. ViewModel sugerido

- `MisGastosViewModel`

Podria contener datos de cabecera, filtros seleccionados, listado de gastos, resumen por categoria y notas/aclaraciones.

## 3. Datos esperados

- `NombreConsorcio`
- `UnidadFuncional`
- `PeriodoSeleccionado`
- `TotalGastosPeriodo`
- `ParticipacionEstimada`
- Lista de gastos
- Resumen por categoria
- Notas o aclaraciones

Cada gasto mostrado al propietario deberia incluir fecha, concepto, categoria, numero de factura, monto y datos del comprobante si existe.

## 4. Seguridad esperada

- Solo usuarios con rol Propietario pueden acceder a `GET /Gastos/MisGastos`.
- El propietario solo puede ver gastos de su propio consorcio.
- El propietario no puede crear, editar ni eliminar gastos desde esta pantalla.
- Las acciones administrativas de gastos deben quedar restringidas al rol Administrador.

## 5. Filtros backend

- Mes
- Año
- Categoria
- Busqueda por concepto o factura

Los filtros deberian aplicarse sobre los gastos del consorcio asociado al propietario autenticado.

## 6. Regla de negocio

- La pantalla es solo consulta.
- Los gastos los carga la administracion.
- El propietario solo visualiza informacion filtrada por su consorcio.
- La participacion estimada debe calcularse segun el criterio de distribucion definido para el consorcio o la unidad funcional.

# Requerimientos Backend - Expensas Propietario

## 1. Acciones esperadas

- `GET /Expensas/MisExpensas`
- `GET /Expensas/Details/{id}`

Estas acciones deberan cargar informacion real del propietario autenticado y sus expensas asociadas.

## 2. ViewModels sugeridos

- `MisExpensasViewModel`
- `ExpensaDetailsViewModel`

`MisExpensasViewModel` podria incluir la expensa actual, filtros seleccionados, historial de expensas y resumen anual. `ExpensaDetailsViewModel` podria incluir datos generales, detalle de gastos, estado de pago y datos para descarga de liquidacion.

## 3. Datos esperados

- Periodo.
- Monto.
- Estado.
- Vencimiento.
- Fecha de emision.
- Detalle de gastos.
- Pagos asociados.
- Liquidacion PDF.

Tambien se espera identificar consorcio, unidad funcional y criterio de distribucion aplicado.

## 4. Seguridad

- El propietario solo puede ver sus expensas.
- El acceso debe estar restringido por usuario autenticado.
- El detalle de expensa debe validar que el `id` solicitado pertenezca al propietario autenticado.
- Las funciones de generacion, edicion o eliminacion de expensas deben quedar fuera del rol Propietario.

## 5. Reglas de negocio

- El estado de la expensa debe calcularse automaticamente segun pago y vencimiento.
- El monto debe calcularse segun el criterio de distribucion definido para la unidad funcional.
- Debe existir historial de pagos asociados a cada expensa.
- La liquidacion PDF debe generarse o recuperarse desde backend.

# Requerimientos Backend - Pagos Propietario

## 1. Acciones esperadas

- `GET /Pagos/InformarPago`
- `POST /Pagos/InformarPago`
- `GET /Pagos/MisPagos`
- `GET /Pagos/Details/{id}`

El GET de informar pago debera cargar expensas pendientes del propietario. El POST debera registrar el pago informado y dejarlo pendiente de revision.

## 2. ViewModels sugeridos

- `InformarPagoViewModel`
- `MisPagosViewModel`
- `PagoDetailsViewModel`

`InformarPagoViewModel` deberia incluir expensas disponibles, datos del formulario y archivo adjunto mediante `IFormFile`. `MisPagosViewModel` deberia incluir filtros y listado de pagos. `PagoDetailsViewModel` deberia incluir datos generales, comprobante y datos de revision.

## 3. Modelo esperado

### Pago

Campos minimos:

- `Id`
- `ExpensaId`
- `FechaPago`
- `MontoPagado`
- `MedioPago`
- `NumeroOperacion`
- `BancoEntidad`
- `ComprobantePath`
- `Comentarios`
- `Estado`
- `FechaCreacion`
- `FechaRevision`
- `ObservacionAdministracion`

## 4. Seguridad

- El propietario solo puede informar pagos propios.
- El propietario solo puede ver sus pagos.
- El administrador revisa y aprueba o rechaza pagos.
- El detalle de pago debe validar que el `id` pertenezca al usuario autenticado.
- Las acciones de aprobacion, rechazo o eliminacion administrativa no deben estar disponibles para Propietario.

## 5. Reglas de negocio

- Todo pago informado queda en estado `PendienteRevision`.
- Al aprobar un pago, la expensa asociada pasa a `Pagada`.
- El comprobante es obligatorio.
- El monto pagado debe ser mayor a 0.
- Se debe validar tipo y tamaño del archivo antes de guardarlo.
- La aprobacion o rechazo debe registrar fecha de revision y observacion administrativa si corresponde.
