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
