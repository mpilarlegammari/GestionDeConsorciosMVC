using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
    public class UsuariosService : IUsuariosService
    {
        private readonly GestionDeConsorciosContext _context;

        public UsuariosService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task EnsurePropietarioUsersAsync(IEnumerable<UnidadFuncional> unidadesFuncionales)
        {
            var unidades = unidadesFuncionales
                .Where(unidad =>
                    !string.IsNullOrWhiteSpace(unidad.MailPropietario) &&
                    !string.IsNullOrWhiteSpace(unidad.DniPropietario))
                .GroupBy(unidad => unidad.MailPropietario.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            if (unidades.Count == 0)
            {
                return;
            }

            foreach (var unidad in unidades)
            {
                var email = unidad.MailPropietario.Trim();
                var dni = unidad.DniPropietario.Trim();
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(item => item.Email == email);

                if (usuario is null)
                {
                    var nombre = BuildNombreApellido(unidad);

                    _context.Usuarios.Add(new Usuario
                    {
                        Nombre = nombre.Nombre,
                        Apellido = nombre.Apellido,
                        Email = email,
                        PasswordHash = dni,
                        Rol = RolUsuario.Propietario,
                        Activo = true
                    });

                    continue;
                }

                usuario.Rol = RolUsuario.Propietario;
                usuario.Activo = true;

                if (string.IsNullOrWhiteSpace(usuario.PasswordHash) ||
                    !string.Equals(usuario.PasswordHash, dni, StringComparison.Ordinal))
                {
                    usuario.PasswordHash = dni;
                }

                var nombreActualizado = BuildNombreApellido(unidad);

                if (string.IsNullOrWhiteSpace(usuario.Nombre))
                {
                    usuario.Nombre = nombreActualizado.Nombre;
                }

                if (string.IsNullOrWhiteSpace(usuario.Apellido))
                {
                    usuario.Apellido = nombreActualizado.Apellido;
                }
            }

            await _context.SaveChangesAsync();
        }

        private static (string Nombre, string Apellido) BuildNombreApellido(UnidadFuncional unidad)
        {
            var partes = (unidad.NombrePropietario ?? string.Empty)
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0)
            {
                return ("Propietario", $"UF {unidad.NumeroUF}");
            }

            if (partes.Length == 1)
            {
                return (partes[0], $"UF {unidad.NumeroUF}");
            }

            return (partes[0], string.Join(' ', partes.Skip(1)));
        }
    }
}
