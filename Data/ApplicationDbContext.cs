using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AppControleDeGastos.Models.UsuarioModel;
using AppControleDeGastos.Models.ReceitaModel;
using AppControleDeGastos.Models.CartaoModel;
using AppControleDeGastos.Models.DespesaModel;
using AppControleDeGastos.Models.CategoriaModel;

namespace AppControleDeGastos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<Cartao> Cartoes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Despesa> Despesa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .ToTable("Usuario");

            modelBuilder.Entity<Receita>()
                .ToTable("Receita");

            modelBuilder.Entity<Cartao>()
                .ToTable("Cartoes"); 

            modelBuilder.Entity<Categoria>()
                .ToTable("Categorias");

            modelBuilder.Entity<Despesa>()
                .ToTable("Despesa");
        }
    }
}
