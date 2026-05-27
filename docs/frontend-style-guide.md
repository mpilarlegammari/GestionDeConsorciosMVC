# Guia Visual Frontend - Sistema de Gestion de Consorcios

Esta guia documenta los patrones visuales reutilizables del frontend para mantener coherencia entre pantallas nuevas y existentes.

El objetivo es que cualquier nueva vista Razor respete la misma estructura, jerarquia visual y comportamiento responsive sin duplicar estilos.

## 1. Principios de diseno

- Mantener estetica administrativa moderna, clara y funcional.
- Priorizar lectura rapida de informacion: titulos claros, tablas limpias, filtros visibles y acciones directas.
- Usar fondo general claro y superficies blancas para contenido.
- Reservar el azul para acciones principales, navegacion activa y elementos destacados.
- Evitar agregar estilos aislados por pantalla si existe un patron reutilizable.
- Mantener diferencias visuales por rol sin crear dos sistemas distintos.
- No mostrar funcionalidades backend como reales si siguen siendo mock.

## 2. Estructura general de pantalla

Todas las pantallas internas deben usar el layout compartido:

- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_Sidebar.cshtml`
- `Views/Shared/_Header.cshtml`

Estructura recomendada dentro de una vista:

```cshtml
@{
    ViewData["Title"] = "Titulo de pantalla";
    ViewData["Role"] = "Administrador";
}

<section class="page-intro">
    <div>
        <h2>Titulo de pantalla</h2>
        <p>Descripcion breve de la tarea o consulta.</p>
    </div>
    <a class="btn btn-primary" href="/Ruta/Create">Accion principal</a>
</section>

<section class="app-card filter-card">
    ...
</section>

<section class="app-card section-card">
    ...
</section>
```

## 3. Layout

### Sidebar

Usar el sidebar como navegacion principal del sistema.

Patrones:

- Fondo azul oscuro.
- Iconos en cada item.
- Estado activo con fondo azul y marcador lateral.
- Tarjeta de rol al pie.
- Renderizado segun `ViewData["Role"]` mientras la autenticacion real no exista.

Criterio:

- Admin apunta a pantallas de gestion.
- Propietario apunta a pantallas de consulta o carga propia.
- No mezclar rutas administrativas en el menu propietario.

### Header

El header muestra:

- Nombre del sistema.
- Titulo actual desde `ViewData["Title"]`.
- Rol actual y usuario mock en desktop.
- Boton hamburguesa en mobile.

Criterio:

- No agregar acciones complejas en el header.
- Las acciones principales viven en `page-intro`.

## 4. Encabezados de pagina

Usar siempre `.page-intro`.

Contenido:

- `h2`: nombre de la pantalla.
- `p`: explicacion corta.
- Accion principal, badge o grupo de botones.

Ejemplo:

```cshtml
<section class="page-intro">
    <div>
        <h2>Gastos</h2>
        <p>Registro y consulta de gastos del consorcio.</p>
    </div>
    <a class="btn btn-primary" href="/Gastos/Create">Registrar gasto</a>
</section>
```

Criterios:

- Un solo boton principal por encabezado cuando sea posible.
- Texto breve, no explicar instrucciones largas.
- En mobile se apila automaticamente.

## 5. Cards

Usar `.app-card` para bloques de contenido.

Variantes habituales:

- `.section-card`: separa secciones despues del primer bloque.
- `.filter-card`: filtros visuales.
- `.metric-card`: metricas resumidas.
- `.expense-card`: resumen destacado de expensa, gasto o informacion principal.

Ejemplo:

```cshtml
<section class="app-card section-card">
    <div class="app-card-title">
        <h3>Listado</h3>
        <span class="status-badge">Datos mock</span>
    </div>
    ...
</section>
```

Criterios:

- No anidar cards dentro de cards.
- Usar cards para grupos reales de informacion, formularios, tablas o detalles.
- Mantener titulos internos chicos y funcionales.

## 6. Grids

Clases disponibles:

- `.dashboard-grid`: grilla de metricas.
- `.content-grid`: layout de contenido principal + aside.
- `.form-grid`: grilla de formulario.
- `.quick-actions`: accesos rapidos.

Criterios:

- En desktop, usar grillas para organizar informacion.
- En mobile, todas pasan a una columna.
- No fijar anchos manuales por pantalla.

## 7. Botones

Usar Bootstrap con estilos del sistema.

Tipos:

- `.btn.btn-primary`: accion principal.
- `.btn.btn-outline-primary`: accion secundaria importante.
- `.btn.btn-outline-secondary`: cancelar, volver o accion neutral.
- `.btn.btn-outline-danger`: accion destructiva visual.

Criterios:

- Texto breve y concreto.
- No usar mas de una accion primaria por bloque.
- En mobile, los botones de grupos se apilan full-width.

Ejemplo:

```cshtml
<div class="button-group">
    <a class="btn btn-outline-secondary" href="/Gastos">Volver</a>
    <button class="btn btn-primary" type="submit">Registrar gasto mock</button>
</div>
```

## 8. Tablas

Usar siempre:

```cshtml
<div class="table-responsive">
    <table class="table table-clean">
        ...
    </table>
</div>
```

Acciones dentro de tablas:

```cshtml
<div class="table-actions">
    <a href="/Entidad/Details/1">Ver detalle</a>
    <a href="/Entidad/Edit/1">Editar</a>
    <button type="button" data-mock-confirm>Eliminar</button>
</div>
```

Criterios:

- Columnas claras y cortas.
- Acciones al final.
- Usar badges para estados.
- En mobile, la tabla mantiene scroll horizontal y muestra ayuda visual.
- No convertir tablas complejas en cards si la informacion es tabular.

## 9. Formularios

Usar:

- `.consorcio-form` para formularios extensos.
- `.form-grid` para campos.
- `.form-label` para labels.
- `.form-summary` o `.form-alert` para mensajes inline.
- `.form-actions` para botones finales.

Ejemplo:

```cshtml
<form asp-action="Create" method="post" class="consorcio-form" novalidate>
    <section class="app-card">
        <div class="app-card-title">
            <h3>Datos generales</h3>
        </div>
        <div class="form-grid">
            ...
        </div>
    </section>

    <div class="form-actions">
        <a class="btn btn-outline-secondary" href="/Entidad">Cancelar</a>
        <button class="btn btn-primary" type="submit">Guardar mock</button>
    </div>
</form>
```

Criterios:

- Agrupar campos por seccion.
- No poner demasiados campos sin titulo intermedio.
- Mantener `name` compatibles con futuro Model Binding.
- Agregar `TODO backend` cuando el formulario quede preparado para integracion real.
- No implementar POST real si la pantalla sigue siendo frontend-only.

## 10. Archivos y comprobantes

Para carga de archivos usar:

- `.file-drop`
- `.file-preview`

Para visualizacion de adjuntos usar el modal reutilizable con:

- `data-mock-attachment`
- `data-attachment-title`
- `data-attachment-file`
- `data-attachment-description`

Ejemplo:

```cshtml
<a href="#"
   data-mock-attachment
   data-attachment-title="Comprobante de pago"
   data-attachment-file="comprobante-mayo-2026.pdf"
   data-attachment-description="Comprobante mock del pago informado.">
    Ver comprobante
</a>
```

## 11. Badges y estados

Usar `.status-badge` para estados, categorias y etiquetas compactas.

Variantes:

- `.status-badge`: informacion general.
- `.status-badge.success`: aprobado, pagado, activo.
- `.status-badge.warning`: pendiente, importante, revision.
- `.status-badge.danger`: vencido, rechazado, error.
- `.status-badge.neutral`: inactivo, normal, archivado.
- `.status-badge.info`: informacion.
- `.status-badge.subtle`: dato secundario.

Criterios:

- No inventar colores por pantalla.
- Mantener los textos cortos.
- Usar el mismo estado con la misma variante en todo el sistema.

## 12. Modales mock

Partial global:

- `Views/Shared/_MockModals.cshtml`

Confirmacion:

- `data-mock-confirm`
- `data-confirm-title`
- `data-confirm-message`
- `data-confirm-action`

Ejemplo:

```cshtml
<button type="button"
        data-mock-confirm
        data-confirm-title="Eliminar gasto"
        data-confirm-message="Vas a eliminar esta factura de forma visual."
        data-confirm-action="Eliminar mock">
    Eliminar
</button>
```

Criterios:

- Usar para acciones destructivas o sensibles.
- Indicar claramente que no modifica datos reales.
- Backend debera reemplazarlo por acciones reales con permisos.

## 13. Toasts y feedback

Partial global:

- `Views/Shared/_MockFeedback.cshtml`

Funcion JS:

- `mostrarFeedbackMock({ title, message, variant, kicker })`

Uso declarativo:

- `data-mock-toast`
- `data-toast-title`
- `data-toast-message`
- `data-toast-variant`
- `data-toast-kicker`

Ejemplo:

```cshtml
<button type="button"
        data-mock-toast
        data-toast-title="Borrador mock"
        data-toast-message="El borrador visual quedaria pendiente de guardado real."
        data-toast-variant="success">
    Guardar borrador mock
</button>
```

Criterios:

- Usar toast para feedback breve global.
- Usar errores inline para campos especificos.
- No reemplazar validaciones de formulario por toast solamente.

## 14. Estados vacios

Partial:

- `Views/Shared/_EmptyState.cshtml`

Uso sugerido:

```cshtml
@{
    ViewData["EmptyTitle"] = "Sin pagos informados";
    ViewData["EmptyMessage"] = "No hay pagos para el periodo seleccionado.";
    ViewData["EmptyActionText"] = "Informar pago";
    ViewData["EmptyActionUrl"] = "/Pagos/InformarPago";
}

@await Html.PartialAsync("_EmptyState")
```

Criterios:

- Usar cuando un listado real llegue vacio.
- Diferenciar falta total de datos de filtros sin resultado.
- Mostrar accion solo si aporta al usuario.

## 15. Responsive

Breakpoints principales:

- Desktop: layout con sidebar fijo.
- `max-width: 1100px`: grillas principales se simplifican.
- `max-width: 768px`: sidebar colapsado, contenido en una columna, botones full-width.
- `max-width: 480px`: ajustes finos para telefonos chicos.

Criterios:

- No crear reglas mobile por vista salvo que sea inevitable.
- Usar las clases globales antes de inventar nuevas.
- Las tablas complejas usan scroll horizontal.
- Los formularios pasan a una columna.
- Los modales y toasts ocupan ancho seguro en mobile.

## 16. Datos mock y notas backend

Mientras el proyecto siga frontend-only:

- Mantener datos mock dentro de vistas.
- Agregar comentarios `TODO backend` donde corresponda.
- Documentar necesidades reales en `docs/backend-notes.md`.
- No crear persistencia, autenticacion real ni logica de base de datos desde el frontend.

## 17. Checklist para nuevas pantallas

Antes de terminar una nueva vista:

- Tiene `ViewData["Title"]`.
- Tiene `ViewData["Role"]` si depende del rol.
- Usa `page-intro`.
- Usa `app-card` para bloques principales.
- Usa `form-grid` si tiene formulario.
- Usa `table-responsive` + `table-clean` si tiene tabla.
- Usa `status-badge` para estados.
- Usa modales/toasts reutilizables si hay acciones mock.
- Es legible en mobile.
- No duplica CSS innecesario.
- Tiene notas frontend/backend si introduce un nuevo modulo o expectativa futura.
