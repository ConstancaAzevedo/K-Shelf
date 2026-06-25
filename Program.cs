using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAÇÃO DE SERVIÇOS (Dependency Injection)

// Configuração do contexto de Base de Dados (ApplicationDbContext) a usar SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do ASP.NET Core Identity com definições personalizadas de segurança
builder.Services.AddDefaultIdentity<Utilizador>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Não exige confirmação de email
    options.User.RequireUniqueEmail = true;         // Exige email único por utilizador
    options.Password.RequireDigit = false;          // Não exige números na senha
    options.Password.RequiredLength = 6;            // Comprimento mínimo de 6 caracteres
    options.Password.RequireNonAlphanumeric = false;// Não exige caracteres especiais
    options.Password.RequireUppercase = false;       // Não exige letras maiúsculas
    options.Password.RequireLowercase = false;       // Não exige letras minúsculas
})
.AddRoles<IdentityRole>() // Ativa suporte a Roles (Cargos como Admin/User)
.AddEntityFrameworkStores<ApplicationDbContext>();

// Ativar suporte para API Controllers (MVC) e Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Ativar suporte para comunicação em tempo real com SignalR
builder.Services.AddSignalR();

// Configuração do Swagger para documentação automática da API REST
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "K-Shelf API",
        Version = "v1",
        Description = "API REST para gestão de coleções de K-Pop. Permite gerir Artistas, Álbuns e Coleções.",
        Contact = new OpenApiContact
        {
            Name = "Constança Azevedo & Rui Dias",
            Email = "admin@kshelf.com"
        }
    });

    // Incluir ficheiro de documentação XML gerado pelo compilador no Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();


// 2. CONFIGURAÇÃO DO PIPELINE DE PEDIDOS HTTP (Middlewares)

if (app.Environment.IsDevelopment())
{
    // Em desenvolvimento, ativa tratamento detalhado de erros e base de dados
    app.UseMigrationsEndPoint();

    // Ativar a interface visual do Swagger no endpoint /swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "K-Shelf API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    // Em produção, usa tratamento genérico de erros e HSTS para conexões seguras
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Intercetar códigos de erro como 404 e 403 e reencaminhar para a nossa página de erro customizada
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles(); // Suporte para servir ficheiros estáticos (HTML, CSS, Imagens, JS) em wwwroot
app.UseRouting();

// Ativar middlewares de segurança: autenticação de identidade e autorização de permissões
app.UseAuthentication();
app.UseAuthorization();

// Configuração das rotas padrão para os Controllers de API e Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


// Mapear o endpoint do Hub do SignalR para o Chat e Contador de utilizadores
app.MapHub<K_Shelf.Hubs.KpopChatHub>("/kpopChatHub");

// 3. INICIALIZAÇÃO E ALIMENTAÇÃO DA BASE DE DADOS (Seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // --- AUTOCREATE PHOTOCARD TABLES ---
        try
        {
            // 1. Criar a tabela de histórico de migrações se não existir
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY,
                        [ProductVersion] nvarchar(32) NOT NULL
                    );
                END");

            // 2. Registar todas as migrações padrão e a nova de photocards como 'aplicadas'
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '00000000000000_CreateIdentitySchema')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('00000000000000_CreateIdentitySchema', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260318112154_InitialCreate')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260318112154_InitialCreate', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260527093034_InitialIdentity')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260527093034_InitialIdentity', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260527105228_Artistas')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260527105228_Artistas', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260603095412_CriarTabelasNovas')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260603095412_CriarTabelasNovas', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260625181812_AddAlbunsToArtista')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260625181812_AddAlbunsToArtista', '8.0.0');
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260625215000_AddPhotocardModels')
                    INSERT INTO [__EFMigrationsHistory] VALUES ('20260625215000_AddPhotocardModels', '8.0.0');
            ");

            // 3. Criar a tabela Photocards caso não exista
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'[Photocards]') IS NULL
                BEGIN
                    CREATE TABLE [Photocards] (
                        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Photocards] PRIMARY KEY,
                        [Versao] nvarchar(100) NOT NULL,
                        [ImagemUrl] nvarchar(max) NOT NULL,
                        [ArtistaId] int NOT NULL CONSTRAINT [FK_Photocards_Artistas_ArtistaId] REFERENCES [Artistas]([Id]) ON DELETE CASCADE,
                        [AlbumId] int NULL CONSTRAINT [FK_Photocards_Albuns_AlbumId] REFERENCES [Albuns]([Id]) ON DELETE NO ACTION
                    );
                END");

            // 4. Criar a tabela UtilizadorPhotocards caso não exista
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'[UtilizadorPhotocards]') IS NULL
                BEGIN
                    CREATE TABLE [UtilizadorPhotocards] (
                        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UtilizadorPhotocards] PRIMARY KEY,
                        [UtilizadorId] nvarchar(450) NOT NULL CONSTRAINT [FK_UtilizadorPhotocards_AspNetUsers_UtilizadorId] REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
                        [PhotocardId] int NOT NULL CONSTRAINT [FK_UtilizadorPhotocards_Photocards_PhotocardId] REFERENCES [Photocards]([Id]) ON DELETE CASCADE,
                        [Estado] int NOT NULL,
                        [Quantidade] int NOT NULL,
                        [Notas] nvarchar(200) NULL
                    );
                END");

            // 5. Criar índices caso não existam
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Photocards_ArtistaId' AND object_id = OBJECT_ID('[Photocards]'))
                    CREATE INDEX [IX_Photocards_ArtistaId] ON [Photocards] ([ArtistaId]);
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Photocards_AlbumId' AND object_id = OBJECT_ID('[Photocards]'))
                    CREATE INDEX [IX_Photocards_AlbumId] ON [Photocards] ([AlbumId]);
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UtilizadorPhotocards_UtilizadorId' AND object_id = OBJECT_ID('[UtilizadorPhotocards]'))
                    CREATE INDEX [IX_UtilizadorPhotocards_UtilizadorId] ON [UtilizadorPhotocards] ([UtilizadorId]);
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UtilizadorPhotocards_PhotocardId' AND object_id = OBJECT_ID('[UtilizadorPhotocards]'))
                    CREATE INDEX [IX_UtilizadorPhotocards_PhotocardId] ON [UtilizadorPhotocards] ([PhotocardId]);
            ");

            // 6. Atualizar as URLs dos photocards de sementeira com as novas imagens locais da pasta
            await context.Database.ExecuteSqlRawAsync(@"
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/jkphoto.png' WHERE [Versao] = 'Selfie Ver. 1';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/jkphoto1.jpg' WHERE [Versao] = 'Concept Photo Black Swan';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/hanniphoto.png' WHERE [Versao] = 'Bunnies Beach Bag Ver. Hanni';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/hanniphoto1.jpg' WHERE [Versao] = 'ETA Concept Card';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/felixphoto.png' WHERE [Versao] = 'Limited Edition S-Class Selfie';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/felixphoto22.jpg' WHERE [Versao] = 'Soundwave POB (Pre-Order)';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/jenniephoto.jpg' WHERE [Versao] = 'Pink Ice Cream Selfie';
                UPDATE [Photocards] SET [ImagemUrl] = '/imagens/jakephoto.jpeg' WHERE [Versao] = 'Dark Blood Orange Ver.';
            ");

            // 7. Criar a coluna PreviewAudioUrl na tabela Musicas caso não exista
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Musicas]') AND name = 'PreviewAudioUrl'
                )
                BEGIN
                    ALTER TABLE [Musicas] ADD [PreviewAudioUrl] nvarchar(max) NULL;
                END");

            // 8. Atualizar as faixas existentes com os URLs de preview de áudio
            await context.Database.ExecuteSqlRawAsync(@"
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3' WHERE [Titulo] = 'ON' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3' WHERE [Titulo] = 'Black Swan' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3' WHERE [Titulo] = 'Filter' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3' WHERE [Titulo] = 'Haegeum' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3' WHERE [Titulo] = 'People Pt.2 (feat. IU)' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-6.mp3' WHERE [Titulo] = 'Lovesick Girls' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-7.mp3' WHERE [Titulo] = 'How You Like That' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-8.mp3' WHERE [Titulo] = 'Super Shy' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-9.mp3' WHERE [Titulo] = 'ETA' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-10.mp3' WHERE [Titulo] = 'LILAC' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-11.mp3' WHERE [Titulo] = 'Celebrity' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-12.mp3' WHERE [Titulo] = 'S-Class' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-13.mp3' WHERE [Titulo] = 'Super Bowl' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-14.mp3' WHERE [Titulo] = 'TOPLINE (feat. Tiger JK)' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-15.mp3' WHERE [Titulo] = 'Bite Me' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-16.mp3' WHERE [Titulo] = 'Sacrifice (Eat Me Up)' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3' WHERE [Titulo] = 'Chaconne' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3' WHERE [Titulo] = 'In Bloom' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3' WHERE [Titulo] = 'New Kidz on the Block' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3' WHERE [Titulo] = 'DASH' AND [PreviewAudioUrl] IS NULL;
                UPDATE [Musicas] SET [PreviewAudioUrl] = 'https://www.soundhelix.com/examples/mp3/SoundHelix-Song-5.mp3' WHERE [Titulo] = 'Soñar (Breaker)' AND [PreviewAudioUrl] IS NULL;
            ");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Aviso ao criar tabelas de photocards.");
        }
        
        // Corre o preenchimento automático de dados de teste (bts, blackpink, etc.)
        await DbSeeder.SeedAsync(context);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Cria os roles se não existirem
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Role '{roleName}' criado com sucesso.");
            }
        }

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular a base de dados.");
    }
}

app.Run();
