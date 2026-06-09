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
                .AddJsonFile("appsettings.json")
                .Build();

            string? connectionString = configuration["ConnectionString:DefaultConnection"];

            DbContextOptionsBuilder<GestionDeConsorciosContext> optionsBuilder = new DbContextOptionsBuilder<GestionDeConsorciosContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new GestionDeConsorciosContext(optionsBuilder.Options);
        }
    }
}