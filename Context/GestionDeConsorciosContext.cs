using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Context
{
    public class GestionDeConsorciosContext : DbContext
    {
        public GestionDeConsorciosContext(DbContextOptions<GestionDeConsorciosContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Consorcio> Consorcios { get; set; } = null!;
        public DbSet<UnidadFuncional> UnidadesFuncionales { get; set; } = null!;
        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<Expensa> Expensas { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<Comunicado> Comunicados { get; set; } = null!;
        public DbSet<Reclamo> Reclamos { get; set; } = null!;
        public DbSet<Amenity> Amenities { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TODO backend: revisar nombres de tablas y restricciones antes de generar migracion definitiva.
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Consorcio>().ToTable("Consorcios");
            modelBuilder.Entity<UnidadFuncional>().ToTable("UnidadesFuncionales");
            modelBuilder.Entity<Gasto>().ToTable("Gastos");
            modelBuilder.Entity<Expensa>().ToTable("Expensas");
            modelBuilder.Entity<Pago>().ToTable("Pagos");
            modelBuilder.Entity<Comunicado>().ToTable("Comunicados");
            modelBuilder.Entity<Reclamo>().ToTable("Reclamos");
            modelBuilder.Entity<Amenity>().ToTable("Amenities");
            modelBuilder.Entity<Reserva>().ToTable("Reservas");

            modelBuilder.Entity<Gasto>()
                .Property(g => g.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Expensa>()
                .Property(e => e.MontoTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pago>()
                .Property(p => p.MontoPagado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Consorcio>()
                .HasMany(c => c.UnidadesFuncionales)
                .WithOne(u => u.Consorcio)
                .HasForeignKey(u => u.ConsorcioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consorcio>()
                .HasMany(c => c.Gastos)
                .WithOne(g => g.Consorcio)
                .HasForeignKey(g => g.ConsorcioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consorcio>()
                .HasMany(c => c.Comunicados)
                .WithOne(c => c.Consorcio)
                .HasForeignKey(c => c.ConsorcioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consorcio>()
                .HasMany(c => c.Amenities)
                .WithOne(a => a.Consorcio)
                .HasForeignKey(a => a.ConsorcioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnidadFuncional>()
                .HasMany(u => u.Expensas)
                .WithOne(e => e.UnidadFuncional)
                .HasForeignKey(e => e.UnidadFuncionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnidadFuncional>()
                .HasMany(u => u.Reclamos)
                .WithOne(r => r.UnidadFuncional)
                .HasForeignKey(r => r.UnidadFuncionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UnidadFuncional>()
                .HasMany(u => u.Reservas)
                .WithOne(r => r.UnidadFuncional)
                .HasForeignKey(r => r.UnidadFuncionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Expensa>()
                .HasMany(e => e.Pagos)
                .WithOne(p => p.Expensa)
                .HasForeignKey(p => p.ExpensaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Amenity>()
                .HasMany(a => a.Reservas)
                .WithOne(r => r.Amenity)
                .HasForeignKey(r => r.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
