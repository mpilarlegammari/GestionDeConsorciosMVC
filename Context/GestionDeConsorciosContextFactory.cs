using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GestionDeConsorciosMVC.Context
{
    public class GestionDeConsorciosContextFactory : IDesignTimeDbContextFactory<GestionDeConsorciosContext>
    {
        public GestionDeConsorciosContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string? connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? configuration["ConnectionString:DefaultConnection"]
                ?? configuration["GESTION_CONSORCIOS_CONNECTION_STRING"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No se encontró una cadena de conexión para GestionDeConsorciosContext. " +
                    "Definí ConnectionStrings:DefaultConnection, ConnectionString:DefaultConnection " +
                    "o la variable de entorno GESTION_CONSORCIOS_CONNECTION_STRING.");
            }

            DbContextOptionsBuilder<GestionDeConsorciosContext> optionsBuilder = new DbContextOptionsBuilder<GestionDeConsorciosContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new GestionDeConsorciosContext(optionsBuilder.Options);
        }
    }
}
