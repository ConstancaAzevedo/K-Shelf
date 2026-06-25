using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using K_Shelf.Models;

namespace K_Shelf.Data
{
    /// <summary>
    /// Contexto da Base de Dados da aplicação K-Shelf.
    /// Herda de IdentityDbContext para integrar a gestão de utilizadores e autorizações do ASP.NET Core Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // NOTA: Não é necessário DbSet<Utilizador> porque o IdentityDbContext já o gere internamente.

        /// <summary>Tabela que armazena os detalhes dos Artistas (membros de grupos ou solistas).</summary>
        public DbSet<Artista> Artistas { get; set; }

        /// <summary>Tabela que armazena as informações dos Grupos de K-Pop.</summary>
        public DbSet<Grupo> Grupos { get; set; }

        /// <summary>Tabela que armazena as informações dos Artistas Solistas.</summary>
        public DbSet<Solista> Solistas { get; set; }

        /// <summary>Tabela que armazena os Álbuns de K-Pop.</summary>
        public DbSet<Album> Albuns { get; set; }

        /// <summary>Tabela que armazena as Músicas (faixas) de cada Álbum.</summary>
        public DbSet<Musica> Musicas { get; set; }

        /// <summary>Tabela que armazena as Coleções criadas pelos utilizadores.</summary>
        public DbSet<Colecao> Colecoes { get; set; }

        /// <summary>Tabela de ligação Muitos-para-Muitos entre Álbuns e Coleções.</summary>
        public DbSet<AlbumColecao> AlbumColecoes { get; set; }

        /// <summary>
        /// Configuração avançada dos modelos e relacionamentos via Fluent API.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Executa as configurações base do ASP.NET Identity (tabelas de utilizadores e funções)
            base.OnModelCreating(builder);

            // Relação Muitos-para-Muitos: Chave Primária Composta em AlbumColecao
            builder.Entity<AlbumColecao>().HasKey(ac => new { ac.AlbumId, ac.ColecaoId });

            // Relação AlbumColecao -> Album: Restringir eliminação em cascata para evitar ciclos de remoção
            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Album)
                .WithMany(a => a.AlbumColecoes)
                .HasForeignKey(ac => ac.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação AlbumColecao -> Colecao: Restringir eliminação em cascata
            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Colecao)
                .WithMany(c => c.AlbumColecoes)
                .HasForeignKey(ac => ac.ColecaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação Um-para-Muitos: Album -> Musica (Eliminação em Cascata ativa)
            // Se um Álbum for apagado, todas as suas músicas associadas são eliminadas automaticamente.
            builder.Entity<Musica>()
                .HasOne(m => m.Album)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            // Criação de índices para otimizar pesquisas frequentes por título e chave estrangeira do álbum
            builder.Entity<Musica>().HasIndex(m => m.Titulo);
            builder.Entity<Musica>().HasIndex(m => m.AlbumId);
        }
    }
}