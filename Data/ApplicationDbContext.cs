using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<FormularioAdocao> FormulariosAdocao { get; set; }
        public DbSet<Adocao> Adocoes { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder construtorModelo)
        {
            base.OnModelCreating(construtorModelo);

            construtorModelo.Entity<Pet>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entidade.Property(e => e.Especie).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.Raca).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.Sexo).IsRequired().HasMaxLength(20);
                entidade.Property(e => e.Porte).IsRequired().HasMaxLength(20);
                entidade.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.Descricao).HasMaxLength(500);
                entidade.Property(e => e.NomeArquivoImagem).HasMaxLength(255);
            });

            construtorModelo.Entity<Usuario>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.CPF).IsRequired().HasMaxLength(11);
                entidade.HasIndex(e => e.CPF).IsUnique();
                entidade.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entidade.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entidade.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
                entidade.Property(e => e.Senha).IsRequired().HasMaxLength(100);
            });
            
            construtorModelo.Entity<Colaborador>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entidade.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entidade.HasIndex(e => e.Email).IsUnique();
                entidade.Property(e => e.Senha).IsRequired().HasMaxLength(100);
                entidade.Property(e => e.CPF).IsRequired().HasMaxLength(11);
                entidade.HasIndex(e => e.CPF).IsUnique();
                entidade.Property(e => e.Telefone).IsRequired().HasMaxLength(15);
                entidade.Property(e => e.Cargo).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.Ativo).IsRequired();
                entidade.Property(e => e.UsuarioId).IsRequired(false);
            });

            construtorModelo.Entity<FormularioAdocao>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.ObservacaoAdminFormulario).HasMaxLength(500);
                entidade.Property(e => e.UsuarioId).IsRequired();
                entidade.Property(e => e.PetId).IsRequired();
                entidade.Property(e => e.RendaMensal).HasPrecision(18, 2);
            });

            construtorModelo.Entity<Adocao>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entidade.Property(e => e.UsuarioId).IsRequired();
                entidade.Property(e => e.PetId).IsRequired();
            });

            construtorModelo.Entity<Notificacao>(entidade =>
            {
                entidade.HasKey(e => e.Id);
                entidade.Property(e => e.Mensagem).IsRequired().HasMaxLength(255);
                entidade.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entidade.Property(e => e.Referencia).HasMaxLength(50);
                entidade.Property(e => e.UsuarioId).IsRequired();
            });

            construtorModelo.Entity<Pet>()
                .HasMany(p => p.FormulariosAdocao)
                .WithOne(f => f.Pet)
                .HasForeignKey(f => f.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            construtorModelo.Entity<Pet>()
                .HasMany(p => p.Adocoes)
                .WithOne(a => a.Pet)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            construtorModelo.Entity<Usuario>()
                .HasMany(u => u.FormulariosAdocao)
                .WithOne(f => f.Usuario)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            construtorModelo.Entity<Usuario>()
                .HasMany(u => u.Adocoes)
                .WithOne(a => a.Usuario)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            construtorModelo.Entity<Usuario>()
                .HasMany(u => u.Notificacoes)
                .WithOne(n => n.Usuario)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}