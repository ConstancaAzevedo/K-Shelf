using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_Shelf.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelasNovas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==========================================
            // CRIAR TABELA Grupos
            // ==========================================
            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataEstreia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Companhia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fansigno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            // ==========================================
            // CRIAR TABELA Solistas
            // ==========================================
            migrationBuilder.CreateTable(
                name: "Solistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataEstreia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Companhia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solistas", x => x.Id);
                });

            // ==========================================
            // CRIAR TABELA Albuns
            // ==========================================
            migrationBuilder.CreateTable(
                name: "Albuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CapaUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Edicao = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: true),
                    SolistaId = table.Column<int>(type: "int", nullable: true),
                    ArtistaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albuns_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Albuns_Solistas_SolistaId",
                        column: x => x.SolistaId,
                        principalTable: "Solistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Albuns_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ==========================================
            // CRIAR TABELA Musicas
            // ==========================================
            migrationBuilder.CreateTable(
                name: "Musicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Duracao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackNumber = table.Column<int>(type: "int", nullable: false),
                    Letra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Compositores = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Produtores = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSingle = table.Column<bool>(type: "bit", nullable: false),
                    IsTitleTrack = table.Column<bool>(type: "bit", nullable: false),
                    SpotifyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YoutubeUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlbumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Musicas_Albuns_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ==========================================
            // CRIAR TABELA Colecoes
            // ==========================================
            migrationBuilder.CreateTable(
                name: "Colecoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colecoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colecoes_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ==========================================
            // CRIAR TABELA AlbumColecoes (Many-to-Many)
            // ==========================================
            migrationBuilder.CreateTable(
                name: "AlbumColecoes",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    ColecaoId = table.Column<int>(type: "int", nullable: false),
                    DataAdicao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumColecoes", x => new { x.AlbumId, x.ColecaoId });
                    table.ForeignKey(
                        name: "FK_AlbumColecoes_Albuns_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlbumColecoes_Colecoes_ColecaoId",
                        column: x => x.ColecaoId,
                        principalTable: "Colecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ==========================================
            // ÍNDICES
            // ==========================================
            migrationBuilder.CreateIndex(
                name: "IX_Albuns_GrupoId",
                table: "Albuns",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Albuns_SolistaId",
                table: "Albuns",
                column: "SolistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Albuns_ArtistaId",
                table: "Albuns",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Musicas_AlbumId",
                table: "Musicas",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Colecoes_UtilizadorId",
                table: "Colecoes",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumColecoes_ColecaoId",
                table: "AlbumColecoes",
                column: "ColecaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlbumColecoes");
            migrationBuilder.DropTable(name: "Musicas");
            migrationBuilder.DropTable(name: "Colecoes");
            migrationBuilder.DropTable(name: "Albuns");
            migrationBuilder.DropTable(name: "Grupos");
            migrationBuilder.DropTable(name: "Solistas");
        }
    }
}