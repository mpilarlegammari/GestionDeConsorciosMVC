using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestionDeConsorciosMVC.Models;

namespace GestionDeConsorciosMVC.Context
{
    public class GestionDeConsorciosContext : DbContext
    {
        public GestionDeConsorciosContext(DbContextOptions<GestionDeConsorciosContext> options)
            : base(options)
        {
        }

        public DbSet<Consorcio> Consorcios { get; set; }
        public DbSet<UnidadFuncional> UnidadesFuncionales { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
    }
}
