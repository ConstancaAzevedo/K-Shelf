using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace K_Shelf.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDuracaoToTimeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Renomear coluna Nacionalidade para Pais na tabela Artistas
            // migrationBuilder.RenameColumn(
            //     name: "Nacionalidade",
            //     table: "Artistas",
            //     newName: "Pais");

            // 2. Alterar o tipo da coluna Duracao para TimeSpan
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Duracao",
                table: "Musicas",
                type: "time",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // 3. Adicionar coluna PreviewAudioUrl
            // migrationBuilder.AddColumn<string>(
            //     name: "PreviewAudioUrl",
            //     table: "Musicas",
            //     type: "nvarchar(max)",
            //     nullable: true);

            /*
            // 4. Criar tabela Photocards 
            migrationBuilder.CreateTable(
                name: "Photocards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Versao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    AlbumId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photocards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photocards_Albuns_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Photocards_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 5. Criar tabela UtilizadorPhotocards (JÁ EXISTE)
            migrationBuilder.CreateTable(
                name: "UtilizadorPhotocards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhotocardId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilizadorPhotocards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UtilizadorPhotocards_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UtilizadorPhotocards_Photocards_PhotocardId",
                        column: x => x.PhotocardId,
                        principalTable: "Photocards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 6. Criar índices para as tabelas (JÁ EXISTEM)
            migrationBuilder.CreateIndex(
                name: "IX_Photocards_AlbumId",
                table: "Photocards",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Photocards_ArtistaId",
                table: "Photocards",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilizadorPhotocards_PhotocardId",
                table: "UtilizadorPhotocards",
                column: "PhotocardId");

            migrationBuilder.CreateIndex(
                name: "IX_UtilizadorPhotocards_UtilizadorId",
                table: "UtilizadorPhotocards",
                column: "UtilizadorId");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // As partes abaixo estão comentadas para não causar erro.
            // ============================================================

            /*
            // Remover tabelas (se existissem)
            migrationBuilder.DropTable(
                name: "UtilizadorPhotocards");

            migrationBuilder.DropTable(
                name: "Photocards");
            */

            // Remover a coluna PreviewAudioUrl
            migrationBuilder.DropColumn(
                name: "PreviewAudioUrl",
                table: "Musicas");

            // Reverter o nome da coluna
            // migrationBuilder.RenameColumn(
            //     name: "Nacionalidade",
            //     table: "Artistas",
            //     newName: "Pais");

            // Reverter o tipo da coluna Duracao
            migrationBuilder.AlterColumn<string>(
                name: "Duracao",
                table: "Musicas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);
        }
    }
}