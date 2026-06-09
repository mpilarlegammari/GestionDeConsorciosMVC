using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;

public static class DevDataSeeder
{
    public static async Task SeedFirstConsorcioOwnersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GestionDeConsorciosContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

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

        foreach (var unidad in unidades)
        {
            await EnsureUsuarioAsync(context, unidad);

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

    private static async Task EnsureUsuarioAsync(GestionDeConsorciosContext context, UnidadFuncional unidad)
    {
        var email = unidad.MailPropietario.Trim();
        var exists = await context.Usuarios.AnyAsync(u => u.Email == email);

        if (exists)
        {
            return;
        }

        var partesNombre = (unidad.NombrePropietario ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        context.Usuarios.Add(new Usuario
        {
            Nombre = partesNombre.FirstOrDefault() ?? $"Propietario UF {unidad.NumeroUF}",
            Apellido = partesNombre.Length > 1 ? string.Join(' ', partesNombre.Skip(1)) : unidad.NumeroUF,
            Email = email,
            PasswordHash = "dev-seed-password",
            Rol = RolUsuario.Propietario,
            Activo = true
        });
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
        var exists = await context.Pagos.AnyAsync(p => p.NumeroOperacion == numeroOperacion);

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
