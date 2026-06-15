using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.Models;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionDeConsorciosMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public HomeController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction(nameof(PropietarioDashboard));
            }

            if (role?.Equals("Administrador", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction(nameof(AdminDashboard));
            }

            return RedirectToAction("Login", "Auth");
        }

        public async Task<IActionResult> AdminDashboard()
        {
            if (IsPropietario())
            {
                return RedirectToAction(nameof(PropietarioDashboard));
            }

            var model = new AdminDashboardViewModel
            {
                TotalConsorcios = await _context.Consorcios.CountAsync(),
                TotalUnidadesFuncionales = await _context.UnidadesFuncionales.CountAsync(),
                ExpensasPendientes = await _context.Expensas.CountAsync(expensa => expensa.Estado != EstadoExpensa.Pagada),
                MontoExpensasPendientes = await _context.Expensas
                    .Where(expensa => expensa.Estado != EstadoExpensa.Pagada)
                    .SumAsync(expensa => expensa.MontoTotal),
                PagosPendientesRevision = await _context.Pagos.CountAsync(pago => pago.Estado == EstadoPago.PendienteRevision),
                ReclamosAbiertos = await _context.Reclamos.CountAsync(reclamo => reclamo.Estado != EstadoReclamo.Cerrado),
                ComunicadosImportantes = await _context.Comunicados.CountAsync(comunicado => comunicado.Importante),
                ReservasPendientes = await _context.Reservas.CountAsync(reserva => reserva.Estado == EstadoReserva.Pendiente),
                ConsorciosRecientes = await _context.Consorcios
                    .Include(consorcio => consorcio.UnidadesFuncionales)
                    .OrderByDescending(consorcio => consorcio.FechaCreacion)
                    .ThenBy(consorcio => consorcio.Nombre)
                    .Take(5)
                    .Select(consorcio => new DashboardConsorcioItem
                    {
                        Id = consorcio.Id,
                        Nombre = consorcio.Nombre,
                        Direccion = consorcio.Direccion,
                        CantidadUnidades = consorcio.UnidadesFuncionales.Count,
                        Estado = consorcio.Estado
                    })
                    .ToListAsync()
            };

            model.ActividadReciente = await GetAdminActivityAsync();

            return View(model);
        }

        public async Task<IActionResult> PropietarioDashboard()
        {
            if (!IsPropietario())
            {
                return RedirectToAction("Login", "Auth");
            }

            var email = (HttpContext.Session.GetString("UserEmail") ?? string.Empty).Trim().ToLower();
            var unidades = string.IsNullOrWhiteSpace(email)
                ? new List<UnidadFuncional>()
                : await _context.UnidadesFuncionales
                    .Include(unidad => unidad.Consorcio)
                    .Where(unidad => unidad.MailPropietario.ToLower() == email)
                    .OrderBy(unidad => unidad.Consorcio.Nombre)
                    .ThenBy(unidad => unidad.NumeroUF)
                    .ToListAsync();

            var unidadIds = unidades.Select(unidad => unidad.Id).ToList();
            var consorcioIds = unidades.Select(unidad => unidad.ConsorcioId).Distinct().ToList();

            var expensasQuery = _context.Expensas
                .Include(expensa => expensa.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .Where(expensa => unidadIds.Contains(expensa.UnidadFuncionalId));

            var pagosQuery = _context.Pagos
                .Include(pago => pago.Expensa)
                    .ThenInclude(expensa => expensa.UnidadFuncional)
                .Where(pago => unidadIds.Contains(pago.Expensa.UnidadFuncionalId)
                    && pago.Expensa.UnidadFuncional.MailPropietario.ToLower() == email);

            var model = new PropietarioDashboardViewModel
            {
                Email = email,
                UnidadesFuncionales = unidades,
                ExpensaActual = await expensasQuery
                    .OrderByDescending(expensa => expensa.FechaEmision)
                    .ThenByDescending(expensa => expensa.FechaVencimiento)
                    .FirstOrDefaultAsync(),
                ExpensasPendientes = await expensasQuery.CountAsync(expensa => expensa.Estado != EstadoExpensa.Pagada),
                MontoPendiente = await expensasQuery
                    .Where(expensa => expensa.Estado != EstadoExpensa.Pagada)
                    .SumAsync(expensa => expensa.MontoTotal),
                PagosPendientesRevision = await pagosQuery.CountAsync(pago => pago.Estado == EstadoPago.PendienteRevision),
                ReclamosAbiertos = await _context.Reclamos
                    .CountAsync(reclamo => unidadIds.Contains(reclamo.UnidadFuncionalId) && reclamo.Estado != EstadoReclamo.Cerrado),
                ReservasProximas = await _context.Reservas
                    .CountAsync(reserva => unidadIds.Contains(reserva.UnidadFuncionalId)
                        && reserva.Estado != EstadoReserva.Cancelada
                        && reserva.FechaReserva.Date >= DateTime.Today),
                ComunicadosRecientes = await _context.Comunicados
                    .Include(comunicado => comunicado.Consorcio)
                    .Where(comunicado => consorcioIds.Contains(comunicado.ConsorcioId))
                    .OrderByDescending(comunicado => comunicado.FechaPublicacion)
                    .Take(3)
                    .ToListAsync(),
                PagosRecientes = await pagosQuery
                    .OrderByDescending(pago => pago.FechaPago)
                    .ThenByDescending(pago => pago.FechaCreacion)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        private async Task<List<DashboardActivityItem>> GetAdminActivityAsync()
        {
            var pagos = await _context.Pagos
                .Include(pago => pago.Expensa)
                    .ThenInclude(expensa => expensa.UnidadFuncional)
                .OrderByDescending(pago => pago.FechaCreacion)
                .Take(4)
                .Select(pago => new DashboardActivityItem
                {
                    Tipo = "Pago",
                    Detalle = $"Pago informado para UF {pago.Expensa.UnidadFuncional.NumeroUF}",
                    Estado = pago.Estado.ToString(),
                    Fecha = pago.FechaCreacion,
                    Tone = pago.Estado == EstadoPago.Aprobado ? "success" : pago.Estado == EstadoPago.Rechazado ? "danger" : "warning"
                })
                .ToListAsync();

            var reclamos = await _context.Reclamos
                .Include(reclamo => reclamo.UnidadFuncional)
                .OrderByDescending(reclamo => reclamo.FechaCreacion)
                .Take(4)
                .Select(reclamo => new DashboardActivityItem
                {
                    Tipo = "Reclamo",
                    Detalle = $"{reclamo.Asunto} - UF {reclamo.UnidadFuncional.NumeroUF}",
                    Estado = reclamo.Estado.ToString(),
                    Fecha = reclamo.FechaCreacion,
                    Tone = reclamo.Estado == EstadoReclamo.Cerrado ? "success" : "danger"
                })
                .ToListAsync();

            var comunicados = await _context.Comunicados
                .OrderByDescending(comunicado => comunicado.FechaPublicacion)
                .Take(4)
                .Select(comunicado => new DashboardActivityItem
                {
                    Tipo = "Comunicado",
                    Detalle = comunicado.Titulo,
                    Estado = comunicado.Importante ? "Importante" : "Publicado",
                    Fecha = comunicado.FechaPublicacion,
                    Tone = comunicado.Importante ? "warning" : string.Empty
                })
                .ToListAsync();

            return pagos
                .Concat(reclamos)
                .Concat(comunicados)
                .OrderByDescending(item => item.Fecha)
                .Take(6)
                .ToList();
        }

        private bool IsPropietario()
        {
            return HttpContext.Session.GetString("UserRole")
                ?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
