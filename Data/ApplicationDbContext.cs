using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Models;

namespace K_Shelf.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Solista> Solistas { get; set; }
        public DbSet<Album> Albuns { get; set; }
        public DbSet<Musica> Musicas { get; set; }
        public DbSet<Colecao> Colecoes { get; set; }
        public DbSet<AlbumColecao> AlbumColecoes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar chave composta para tabela de junção
            builder.Entity<AlbumColecao>()
                .HasKey(ac => new { ac.AlbumId, ac.ColecaoId });

            // Configurar relacionamentos
            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Album)
                .WithMany(a => a.AlbumColecoes)
                .HasForeignKey(ac => ac.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Colecao)
                .WithMany(c => c.AlbumColecoes)
                .HasForeignKey(ac => ac.ColecaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relacionamento Album -> Musica
            builder.Entity<Musica>()
                .HasOne(m => m.Album)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.AlbumId)
                .OnDelete(DeleteBehavior.Cascade); // Se apagar álbum, apaga músicas

            // Índices para melhor performance
            builder.Entity<Musica>()
                .HasIndex(m => m.Titulo);

            builder.Entity<Musica>()
                .HasIndex(m => m.AlbumId);
        }
    }
}