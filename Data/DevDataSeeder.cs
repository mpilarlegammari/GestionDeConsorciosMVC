using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.Services;
using Microsoft.EntityFrameworkCore;

public static class DevDataSeeder
{
    public static async Task EnsureAdminUserAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GestionDeConsorciosContext>();

        var admin = await context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Email == "admin@admin.com");

        if (admin is null)
        {
            context.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                Apellido = "Demo",
                Email = "admin@admin.com",
                PasswordHash = "admin",
                Rol = RolUsuario.Administrador,
                Activo = true
            });
        }
        else
        {
            admin.Rol = RolUsuario.Administrador;
            admin.Activo = true;

            if (string.IsNullOrWhiteSpace(admin.PasswordHash) ||
                !string.Equals(admin.PasswordHash, "admin", StringComparison.Ordinal))
            {
                admin.PasswordHash = "admin";
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedFirstConsorcioOwnersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GestionDeConsorciosContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var usuariosService = scope.ServiceProvider.GetRequiredService<IUsuariosService>();

        var consorcio = await context.Consorcios
            .Include(c => c.UnidadesFuncionales)
                .ThenInclude(u => u.Expensas)
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();

        if (consorcio is null)
        {
            return;
        }

        var unidades = consorcio.UnidadesFuncionales
            .Where(u => !string.IsNullOrWhiteSpace(u.MailPropietario))
            .OrderBy(u => u.NumeroUF)
            .Take(2)
            .ToList();

        await usuariosService.EnsurePropietarioUsersAsync(unidades);

        foreach (var unidad in unidades)
        {
            var expensa = unidad.Expensas
                .OrderByDescending(e => e.FechaEmision)
                .FirstOrDefault();

            if (expensa is null)
            {
                expensa = CreateSeedExpensa(unidad);
                context.Expensas.Add(expensa);
                await context.SaveChangesAsync();
            }

            await EnsurePagoAsync(context, environment, unidad, expensa);
        }

        await context.SaveChangesAsync();
    }

    private static Expensa CreateSeedExpensa(UnidadFuncional unidad)
    {
        var today = DateTime.Today;
        var periodo = today.ToString("yyyy-MM");

        return new Expensa
        {
            UnidadFuncionalId = unidad.Id,
            Periodo = periodo,
            FechaEmision = new DateTime(today.Year, today.Month, 1),
            FechaVencimiento = new DateTime(today.Year, today.Month, 10).AddMonths(1),
            MontoTotal = 0,
            Estado = EstadoExpensa.Pendiente,
            Observaciones = "Expensa creada automaticamente para datos de desarrollo."
        };
    }

    private static async Task EnsurePagoAsync(
        GestionDeConsorciosContext context,
        IWebHostEnvironment environment,
        UnidadFuncional unidad,
        Expensa expensa)
    {
        var numeroOperacion = $"SEED-CONSORCIO-{unidad.ConsorcioId}-UF-{unidad.NumeroUF}-{expensa.Periodo}";
        var exists = await context.Pagos.AnyAsync(p =>
            p.NumeroOperacion == numeroOperacion
            || (p.ExpensaId == expensa.Id
                && p.NumeroOperacion != null
                && p.NumeroOperacion.StartsWith("SEED-")
                && p.Comentarios != null
                && p.Comentarios.Contains(unidad.MailPropietario)));

        if (exists)
        {
            return;
        }

        var comprobantePath = await EnsureComprobanteAsync(environment, unidad, expensa);

        context.Pagos.Add(new Pago
        {
            ExpensaId = expensa.Id,
            FechaPago = DateTime.Today,
            MontoPagado = expensa.MontoTotal,
            MedioPago = "Transferencia bancaria",
            NumeroOperacion = numeroOperacion,
            BancoEntidad = "Banco Nacion",
            ComprobantePath = comprobantePath,
            Comentarios = $"Pago de desarrollo asociado a {unidad.MailPropietario}.",
            Estado = EstadoPago.PendienteRevision
        });
    }

    private static async Task<string> EnsureComprobanteAsync(
        IWebHostEnvironment environment,
        UnidadFuncional unidad,
        Expensa expensa)
    {
        var uploadsPath = Path.Combine(environment.WebRootPath, "uploads", "pagos");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"comprobante-seed-consorcio-{unidad.ConsorcioId}-uf-{unidad.NumeroUF}-{expensa.Periodo}.pdf";
        var filePath = Path.Combine(uploadsPath, fileName);

        if (!File.Exists(filePath))
        {
            var pdf = "%PDF-1.1\n1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n2 0 obj\n<< /Type /Pages /Count 0 >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF";
            await File.WriteAllTextAsync(filePath, pdf);
        }

        return $"/uploads/pagos/{fileName}";
    }
}
