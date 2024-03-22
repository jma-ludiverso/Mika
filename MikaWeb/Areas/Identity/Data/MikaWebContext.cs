using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MikaWeb.Areas.Identity.Data;
using MikaWeb.Models;

namespace MikaWeb.Data
{
    public class MikaWebContext : IdentityDbContext<MikaWebUser>
    {
        public MikaWebContext(DbContextOptions<MikaWebContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Cajas>().HasKey(c => new { c.NCaja, c.IdSalon });
            builder.Entity<Cliente>().HasKey(c => new { c.IdCliente });
            builder.Entity<Cajas_Gastos>().HasKey(g => new { g.NCaja, g.IdSalon, g.Linea });
            builder.Entity<Servicio>().HasKey(s => new { s.IdServicio, s.IdEmpresa});
        }

        public DbSet<Cajas> Cajas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cajas_Gastos> Cajas_Gastos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Empleado> vEmpleados { get; set; }

    }
}
