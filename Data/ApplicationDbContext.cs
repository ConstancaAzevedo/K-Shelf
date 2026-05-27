using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Models;

namespace K_Shelf.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // NÃO tem DbSet<Utilizador> - Identity já gere

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

            builder.Entity<AlbumColecao>().HasKey(ac => new { ac.AlbumId, ac.ColecaoId });

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

            builder.Entity<Musica>()
                .HasOne(m => m.Album)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Musica>().HasIndex(m => m.Titulo);
            builder.Entity<Musica>().HasIndex(m => m.AlbumId);
        }
    }
}