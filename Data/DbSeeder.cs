using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace K_Shelf.Data
{
    /// <summary>
    /// Classe utilitária responsável por semear (popular) a base de dados com dados iniciais de teste.
    /// Cria grupos, solistas, artistas, álbuns e músicas se estes ainda não existirem no sistema.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Semeia a base de dados de forma assíncrona.
        /// Garante a criação física da base de dados e insere registos de vários artistas famosos de K-Pop.
        /// </summary>
        /// <param name="context">O contexto da base de dados da aplicação.</param>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Certificar que a base de dados existe e está criada fisicamente na máquina
            await context.Database.EnsureCreatedAsync();

            // ==========================================
            // 1. BTS & Agust D (Membros e Álbuns Relacionados)
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "BTS"))
            {
                var bts = new Grupo
                {
                    Nome = "BTS",
                    DataEstreia = new DateTime(2013, 6, 13),
                    Companhia = "Big Hit Music (HYBE)",
                    Fansigno = "ARMY",
                    ImagemUrl = "/imagens/.jpeg",
                    IsAtivo = true
                };
                context.Grupos.Add(bts);
                await context.SaveChangesAsync();

                var agustD = await context.Solistas.FirstOrDefaultAsync(s => s.Nome == "Agust D");
                if (agustD == null)
                {
                    agustD = new Solista
                    {
                        Nome = "Agust D",
                        DataEstreia = new DateTime(2016, 8, 15),
                        Companhia = "Big Hit Music (HYBE)",
                        ImagemUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?q=80&w=600&auto=format&fit=crop",
                        IsAtivo = true
                    };
                    context.Solistas.Add(agustD);
                    await context.SaveChangesAsync();
                }

                var jungkook = new Artista
                {
                    Nome = "Jeon Jung-kook",
                    NomeArtistico = "Jungkook",
                    DataNascimento = new DateTime(1997, 9, 1),
                    Posicao = "Vocalista Principal, Dançarino, Centro",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jungkook.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id
                };

                var rm = new Artista
                {
                    Nome = "Kim Nam-joon",
                    NomeArtistico = "RM",
                    DataNascimento = new DateTime(1994, 9, 12),
                    Posicao = "Líder, Rapper Principal",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/RM.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id
                };

                var suga = new Artista
                {
                    Nome = "Min Yoon-gi",
                    NomeArtistico = "Suga",
                    DataNascimento = new DateTime(1993, 3, 9),
                    Posicao = "Rapper Líder",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Suga.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id,
                    SolistaId = agustD.Id
                };

                context.Artistas.AddRange(jungkook, rm, suga);
                await context.SaveChangesAsync();

                var mapOfTheSoul = new Album
                {
                    Titulo = "Map of the Soul: 7",
                    DataLancamento = new DateTime(2020, 2, 21),
                    CapaUrl = "/imagens/mapalbum.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = bts.Id
                };
                context.Albuns.Add(mapOfTheSoul);
                await context.SaveChangesAsync();

                var dday = new Album
                {
                    Titulo = "D-DAY",
                    DataLancamento = new DateTime(2023, 4, 21),
                    CapaUrl = "/imagens/agustd.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.JewelCase,
                    SolistaId = agustD.Id
                };
                context.Albuns.Add(dday);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "ON", Duracao = "4:06", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = mapOfTheSoul.Id },
                    new Musica { Titulo = "Black Swan", Duracao = "3:18", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = mapOfTheSoul.Id },
                    new Musica { Titulo = "Filter", Duracao = "3:00", TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = mapOfTheSoul.Id },
                    new Musica { Titulo = "Haegeum", Duracao = "2:48", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = dday.Id },
                    new Musica { Titulo = "People Pt.2 (feat. IU)", Duracao = "3:33", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = dday.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 2. BLACKPINK
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "BLACKPINK"))
            {
                var bp = new Grupo
                {
                    Nome = "BLACKPINK",
                    DataEstreia = new DateTime(2016, 8, 8),
                    Companhia = "YG Entertainment",
                    Fansigno = "BLINK",
                    ImagemUrl = "https://images.unsplash.com/photo-1598387181032-a3103a2db5b3?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                context.Grupos.Add(bp);
                await context.SaveChangesAsync();

                var jennie = new Artista
                {
                    Nome = "Jennie Kim",
                    NomeArtistico = "Jennie",
                    DataNascimento = new DateTime(1996, 1, 16),
                    Posicao = "Rapper Principal, Vocalista Líder",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jenni.jpeg",
                    DataEntrada = new DateTime(2016, 8, 8),
                    IsAtivo = true,
                    GrupoId = bp.Id
                };

                var lisa = new Artista
                {
                    Nome = "Lalisa Manobal",
                    NomeArtistico = "Lisa",
                    DataNascimento = new DateTime(1997, 3, 27),
                    Posicao = "Dançarina Principal, Rapper Líder",
                    Nacionalidade = "Tailandesa",
                    ImagemUrl = "/imagens/Lisa.jpeg",
                    DataEntrada = new DateTime(2016, 8, 8),
                    IsAtivo = true,
                    GrupoId = bp.Id
                };

                context.Artistas.AddRange(jennie, lisa);
                await context.SaveChangesAsync();

                var theAlbum = new Album
                {
                    Titulo = "The Album",
                    DataLancamento = new DateTime(2020, 10, 2),
                    CapaUrl = "/imagens/thealbum.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Special,
                    GrupoId = bp.Id
                };
                context.Albuns.Add(theAlbum);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "Lovesick Girls", Duracao = "3:12", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = theAlbum.Id },
                    new Musica { Titulo = "How You Like That", Duracao = "3:01", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = theAlbum.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 3. NewJeans
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "NewJeans"))
            {
                var nj = new Grupo
                {
                    Nome = "NewJeans",
                    DataEstreia = new DateTime(2022, 7, 22),
                    Companhia = "ADOR (HYBE)",
                    Fansigno = "Bunnies",
                    ImagemUrl = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                context.Grupos.Add(nj);
                await context.SaveChangesAsync();

                var hanni = new Artista
                {
                    Nome = "Hanni Pham",
                    NomeArtistico = "Hanni",
                    DataNascimento = new DateTime(2004, 10, 6),
                    Posicao = "Vocalista Líder, Dançarina Líder",
                    Nacionalidade = "Vietnamita-Australiana",
                    ImagemUrl = "/imagens/Hanni.webp",
                    DataEntrada = new DateTime(2022, 7, 22),
                    IsAtivo = true,
                    GrupoId = nj.Id
                };
                context.Artistas.Add(hanni);
                await context.SaveChangesAsync();

                var getUp = new Album
                {
                    Titulo = "Get Up",
                    DataLancamento = new DateTime(2023, 7, 21),
                    CapaUrl = "/imagens/newjeans.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Platform,
                    GrupoId = nj.Id
                };
                context.Albuns.Add(getUp);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "Super Shy", Duracao = "2:34", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = getUp.Id },
                    new Musica { Titulo = "ETA", Duracao = "2:31", TrackNumber = 2, IsSingle = true, IsTitleTrack = true, AlbumId = getUp.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 4. IU
            // ==========================================
            if (!await context.Solistas.AnyAsync(s => s.Nome == "IU"))
            {
                var iuSolista = new Solista
                {
                    Nome = "IU",
                    DataEstreia = new DateTime(2008, 9, 18),
                    Companhia = "EDAM Entertainment",
                    ImagemUrl = "/imagens/IU.jpeg",
                    IsAtivo = true
                };
                context.Solistas.Add(iuSolista);
                await context.SaveChangesAsync();

                var iuArtista = new Artista
                {
                    Nome = "Lee Ji-eun",
                    NomeArtistico = "IU",
                    DataNascimento = new DateTime(1993, 5, 16),
                    Posicao = "Vocalista Principal, Compositora",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/IU.jpeg",
                    DataEntrada = new DateTime(2008, 9, 18),
                    IsAtivo = true,
                    SolistaId = iuSolista.Id
                };
                context.Artistas.Add(iuArtista);
                await context.SaveChangesAsync();

                var lilac = new Album
                {
                    Titulo = "LILAC",
                    DataLancamento = new DateTime(2021, 3, 25),
                    CapaUrl = "/imagens/lilac.jpg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Limited,
                    SolistaId = iuSolista.Id
                };
                context.Albuns.Add(lilac);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "LILAC", Duracao = "3:34", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = lilac.Id },
                    new Musica { Titulo = "Celebrity", Duracao = "3:15", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = lilac.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 5. Stray Kids (SKZ) -- NOVO!
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "Stray Kids"))
            {
                var skz = new Grupo
                {
                    Nome = "Stray Kids",
                    DataEstreia = new DateTime(2018, 3, 25),
                    Companhia = "JYP Entertainment",
                    Fansigno = "STAY",
                    ImagemUrl = "/imagens/stray-kids.png",
                    IsAtivo = true
                };
                context.Grupos.Add(skz);
                await context.SaveChangesAsync();

                var felix = new Artista
                {
                    Nome = "Lee Felix",
                    NomeArtistico = "Felix",
                    DataNascimento = new DateTime(2000, 9, 15),
                    Posicao = "Dançarino Líder, Rapper Líder",
                    Nacionalidade = "Australiana",
                    ImagemUrl = "/imagens/Felix.png",
                    DataEntrada = new DateTime(2018, 3, 25),
                    IsAtivo = true,
                    GrupoId = skz.Id
                };

                var bangchan = new Artista
                {
                    Nome = "Christopher Bang",
                    NomeArtistico = "Bang Chan",
                    DataNascimento = new DateTime(1997, 10, 3),
                    Posicao = "Líder, Produtor, Rapper, Vocalista",
                    Nacionalidade = "Australiana",
                    ImagemUrl = "/imagens/BangChan.png",
                    DataEntrada = new DateTime(2018, 3, 25),
                    IsAtivo = true,
                    GrupoId = skz.Id
                };

                context.Artistas.AddRange(felix, bangchan);
                await context.SaveChangesAsync();

                var fiveStar = new Album
                {
                    Titulo = "★★★★★ (5-STAR)",
                    DataLancamento = new DateTime(2023, 6, 2),
                    CapaUrl = "/imagens/5star.png",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = skz.Id
                };
                context.Albuns.Add(fiveStar);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "S-Class", Duracao = "3:16", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = fiveStar.Id },
                    new Musica { Titulo = "Super Bowl", Duracao = "3:06", TrackNumber = 2, IsSingle = false, IsTitleTrack = false, AlbumId = fiveStar.Id },
                    new Musica { Titulo = "TOPLINE (feat. Tiger JK)", Duracao = "3:24", TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = fiveStar.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 6. ENHYPEN -- NOVO!
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "ENHYPEN"))
            {
                var enhypen = new Grupo
                {
                    Nome = "ENHYPEN",
                    DataEstreia = new DateTime(2020, 11, 30),
                    Companhia = "Belift Lab (HYBE)",
                    Fansigno = "ENGENE",
                    ImagemUrl = "https://images.unsplash.com/photo-1557672172-298e090bd0f1?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                context.Grupos.Add(enhypen);
                await context.SaveChangesAsync();

                var jungwon = new Artista
                {
                    Nome = "Yang Jung-won",
                    NomeArtistico = "Jungwon",
                    DataNascimento = new DateTime(2004, 2, 9),
                    Posicao = "Líder, Vocalista, Dançarino",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jungwon.jpeg",
                    DataEntrada = new DateTime(2020, 11, 30),
                    IsAtivo = true,
                    GrupoId = enhypen.Id
                };

                var niki = new Artista
                {
                    Nome = "Nishimura Riki",
                    NomeArtistico = "Ni-ki",
                    DataNascimento = new DateTime(2005, 12, 9),
                    Posicao = "Dançarino Principal, Vocalista, Maknae",
                    Nacionalidade = "Japonesa",
                    ImagemUrl = "/imagens/Niki.jpeg",
                    DataEntrada = new DateTime(2020, 11, 30),
                    IsAtivo = true,
                    GrupoId = enhypen.Id
                };

                context.Artistas.AddRange(jungwon, niki);
                await context.SaveChangesAsync();

                var darkBlood = new Album
                {
                    Titulo = "DARK BLOOD",
                    DataLancamento = new DateTime(2023, 5, 22),
                    CapaUrl = "/imagens/enhypen.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Limited,
                    GrupoId = enhypen.Id
                };
                context.Albuns.Add(darkBlood);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "Bite Me", Duracao = "2:37", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = darkBlood.Id },
                    new Musica { Titulo = "Sacrifice (Eat Me Up)", Duracao = "3:22", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = darkBlood.Id },
                    new Musica { Titulo = "Chaconne", Duracao = "2:59", TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = darkBlood.Id }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 7. ZEROBASEONE (ZB1) -- NOVO!
            // ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "ZEROBASEONE"))
            {
                var zb1 = new Grupo
                {
                    Nome = "ZEROBASEONE",
                    DataEstreia = new DateTime(2023, 7, 10),
                    Companhia = "WakeOne Entertainment",
                    Fansigno = "ZEROSE",
                    ImagemUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                context.Grupos.Add(zb1);
                await context.SaveChangesAsync();

                var zhanghao = new Artista
                {
                    Nome = "Zhang Hao",
                    NomeArtistico = "Zhang Hao",
                    DataNascimento = new DateTime(2000, 7, 25),
                    Posicao = "Centro, Vocalista Principal",
                    Nacionalidade = "Chinesa",
                    ImagemUrl = "/imagens/ZhangHao.jpeg",
                    DataEntrada = new DateTime(2023, 7, 10),
                    IsAtivo = true,
                    GrupoId = zb1.Id
                };

                context.Artistas.Add(zhanghao);
                await context.SaveChangesAsync();

                var youthInShade = new Album
                {
                    Titulo = "YOUTH IN THE SHADE",
                    DataLancamento = new DateTime(2023, 7, 10),
                    CapaUrl = "/imagens/zb1.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = zb1.Id
                };
                context.Albuns.Add(youthInShade);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "In Bloom", Duracao = "3:00", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = youthInShade.Id },
                    new Musica { Titulo = "New Kidz on the Block", Duracao = "3:02", TrackNumber = 2, IsSingle = false, IsTitleTrack = false, AlbumId = youthInShade.Id }
                );
                await context.SaveChangesAsync();
            }

            // 8. NMIXX ==========================================
            if (!await context.Grupos.AnyAsync(g => g.Nome == "NMIXX"))
            {
                var nmixx = new Grupo
                {
                    Nome = "NMIXX",
                    DataEstreia = new DateTime(2022, 2, 22),
                    Companhia = "JYP Entertainment",
                    Fansigno = "NSWER",
                    ImagemUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                context.Grupos.Add(nmixx);
                await context.SaveChangesAsync();

                var haewon = new Artista
                {
                    Nome = "Oh Hae-won",
                    NomeArtistico = "Haewon",
                    DataNascimento = new DateTime(2003, 2, 25),
                    Posicao = "Líder, Vocalista Principal",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Haewon.jpeg",
                    DataEntrada = new DateTime(2022, 2, 22),
                    IsAtivo = true,
                    GrupoId = nmixx.Id
                };

                var sullyoon = new Artista
                {
                    Nome = "Seol Yoon-a",
                    NomeArtistico = "Sullyoon",
                    DataNascimento = new DateTime(2004, 1, 26),
                    Posicao = "Vocalista, Dançarina, Visual",
                    Nacionalidade = "Sul-Coreana",
                    ImagemUrl = "/imagens/Sullyoon.jpeg",
                    DataEntrada = new DateTime(2022, 2, 22),
                    IsAtivo = true,
                    GrupoId = nmixx.Id
                };

                context.Artistas.AddRange(haewon, sullyoon);
                await context.SaveChangesAsync();

                var breakAlbum = new Album
                {
                    Titulo = "Fe3O4: BREAK",
                    DataLancamento = new DateTime(2024, 1, 15),
                    CapaUrl = "/imagens/nmixx.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Photobook,
                    GrupoId = nmixx.Id
                };
                context.Albuns.Add(breakAlbum);
                await context.SaveChangesAsync();

                context.Musicas.AddRange(
                    new Musica { Titulo = "DASH", Duracao = "2:46", TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = breakAlbum.Id },
                    new Musica { Titulo = "Soñar (Breaker)", Duracao = "2:55", TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = breakAlbum.Id }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
