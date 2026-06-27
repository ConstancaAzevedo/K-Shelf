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
    /// classe utilitaria responsavel por semear (popular) a base de dados com dados iniciais de teste
    /// cria grupos, solistas, artistas, albuns e musicas se estes ainda nao existirem no sistema
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// semeia a base de dados de forma assincrona
        /// garante a criacao fisica da base de dados e insere registos de varios artistas famosos de k-pop
        /// </summary>
        /// <param name="context">o contexto da base de dados da aplicacao</param>

        // metodo auxiliar para converter string "mm:ss" em timespan
        private static TimeSpan? ParseDuracao(string duracao)
        {
            // se a string for vazia ou nula, retorna null
            if (string.IsNullOrEmpty(duracao)) return null;
            // tenta fazer parse no formato mm:ss
            if (TimeSpan.TryParseExact(duracao, @"mm\:ss", System.Globalization.CultureInfo.InvariantCulture, out var result))
                return result;
            // tenta fazer parse generico como fallback
            if (TimeSpan.TryParse(duracao, out var result2))
                return result2;
            // se nenhum formato funcionar, retorna null
            return null;
        }

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // certifica que a base de dados existe e esta criada fisicamente na maquina
            await context.Database.EnsureCreatedAsync();

            // bts & agust d (membros e albuns relacionados)
            // verifica se o grupo bts ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "BTS"))
            {
                // cria o grupo bts
                var bts = new Grupo
                {
                    Nome = "BTS",
                    DataEstreia = new DateTime(2013, 6, 13),
                    Companhia = "Big Hit Music (HYBE)",
                    Fansigno = "ARMY",
                    ImagemUrl = "/imagens/.jpeg",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(bts);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // verifica se o solista agust d ja existe
                var agustD = await context.Solistas.FirstOrDefaultAsync(s => s.Nome == "Agust D");
                if (agustD == null)
                {
                    // cria o solista agust d
                    agustD = new Solista
                    {
                        Nome = "Agust D",
                        DataEstreia = new DateTime(2016, 8, 15),
                        Companhia = "Big Hit Music (HYBE)",
                        ImagemUrl = "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?q=80&w=600&auto=format&fit=crop",
                        IsAtivo = true
                    };
                    // adiciona o solista ao contexto
                    context.Solistas.Add(agustD);
                    // guarda as alteracoes
                    await context.SaveChangesAsync();
                }

                // cria o artista jungkook
                var jungkook = new Artista
                {
                    Nome = "Jeon Jung-kook",
                    NomeArtistico = "Jungkook",
                    DataNascimento = new DateTime(1997, 9, 1),
                    Posicao = "Vocalista Principal, Dançarino, Centro",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jungkook.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id
                };

                // cria o artista rm
                var rm = new Artista
                {
                    Nome = "Kim Nam-joon",
                    NomeArtistico = "RM",
                    DataNascimento = new DateTime(1994, 9, 12),
                    Posicao = "Líder, Rapper Principal",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/RM.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id
                };

                // cria o artista suga
                var suga = new Artista
                {
                    Nome = "Min Yoon-gi",
                    NomeArtistico = "Suga",
                    DataNascimento = new DateTime(1993, 3, 9),
                    Posicao = "Rapper Líder",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Suga.jpeg",
                    DataEntrada = new DateTime(2013, 6, 13),
                    IsAtivo = true,
                    GrupoId = bts.Id,
                    SolistaId = agustD.Id
                };

                // adiciona os artistas ao contexto
                context.Artistas.AddRange(jungkook, rm, suga);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album map of the soul: 7
                var mapOfTheSoul = new Album
                {
                    Titulo = "Map of the Soul: 7",
                    DataLancamento = new DateTime(2020, 2, 21),
                    CapaUrl = "/imagens/mapalbum.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = bts.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(mapOfTheSoul);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album d-day
                var dday = new Album
                {
                    Titulo = "D-DAY",
                    DataLancamento = new DateTime(2023, 4, 21),
                    CapaUrl = "/imagens/agustd.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.JewelCase,
                    SolistaId = agustD.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(dday);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas dos albuns
                context.Musicas.AddRange(
                    new Musica { Titulo = "ON", Duracao = ParseDuracao("4:06"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = mapOfTheSoul.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3" },
                    new Musica { Titulo = "Black Swan", Duracao = ParseDuracao("3:18"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = mapOfTheSoul.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3" },
                    new Musica { Titulo = "Filter", Duracao = ParseDuracao("3:00"), TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = mapOfTheSoul.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3" },
                    new Musica { Titulo = "Haegeum", Duracao = ParseDuracao("2:48"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = dday.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3" },
                    new Musica { Titulo = "People Pt.2 (feat. IU)", Duracao = ParseDuracao("3:33"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = dday.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // blackpink
            // verifica se o grupo blackpink ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "BLACKPINK"))
            {
                // cria o grupo blackpink
                var bp = new Grupo
                {
                    Nome = "BLACKPINK",
                    DataEstreia = new DateTime(2016, 8, 8),
                    Companhia = "YG Entertainment",
                    Fansigno = "BLINK",
                    ImagemUrl = "https://images.unsplash.com/photo-1598387181032-a3103a2db5b3?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(bp);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista jennie
                var jennie = new Artista
                {
                    Nome = "Jennie Kim",
                    NomeArtistico = "Jennie",
                    DataNascimento = new DateTime(1996, 1, 16),
                    Posicao = "Rapper Principal, Vocalista Líder",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jenni.jpeg",
                    DataEntrada = new DateTime(2016, 8, 8),
                    IsAtivo = true,
                    GrupoId = bp.Id
                };

                // cria o artista lisa
                var lisa = new Artista
                {
                    Nome = "Lalisa Manobal",
                    NomeArtistico = "Lisa",
                    DataNascimento = new DateTime(1997, 3, 27),
                    Posicao = "Dançarina Principal, Rapper Líder",
                    Pais = "Tailandesa",
                    ImagemUrl = "/imagens/Lisa.jpeg",
                    DataEntrada = new DateTime(2016, 8, 8),
                    IsAtivo = true,
                    GrupoId = bp.Id
                };

                // adiciona os artistas ao contexto
                context.Artistas.AddRange(jennie, lisa);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album the album
                var theAlbum = new Album
                {
                    Titulo = "The Album",
                    DataLancamento = new DateTime(2020, 10, 2),
                    CapaUrl = "/imagens/thealbum.jpeg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Special,
                    GrupoId = bp.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(theAlbum);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "Lovesick Girls", Duracao = ParseDuracao("3:12"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = theAlbum.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3" },
                    new Musica { Titulo = "How You Like That", Duracao = ParseDuracao("3:01"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = theAlbum.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-7.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // newjeans
            // verifica se o grupo newjeans ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "NewJeans"))
            {
                // cria o grupo newjeans
                var nj = new Grupo
                {
                    Nome = "NewJeans",
                    DataEstreia = new DateTime(2022, 7, 22),
                    Companhia = "ADOR (HYBE)",
                    Fansigno = "Bunnies",
                    ImagemUrl = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(nj);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista hanni
                var hanni = new Artista
                {
                    Nome = "Hanni Pham",
                    NomeArtistico = "Hanni",
                    DataNascimento = new DateTime(2004, 10, 6),
                    Posicao = "Vocalista Líder, Dançarina Líder",
                    Pais = "Vietnamita-Australiana",
                    ImagemUrl = "/imagens/Hanni.webp",
                    DataEntrada = new DateTime(2022, 7, 22),
                    IsAtivo = true,
                    GrupoId = nj.Id
                };
                // adiciona o artista ao contexto
                context.Artistas.Add(hanni);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album get up
                var getUp = new Album
                {
                    Titulo = "Get Up",
                    DataLancamento = new DateTime(2023, 7, 21),
                    CapaUrl = "/imagens/newjeans.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Platform,
                    GrupoId = nj.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(getUp);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "Super Shy", Duracao = ParseDuracao("2:34"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = getUp.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-8.mp3" },
                    new Musica { Titulo = "ETA", Duracao = ParseDuracao("2:31"), TrackNumber = 2, IsSingle = true, IsTitleTrack = true, AlbumId = getUp.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-9.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // iu
            // verifica se o solista iu ja existe
            if (!await context.Solistas.AnyAsync(s => s.Nome == "IU"))
            {
                // cria o solista iu
                var iuSolista = new Solista
                {
                    Nome = "IU",
                    DataEstreia = new DateTime(2008, 9, 18),
                    Companhia = "EDAM Entertainment",
                    ImagemUrl = "/imagens/IU.jpeg",
                    IsAtivo = true
                };
                // adiciona o solista ao contexto
                context.Solistas.Add(iuSolista);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista iu
                var iuArtista = new Artista
                {
                    Nome = "Lee Ji-eun",
                    NomeArtistico = "IU",
                    DataNascimento = new DateTime(1993, 5, 16),
                    Posicao = "Vocalista Principal, Compositora",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/IU.jpeg",
                    DataEntrada = new DateTime(2008, 9, 18),
                    IsAtivo = true,
                    SolistaId = iuSolista.Id
                };
                // adiciona o artista ao contexto
                context.Artistas.Add(iuArtista);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album lilac
                var lilac = new Album
                {
                    Titulo = "LILAC",
                    DataLancamento = new DateTime(2021, 3, 25),
                    CapaUrl = "/imagens/lilac.jpg",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Limited,
                    SolistaId = iuSolista.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(lilac);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "LILAC", Duracao = ParseDuracao("3:34"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = lilac.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-10.mp3" },
                    new Musica { Titulo = "Celebrity", Duracao = ParseDuracao("3:15"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = lilac.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-11.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // stray kids (skz)
            // verifica se o grupo stray kids ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "Stray Kids"))
            {
                // cria o grupo stray kids
                var skz = new Grupo
                {
                    Nome = "Stray Kids",
                    DataEstreia = new DateTime(2018, 3, 25),
                    Companhia = "JYP Entertainment",
                    Fansigno = "STAY",
                    ImagemUrl = "/imagens/stray-kids.png",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(skz);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista felix
                var felix = new Artista
                {
                    Nome = "Lee Felix",
                    NomeArtistico = "Felix",
                    DataNascimento = new DateTime(2000, 9, 15),
                    Posicao = "Dançarino Líder, Rapper Líder",
                    Pais = "Australiana",
                    ImagemUrl = "/imagens/Felix.png",
                    DataEntrada = new DateTime(2018, 3, 25),
                    IsAtivo = true,
                    GrupoId = skz.Id
                };

                // cria o artista bang chan
                var bangchan = new Artista
                {
                    Nome = "Christopher Bang",
                    NomeArtistico = "Bang Chan",
                    DataNascimento = new DateTime(1997, 10, 3),
                    Posicao = "Líder, Produtor, Rapper, Vocalista",
                    Pais = "Australiana",
                    ImagemUrl = "/imagens/BangChan.png",
                    DataEntrada = new DateTime(2018, 3, 25),
                    IsAtivo = true,
                    GrupoId = skz.Id
                };

                // adiciona os artistas ao contexto
                context.Artistas.AddRange(felix, bangchan);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album 5-star
                var fiveStar = new Album
                {
                    Titulo = "★★★★★ (5-STAR)",
                    DataLancamento = new DateTime(2023, 6, 2),
                    CapaUrl = "/imagens/5star.png",
                    Tipo = Album.TipoAlbum.Studio,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = skz.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(fiveStar);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "S-Class", Duracao = ParseDuracao("3:16"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = fiveStar.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-12.mp3" },
                    new Musica { Titulo = "Super Bowl", Duracao = ParseDuracao("3:06"), TrackNumber = 2, IsSingle = false, IsTitleTrack = false, AlbumId = fiveStar.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-13.mp3" },
                    new Musica { Titulo = "TOPLINE (feat. Tiger JK)", Duracao = ParseDuracao("3:24"), TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = fiveStar.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-14.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // enhypen
            // verifica se o grupo enhypen ja existe
            var enhypen = await context.Grupos.FirstOrDefaultAsync(g => g.Nome == "ENHYPEN");
            if (enhypen == null)
            {
                // cria o grupo enhypen
                enhypen = new Grupo
                {
                    Nome = "ENHYPEN",
                    DataEstreia = new DateTime(2020, 11, 30),
                    Companhia = "Belift Lab (HYBE)",
                    Fansigno = "ENGENE",
                    ImagemUrl = "https://images.unsplash.com/photo-1557672172-298e090bd0f1?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(enhypen);
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // verifica se o artista jungwon ja existe
            var jungwon = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Jungwon" && a.GrupoId == enhypen.Id);
            if (jungwon == null)
            {
                // cria o artista jungwon
                jungwon = new Artista
                {
                    Nome = "Yang Jung-won",
                    NomeArtistico = "Jungwon",
                    DataNascimento = new DateTime(2004, 2, 9),
                    Posicao = "Líder, Vocalista, Dançarino",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Jungwon.jpeg",
                    DataEntrada = new DateTime(2020, 11, 30),
                    IsAtivo = true,
                    GrupoId = enhypen.Id
                };
                // adiciona o artista ao contexto
                context.Artistas.Add(jungwon);
            }

            // verifica se o artista ni-ki ja existe
            var niki = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Ni-ki" && a.GrupoId == enhypen.Id);
            if (niki == null)
            {
                // cria o artista ni-ki
                niki = new Artista
                {
                    Nome = "Nishimura Riki",
                    NomeArtistico = "Ni-ki",
                    DataNascimento = new DateTime(2005, 12, 9),
                    Posicao = "Dançarino Principal, Vocalista, Maknae",
                    Pais = "Japonesa",
                    ImagemUrl = "/imagens/Niki.jpeg",
                    DataEntrada = new DateTime(2020, 11, 30),
                    IsAtivo = true,
                    GrupoId = enhypen.Id
                };
                // adiciona o artista ao contexto
                context.Artistas.Add(niki);
            }

            // verifica se o artista jake ja 
            var jake = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Jake" && a.GrupoId == enhypen.Id);
            if (jake == null)
            {
                // cria o artista 
                jake = new Artista
                {
                    Nome = "Sim Jae-yun",
                    NomeArtistico = "Jake",
                    DataNascimento = new DateTime(2002, 11, 15),
                    Posicao = "Vocalista, Rapper, Dançarino",
                    Pais = "Australiana",
                    ImagemUrl = "/imagens/jakephoto.jpeg",
                    DataEntrada = new DateTime(2020, 11, 30),
                    IsAtivo = true,
                    GrupoId = enhypen.Id
                };
                // adiciona o artista ao contexto
                context.Artistas.Add(jake);
            }
            // guarda as alteracoes
            await context.SaveChangesAsync();

            // verifica se o album dark blood ja existe
            var darkBlood = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "DARK BLOOD" && a.GrupoId == enhypen.Id);
            if (darkBlood == null)
            {
                // cria o album dark blood
                darkBlood = new Album
                {
                    Titulo = "DARK BLOOD",
                    DataLancamento = new DateTime(2023, 5, 22),
                    CapaUrl = "/imagens/enhypen.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Limited,
                    GrupoId = enhypen.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(darkBlood);
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // verifica se o album dark blood ja tem musicas
            if (!await context.Musicas.AnyAsync(m => m.AlbumId == darkBlood.Id))
            {
                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "Bite Me", Duracao = ParseDuracao("2:37"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = darkBlood.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-15.mp3" },
                    new Musica { Titulo = "Sacrifice (Eat Me Up)", Duracao = ParseDuracao("3:22"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = darkBlood.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-16.mp3" },
                    new Musica { Titulo = "Chaconne", Duracao = ParseDuracao("2:59"), TrackNumber = 3, IsSingle = false, IsTitleTrack = false, AlbumId = darkBlood.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // ZEROBASEONE (ZB1) 
            // verifica se o grupo zerobaseone ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "ZEROBASEONE"))
            {
                // cria o grupo zerobaseone
                var zb1 = new Grupo
                {
                    Nome = "ZEROBASEONE",
                    DataEstreia = new DateTime(2023, 7, 10),
                    Companhia = "WakeOne Entertainment",
                    Fansigno = "ZEROSE",
                    ImagemUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(zb1);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista zhang hao
                var zhanghao = new Artista
                {
                    Nome = "Zhang Hao",
                    NomeArtistico = "Zhang Hao",
                    DataNascimento = new DateTime(2000, 7, 25),
                    Posicao = "Centro, Vocalista Principal",
                    Pais = "Chinesa",
                    ImagemUrl = "/imagens/ZhangHao.jpeg",
                    DataEntrada = new DateTime(2023, 7, 10),
                    IsAtivo = true,
                    GrupoId = zb1.Id
                };

                // adiciona o artista ao contexto
                context.Artistas.Add(zhanghao);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album youth in the shade
                var youthInShade = new Album
                {
                    Titulo = "YOUTH IN THE SHADE",
                    DataLancamento = new DateTime(2023, 7, 10),
                    CapaUrl = "/imagens/zb1.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Standard,
                    GrupoId = zb1.Id
                };
                // adiciona o album ao contexto
                context.Albuns.Add(youthInShade);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "In Bloom", Duracao = ParseDuracao("3:00"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = youthInShade.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3" },
                    new Musica { Titulo = "New Kidz on the Block", Duracao = ParseDuracao("3:02"), TrackNumber = 2, IsSingle = false, IsTitleTrack = false, AlbumId = youthInShade.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // NMIXX 
            // verifica se o grupo nmixx ja existe
            if (!await context.Grupos.AnyAsync(g => g.Nome == "NMIXX"))
            {
                // cria o grupo nmixx
                var nmixx = new Grupo
                {
                    Nome = "NMIXX",
                    DataEstreia = new DateTime(2022, 2, 22),
                    Companhia = "JYP Entertainment",
                    Fansigno = "NSWER",
                    ImagemUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?q=80&w=600&auto=format&fit=crop",
                    IsAtivo = true
                };
                // adiciona o grupo ao contexto
                context.Grupos.Add(nmixx);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o artista haewon
                var haewon = new Artista
                {
                    Nome = "Oh Hae-won",
                    NomeArtistico = "Haewon",
                    DataNascimento = new DateTime(2003, 2, 25),
                    Posicao = "Líder, Vocalista Principal",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Haewon.jpeg",
                    DataEntrada = new DateTime(2022, 2, 22),
                    IsAtivo = true,
                    GrupoId = nmixx.Id
                };

                // cria a artista sullyoon
                var sullyoon = new Artista
                {
                    Nome = "Seol Yoon-a",
                    NomeArtistico = "Sullyoon",
                    DataNascimento = new DateTime(2004, 1, 26),
                    Posicao = "Vocalista, Dançarina, Visual",
                    Pais = "Sul-Coreana",
                    ImagemUrl = "/imagens/Sullyoon.jpeg",
                    DataEntrada = new DateTime(2022, 2, 22),
                    IsAtivo = true,
                    GrupoId = nmixx.Id
                };

                // adiciona os artistas ao contexto
                context.Artistas.AddRange(haewon, sullyoon);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // cria o album fe3o4: break
                var breakAlbum = new Album
                {
                    Titulo = "Fe3O4: BREAK",
                    DataLancamento = new DateTime(2024, 1, 15),
                    CapaUrl = "/imagens/nmixx.jpeg",
                    Tipo = Album.TipoAlbum.EP,
                    Edicao = Album.EdicaoAlbum.Photobook,
                    GrupoId = nmixx.Id
                };

                // adiciona o album ao contexto
                context.Albuns.Add(breakAlbum);
                // guarda as alteracoes
                await context.SaveChangesAsync();

                // adiciona as musicas do album
                context.Musicas.AddRange(
                    new Musica { Titulo = "DASH", Duracao = ParseDuracao("2:46"), TrackNumber = 1, IsSingle = true, IsTitleTrack = true, AlbumId = breakAlbum.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3" },
                    new Musica { Titulo = "Soñar (Breaker)", Duracao = ParseDuracao("2:55"), TrackNumber = 2, IsSingle = true, IsTitleTrack = false, AlbumId = breakAlbum.Id, PreviewAudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3" }
                );
                // guarda as alteracoes
                await context.SaveChangesAsync();
            }

            // Seeding Photocards
            // obtem os artistas necessarios para os photocards
            var photoJungkook = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Jungkook");
            var photoHanni = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Hanni");
            var photoFelix = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Felix");
            var photoJennie = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Jennie");
            var photoJake = await context.Artistas.FirstOrDefaultAsync(a => a.NomeArtistico == "Jake");

            // obtem os albuns necessarios para os photocards
            var albumMapOfTheSoul = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "Map of the Soul: 7");
            var albumGetUp = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "Get Up");
            var albumFiveStar = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "★★★★★ (5-STAR)" || a.Titulo == "????? (5-STAR)");
            var albumTheAlbum = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "The Album");
            var albumDarkBlood = await context.Albuns.FirstOrDefaultAsync(a => a.Titulo == "DARK BLOOD");

            // metodo local auxiliar para assegurar a criacao do photocard de forma idempotente
            async Task EnsurePhotocardAsync(string versao, string imagemUrl, int artistaId, int? albumId)
            {
                // verifica se o photocard ja existe
                if (!await context.Photocards.AnyAsync(p => p.Versao == versao && p.ArtistaId == artistaId))
                {
                    // adiciona o photocard ao contexto
                    context.Photocards.Add(new Photocard
                    {
                        Versao = versao,
                        ImagemUrl = imagemUrl,
                        ArtistaId = artistaId,
                        AlbumId = albumId
                    });
                }
            }

            // cria os photocards para o jungkook
            if (photoJungkook != null)
            {
                await EnsurePhotocardAsync("Selfie Ver. 1", "/imagens/jkphoto.png", photoJungkook.Id, albumMapOfTheSoul?.Id);
                await EnsurePhotocardAsync("Concept Photo Black Swan", "/imagens/jkphoto1.jpg", photoJungkook.Id, albumMapOfTheSoul?.Id);
            }

            // cria os photocards para a hanni
            if (photoHanni != null)
            {
                await EnsurePhotocardAsync("Bunnies Beach Bag Ver. Hanni", "/imagens/hanniphoto.png", photoHanni.Id, albumGetUp?.Id);
                await EnsurePhotocardAsync("ETA Concept Card", "/imagens/hanniphoto1.jpg", photoHanni.Id, albumGetUp?.Id);
            }

            // cria os photocards para o felix
            if (photoFelix != null)
            {
                await EnsurePhotocardAsync("Limited Edition S-Class Selfie", "/imagens/felixphoto.png", photoFelix.Id, albumFiveStar?.Id);
                await EnsurePhotocardAsync("Soundwave POB (Pre-Order)", "/imagens/felixphoto22.jpg", photoFelix.Id, albumFiveStar?.Id);
            }

            // cria os photocards para a jennie
            if (photoJennie != null)
            {
                await EnsurePhotocardAsync("Pink Ice Cream Selfie", "/imagens/jenniephoto.jpg", photoJennie.Id, albumTheAlbum?.Id);
            }

            // cria os photocards para o jake
            if (photoJake != null)
            {
                await EnsurePhotocardAsync("Dark Blood Orange Ver.", "/imagens/jakephoto.jpeg", photoJake.Id, albumDarkBlood?.Id);
            }
            // guarda as alteracoes
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// pesquisa e atualiza os links de preview de audio reais das musicas a partir do itunes
        /// </summary>
        public static async Task FetchItunesPreviewsAsync(ApplicationDbContext context)
        {
            // procura todas as musicas que nao tem audio ou que usam o soundhelix de teste
            var musicas = await context.Musicas
                .Include(m => m.Album)
                    .ThenInclude(a => a!.Grupo)
                .Include(m => m.Album)
                    .ThenInclude(a => a!.Solista)
                .Where(m => string.IsNullOrEmpty(m.PreviewAudioUrl) || m.PreviewAudioUrl.Contains("soundhelix.com"))
                .ToListAsync();

            // se nao houver musicas para atualizar, sai do metodo
            if (!musicas.Any()) return;

            // cria um cliente http para fazer as requisicoes
            using var httpClient = new HttpClient();
            // adiciona um user agent para evitar bloqueios
            httpClient.DefaultRequestHeaders.Add("User-Agent", "K-Shelf-App");

            // percorre cada musica
            foreach (var musica in musicas)
            {
                // obtem o nome do artista a partir do album
                var artistaNome = musica.Album?.Grupo?.Nome ?? musica.Album?.Solista?.Nome ?? "";
                // se nao houver nome do artista, passa para a 
                if (string.IsNullOrEmpty(artistaNome)) continue;

                try
                {
                    // constroi a query para a api do itunes
                    var query = Uri.EscapeDataString($"{artistaNome} {musica.Titulo}");
                    var url = $"https://itunes.apple.com/search?term={query}&media=music&entity=song&limit=1";

                    // faz a requisicao a api do itunes
                    var response = await httpClient.GetStringAsync(url);
                    // faz o parse da resposta json
                    using var doc = System.Text.Json.JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    // verifica se ha resultados
                    if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                    {
                        // obtem o primeiro resultado
                        var firstResult = results[0];
                        // verifica se existe o campo previewurl
                        if (firstResult.TryGetProperty("previewUrl", out var previewUrl))
                        {
                            // atualiza o previewurl da musica
                            musica.PreviewAudioUrl = previewUrl.GetString();
                        }
                    }

                    // pequeno atraso para respeitar os limites de taxa da api do itunes
                    await Task.Delay(300);
                }
                catch
                {
                    // falha silenciosa para nao interromper a inicializacao
                }
            }
            // guarda as alteracoes na base de dados
            await context.SaveChangesAsync();
        }
    }
}
