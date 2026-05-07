using Microsoft.EntityFrameworkCore;

public class GestionConsorciosContext : DbContext
{
    public DbSet<Consorcio> Consorcios { get; set; }
    public DbSet<UnidadFuncional> UnidadesFuncionales { get; set; }
    public DbSet<Gasto> Gastos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=DESKTOP-MPILARL;Database=GestionConsorciosDB;Trusted_Connection=True;TrustServerCertificate=True;"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consorcio>()
            .HasMany(c => c.UnidadesFuncionales)
            .WithOne(u => u.Consorcio)
            .HasForeignKey(u => u.ConsorcioId);

        modelBuilder.Entity<Consorcio>()
            .HasMany(c => c.Gastos)
            .WithOne(g => g.Consorcio)
            .HasForeignKey(g => g.ConsorcioId);
    }
}