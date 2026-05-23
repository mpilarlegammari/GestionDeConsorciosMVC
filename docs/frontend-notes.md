# Notas Frontend - Sistema de Gestion de Consorcios

# Dashboards Frontend

## 1. Pantallas creadas

- `Views/Home/AdminDashboard.cshtml`: dashboard mock para Administrador.
- `Views/Home/PropietarioDashboard.cshtml`: dashboard mock para Propietario.

Tambien se agregaron acciones GET minimas en `HomeController` para poder visualizar estas vistas:

- `GET /Home/AdminDashboard`
- `GET /Home/PropietarioDashboard`

Estas acciones no tienen autorizacion, sesion, base de datos ni logica real.

## 2. Datos mockeados

### Panel Administrador

- Consorcios administrados: 4.
- Unidades funcionales: 86.
- Expensas pendientes: 23.
- Reclamos abiertos: 7.
- Actividad reciente con eventos administrativos simulados.
- Tabla de consorcios recientes con nombre, direccion, cantidad de UF, estado y accion.

### Panel Propietario

- Expensa actual del periodo Mayo 2026.
- Monto de expensa: `$85.000`.
- Vencimiento: `10/06/2026`.
- Estado: Pendiente.
- Pagos informados: 2.
- Comunicados nuevos: 3.
- Reclamos abiertos: 1.
- Reservas proximas: 1.
- Lista de ultimos comunicados.
- Tabla de ultimos pagos.

## 3. Componentes visuales agregados

- Cards de metricas.
- Card principal de expensa actual.
- Accesos rapidos por rol.
- Lista de actividad reciente.
- Badges de estado.
- Tabla limpia para consorcios recientes.
- Tabla limpia para ultimos pagos.
- Estilos auxiliares en `site.css`: `section-card`, `activity-list`, `activity-dot`, `table-action`, `expense-card`, `card-kicker` y `expense-summary`.

## 4. Rutas futuras enlazadas

Los botones y accesos rapidos apuntan a rutas futuras que todavia no se implementan en frontend:

- `/Consorcios`
- `/Consorcios/Create`
- `/Gastos/Create`
- `/Expensas/Generar`
- `/Comunicados/Create`
- `/Expensas/MisExpensas`
- `/Pagos/InformarPago`
- `/Reclamos/Create`
- `/Reservas/Create`

No se crearon esas vistas en esta etapa.

## 5. Falta conectar con backend

- Reemplazar datos hardcodeados por ViewModels.
- Recibir datos reales del usuario autenticado.
- Filtrar datos por rol.
- Cargar metricas reales desde servicios o controladores.
- Obtener actividad reciente desde datos persistidos.
- Obtener consorcios recientes, comunicados y pagos desde backend.
- Agregar autorizacion real cuando backend implemente autenticacion.

## Login mock relacionado

El login mock redirige visualmente a los dashboards:

- Rol Administrador: `/Home/AdminDashboard`.
- Rol Propietario: `/Home/PropietarioDashboard`.

Esta navegacion se resuelve con JavaScript en `wwwroot/js/site.js`; no autentica usuarios.

# Frontend - Consorcios

## 1. Vistas creadas

- `Views/Consorcios/Index.cshtml`: listado mock de consorcios.
- `Views/Consorcios/Create.cshtml`: formulario mock para alta de consorcio y unidades funcionales.
- `Views/Consorcios/Details.cshtml`: detalle mock de consorcio.

Tambien se creo `ConsorciosController` con acciones GET minimas:

- `GET /Consorcios`
- `GET /Consorcios/Create`
- `GET /Consorcios/Details/{id}`

Estas acciones solo renderizan vistas. No guardan, no consultan base de datos y no aplican reglas reales.

## 2. Componentes visuales agregados

- Filtros visuales de listado.
- Tabla de consorcios con badges y acciones.
- Formulario dividido en cards.
- Tabla dinamica de unidades funcionales.
- Card de detalle con metricas.
- Lista de ultimos movimientos.
- Tabla de unidades funcionales dentro del detalle.
- Botones mock hacia rutas futuras.

## 3. Datos mockeados

- Cuatro consorcios de ejemplo en el listado.
- Estados visuales: Activo, Pendiente e Inactivo.
- Datos generales del Consorcio Las Heras en detalle.
- Metricas mock del detalle: cantidad UF, expensas pendientes, gastos del mes y reclamos abiertos.
- Unidades funcionales mock en detalle.
- Ultimos movimientos mock.

## 4. Interacciones JS existentes

En `wwwroot/js/site.js` se agregaron:

- `agregarUF()`: agrega una fila visual a la tabla de unidades funcionales.
- `eliminarUF()`: elimina una fila visual, manteniendo al menos una fila.
- `validarUFDuplicadas()`: detecta numeros de UF repetidos.

La pantalla `Create` usa estas funciones solo para interaccion frontend. No persiste datos.

## 5. Validaciones frontend existentes

- Campos requeridos del consorcio.
- Numero UF requerido.
- DNI propietario requerido.
- Email de propietario con formato valido si fue cargado.
- Validacion visual de UF duplicadas.
- Mensaje visual de error o confirmacion mock.

## 6. Falta conectar con backend

- Cargar listado real de consorcios.
- Persistir nuevos consorcios.
- Persistir unidades funcionales.
- Validar reglas del lado servidor.
- Mostrar errores reales del backend.
- Cargar detalle real por id.
- Implementar edicion y eliminacion real.
- Conectar acciones de gastos, expensas y comunicados cuando existan esos modulos.
