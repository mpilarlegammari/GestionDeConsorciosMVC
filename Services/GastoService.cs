using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
	public class GastoService : IGastoService
	{
		private static readonly List<string> DefaultCategorias =
		[
			"Limpieza",
			"Mantenimiento",
			"Servicios",
			"Seguridad",
			"Administracion",
			"Otros"
		];

		private readonly GestionDeConsorciosContext _context;

		public GastoService(GestionDeConsorciosContext context)
		{
			_context = context;
		}

		public async Task<List<Gasto>> GetAllAsync(
			int? consorcioId = null,
			int? mes = null,
			int? anio = null,
			string? categoria = null,
			string? busqueda = null)
		{
			var query = _context.Gastos
				.Include(gasto => gasto.Consorcio)
				.AsNoTracking();

			if (consorcioId.HasValue)
			{
				query = query.Where(gasto => gasto.ConsorcioId == consorcioId.Value);
			}

			if (mes is >= 1 and <= 12)
			{
				query = query.Where(gasto => gasto.Fecha.Month == mes.Value);
			}

			if (anio.HasValue && anio.Value > 0)
			{
				query = query.Where(gasto => gasto.Fecha.Year == anio.Value);
			}

			var normalizedCategoria = Normalize(categoria);

			if (TryParseCategoria(normalizedCategoria, out var categoriaGasto))
			{
				query = query.Where(gasto => gasto.Categoria == categoriaGasto);
			}

			var normalizedBusqueda = Normalize(busqueda);

			if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
			{
				query = query.Where(gasto =>
					EF.Functions.Like(gasto.NumeroFactura, $"%{normalizedBusqueda}%") ||
					EF.Functions.Like(gasto.Concepto, $"%{normalizedBusqueda}%"));
			}

			return await query
				.OrderByDescending(gasto => gasto.Fecha)
				.ThenBy(gasto => gasto.NumeroFactura)
				.ToListAsync();
		}

		public async Task<MisGastosViewModel> GetMisGastosAsync(
			string email,
			int? unidadFuncionalId = null,
			int? mes = null,
			int? anio = null,
			string? categoria = null,
			string? busqueda = null)
		{
			var normalizedEmail = Normalize(email);
			var normalizedCategoria = NormalizeOptional(categoria);
			var normalizedBusqueda = NormalizeOptional(busqueda);
			var unidades = await GetOwnerUnidadesAsync(normalizedEmail);
			var unidadesSeleccionadas = unidadFuncionalId.HasValue
				? unidades.Where(unidad => unidad.Id == unidadFuncionalId.Value).ToList()
				: unidades.ToList();
			var consorcioIds = unidadesSeleccionadas
				.Select(unidad => unidad.ConsorcioId)
				.Distinct()
				.ToList();
			var ownerConsorcioIds = unidades
				.Select(unidad => unidad.ConsorcioId)
				.Distinct()
				.ToList();

			var query = _context.Gastos
				.Include(gasto => gasto.Consorcio)
				.AsNoTracking()
				.Where(gasto => consorcioIds.Contains(gasto.ConsorcioId));

			if (mes is >= 1 and <= 12)
			{
				query = query.Where(gasto => gasto.Fecha.Month == mes.Value);
			}

			if (anio.HasValue && anio.Value > 0)
			{
				query = query.Where(gasto => gasto.Fecha.Year == anio.Value);
			}

			if (TryParseCategoria(normalizedCategoria, out var categoriaGasto))
			{
				query = query.Where(gasto => gasto.Categoria == categoriaGasto);
			}

			if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
			{
				var pattern = $"%{normalizedBusqueda}%";
				query = query.Where(gasto =>
					EF.Functions.Like(gasto.NumeroFactura, pattern) ||
					EF.Functions.Like(gasto.Concepto, pattern));
			}

			return new MisGastosViewModel
			{
				Gastos = await query
					.OrderByDescending(gasto => gasto.Fecha)
					.ThenBy(gasto => gasto.NumeroFactura)
					.ToListAsync(),
				UnidadesFuncionales = unidades,
				UnidadesSeleccionadas = unidadesSeleccionadas,
				Categorias = await GetCategoriasAsync(ownerConsorcioIds),
				AniosDisponibles = await GetAniosDisponiblesAsync(ownerConsorcioIds),
				UnidadFuncionalId = unidadFuncionalId,
				Mes = mes,
				Anio = anio,
				Categoria = normalizedCategoria,
				Busqueda = normalizedBusqueda
			};
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
				Categoria = gasto.Categoria.ToString(),
				Descripcion = gasto.Descripcion,
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
				Categoria = ParseCategoria(model.Categoria),
				ArchivoFacturaPath = archivoFacturaPath,
				Descripcion = NormalizeOptional(model.Descripcion)
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
			gasto.Categoria = ParseCategoria(model.Categoria);
			gasto.Descripcion = NormalizeOptional(model.Descripcion);

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

		private static string? NormalizeOptional(string? value)
		{
			var normalized = Normalize(value);
			return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
		}

		private static CategoriaGasto ParseCategoria(string? value)
		{
			return TryParseCategoria(value, out var categoria) ? categoria : CategoriaGasto.Otros;
		}

		private static bool TryParseCategoria(string? value, out CategoriaGasto categoria)
		{
			return Enum.TryParse(Normalize(value), ignoreCase: true, out categoria);
		}

		private async Task<List<UnidadFuncional>> GetOwnerUnidadesAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return [];
			}

			return await _context.UnidadesFuncionales
				.Include(unidad => unidad.Consorcio)
				.AsNoTracking()
				.Where(unidad => unidad.MailPropietario == email)
				.OrderBy(unidad => unidad.Consorcio.Nombre)
				.ThenBy(unidad => unidad.NumeroUF)
				.ToListAsync();
		}

		private async Task<List<string>> GetCategoriasAsync(List<int> consorcioIds)
		{
			if (consorcioIds.Count == 0)
			{
				return DefaultCategorias;
			}

			var gastos = await _context.Gastos
				.AsNoTracking()
				.Where(gasto => consorcioIds.Contains(gasto.ConsorcioId))
				.ToListAsync();

			var categorias = gastos
				.Select(gasto => gasto.Categoria.ToString())
				.Distinct()
				.OrderBy(categoria => categoria)
				.ToList();

			return categorias.Count == 0 ? DefaultCategorias : categorias;
		}

		private async Task<List<int>> GetAniosDisponiblesAsync(List<int> consorcioIds)
		{
			if (consorcioIds.Count == 0)
			{
				return [DateTime.Today.Year];
			}

			var anios = await _context.Gastos
				.AsNoTracking()
				.Where(gasto => consorcioIds.Contains(gasto.ConsorcioId))
				.Select(gasto => gasto.Fecha.Year)
				.Distinct()
				.OrderByDescending(anio => anio)
				.ToListAsync();

			return anios.Count == 0 ? [DateTime.Today.Year] : anios;
		}
	}
}
