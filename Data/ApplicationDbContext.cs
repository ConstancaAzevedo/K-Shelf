using K_Shelf.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace K_Shelf.Data
{
    /// <summary>
    /// contexto da base de dados da aplicacao k-shelf
    /// herda de identitydbcontext para integrar a gestao de utilizadores e autorizacoes do asp.net core identity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        // construtor que recebe as opcoes de configuracao do dbcontext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // nota: nao e necessario dbset<utilizador> porque o identitydbcontext ja o gere internamente

        // tabela que armazena os detalhes dos artistas (membros de grupos ou solistas)
        public DbSet<Artista> Artistas { get; set; }

        // tabela que armazena as informacoes dos grupos de k-pop
        public DbSet<Grupo> Grupos { get; set; }

        // tabela que armazena as informacoes dos artistas solistas
        public DbSet<Solista> Solistas { get; set; }

        // tabela que armazena os albuns de k-pop
        public DbSet<Album> Albuns { get; set; }

        // tabela que armazena as musicas (faixas) de cada album
        public DbSet<Musica> Musicas { get; set; }

        // tabela que armazena as colecoes criadas pelos utilizadores
        public DbSet<Colecao> Colecoes { get; set; }

        // tabela de ligacao muitos-para-muitos entre albuns e colecoes
        public DbSet<AlbumColecao> AlbumColecoes { get; set; }

        // tabela que armazena os photocards disponiveis no catalogo
        public DbSet<Photocard> Photocards { get; set; }

        // tabela que armazena os photocards colecionados pelos utilizadores
        public DbSet<UtilizadorPhotocard> UtilizadorPhotocards { get; set; }

        // configuracao avancada dos modelos e relacionamentos via fluent api
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // executa as configuracoes base do asp.net identity (tabelas de utilizadores e funcoes)
            base.OnModelCreating(builder);

            // relacao muitos-para-muitos: chave primaria composta em albumcolecao
            builder.Entity<AlbumColecao>().HasKey(ac => new { ac.AlbumId, ac.ColecaoId });

            // relacao albumcolecao -> album: restringir eliminacao em cascata para evitar ciclos de remocao
            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Album)
                .WithMany(a => a.AlbumColecoes)
                .HasForeignKey(ac => ac.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            // relacao albumcolecao -> colecao: restringir eliminacao em cascata
            builder.Entity<AlbumColecao>()
                .HasOne(ac => ac.Colecao)
                .WithMany(c => c.AlbumColecoes)
                .HasForeignKey(ac => ac.ColecaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // relacao um-para-muitos: album -> musica (eliminacao em cascata ativa)
            // se um album for apagado, todas as suas musicas associadas sao eliminadas automaticamente
            builder.Entity<Musica>()
                .HasOne(m => m.Album)
                .WithMany(a => a.Musicas)
                .HasForeignKey(m => m.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacao photocard -> album: restringir eliminacao em cascata para evitar ciclos de remocao
            builder.Entity<Photocard>()
                .HasOne(p => p.Album)
                .WithMany()
                .HasForeignKey(p => p.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            // relacao utilizadorphotocard -> utilizador: eliminacao em cascata
            builder.Entity<UtilizadorPhotocard>()
                .HasOne(up => up.Utilizador)
                .WithMany()
                .HasForeignKey(up => up.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacao utilizadorphotocard -> photocard: eliminacao em cascata
            builder.Entity<UtilizadorPhotocard>()
                .HasOne(up => up.Photocard)
                .WithMany(p => p.UtilizadorPhotocards)
                .HasForeignKey(up => up.PhotocardId)
                .OnDelete(DeleteBehavior.Cascade);

            // criacao de indices para otimizar pesquisas frequentes por titulo e chave estrangeira do album
            builder.Entity<Musica>().HasIndex(m => m.Titulo);
            builder.Entity<Musica>().HasIndex(m => m.AlbumId);
        }
    }
}