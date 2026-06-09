using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
	public class GastoService : IGastoService
	{
		private readonly GestionDeConsorciosContext _context;

		public GastoService(GestionDeConsorciosContext context)
		{
			_context = context;
		}

		public async Task<List<Gasto>> GetAllAsync(int? consorcioId = null)
		{
			var query = _context.Gastos
				.Include(gasto => gasto.Consorcio)
				.AsNoTracking();

			if (consorcioId.HasValue)
			{
				query = query.Where(gasto => gasto.ConsorcioId == consorcioId.Value);
			}

			return await query
				.OrderByDescending(gasto => gasto.Fecha)
				.ThenBy(gasto => gasto.NumeroFactura)
				.ToListAsync();
		}

		public async Task<Gasto?> GetByIdAsync(int id)
		{
			return await _context.Gastos
				.Include(gasto => gasto.Consorcio)
				.AsNoTracking()
				.FirstOrDefaultAsync(gasto => gasto.Id == id);
		}

		public async Task<GastoVM?> GetForEditAsync(int id)
		{
			var gasto = await _context.Gastos
				.AsNoTracking()
				.FirstOrDefaultAsync(item => item.Id == id);

			if (gasto is null)
			{
				return null;
			}

			return new GastoVM
			{
				Id = gasto.Id,
				ConsorcioId = gasto.ConsorcioId,
				NumeroFactura = gasto.NumeroFactura,
				Fecha = gasto.Fecha,
				Monto = gasto.Monto,
				Concepto = gasto.Concepto,
				Categoria = gasto.Categoria,
				ArchivoFacturaPath = gasto.ArchivoFacturaPath
			};
		}

		public async Task<List<Consorcio>> GetConsorciosAsync()
		{
			return await _context.Consorcios
				.AsNoTracking()
				.OrderBy(consorcio => consorcio.Nombre)
				.ToListAsync();
		}

		public async Task<bool> ExistsNumeroFacturaAsync(string numeroFactura, int? excludeGastoId = null)
		{
			var normalized = Normalize(numeroFactura);

			return await _context.Gastos.AnyAsync(gasto =>
				gasto.NumeroFactura == normalized &&
				(!excludeGastoId.HasValue || gasto.Id != excludeGastoId.Value));
		}

		public async Task<Gasto> CreateAsync(GastoVM model, string archivoFacturaPath)
		{
			var gasto = new Gasto
			{
				ConsorcioId = model.ConsorcioId,
				NumeroFactura = Normalize(model.NumeroFactura),
				Fecha = model.Fecha.Date,
				Monto = model.Monto,
				Concepto = Normalize(model.Concepto),
				Categoria = Normalize(model.Categoria),
				ArchivoFacturaPath = archivoFacturaPath
			};

			_context.Gastos.Add(gasto);
			await _context.SaveChangesAsync();

			return gasto;
		}

		public async Task<bool> UpdateAsync(GastoVM model, string? archivoFacturaPath = null)
		{
			var gasto = await _context.Gastos.FindAsync(model.Id);

			if (gasto is null)
			{
				return false;
			}

			gasto.ConsorcioId = model.ConsorcioId;
			gasto.NumeroFactura = Normalize(model.NumeroFactura);
			gasto.Fecha = model.Fecha.Date;
			gasto.Monto = model.Monto;
			gasto.Concepto = Normalize(model.Concepto);
			gasto.Categoria = Normalize(model.Categoria);

			if (!string.IsNullOrWhiteSpace(archivoFacturaPath))
			{
				gasto.ArchivoFacturaPath = archivoFacturaPath;
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var gasto = await _context.Gastos.FindAsync(id);

			if (gasto is null)
			{
				return false;
			}

			_context.Gastos.Remove(gasto);
			await _context.SaveChangesAsync();
			return true;
		}

		private static string Normalize(string? value)
		{
			return (value ?? string.Empty).Trim();
		}
	}
}
