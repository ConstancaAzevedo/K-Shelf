using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

// Ponto de entrada da aplicação
// Configura todos os serviços, middlewares e inicializa a base de dados


// contrução da aplicação
var builder = WebApplication.CreateBuilder(args);

// configuraçáo se serviços

// contexto de Base de Dados (ApplicationDbContext)
// regista o ApplicationDbContext para ser usado com SQL Server
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// autenticação e autorização 
// configura o Identity com o modelo Utilizador e as opções de segurança
builder.Services.AddDefaultIdentity<Utilizador>(options =>
{
    // Não exige confirmação de email para login (simplifica o registo)
    options.SignIn.RequireConfirmedAccount = false;
    // Garante que cada email é único na base de dados
    options.User.RequireUniqueEmail = true;

    // configuração de passwords
    options.Password.RequireDigit = false;          // Não exige números (ex: 123)
    options.Password.RequiredLength = 6;            // Comprimento mínimo: 6 caracteres
    options.Password.RequireNonAlphanumeric = false;// Não exige caracteres especiais (ex: @, #)
    options.Password.RequireUppercase = false;      // Não exige letras maiúsculas
    options.Password.RequireLowercase = false;      // Não exige letras minúsculas
})
.AddRoles<IdentityRole>() // Ativa suporte a Roles (Admin, User)
.AddEntityFrameworkStores<ApplicationDbContext>(); // Guarda os dados no mesmo contexto

// controllers e views
// Ativa suporte para API Controllers (MVC) e Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SINGALR comunicação em tempo real
// Ativa o suporte para WebSockets e notificações em tempo real
builder.Services.AddSignalR();

// Configuração do Swagger para documentação automática da API REST
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Informações gerais da API
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

// construçáo da aplicação (após configuração)
var app = builder.Build();

// congiuração do pipeline de pedidos HTTP (Middlewares) 

// ambiente de desenvolvimneto
if (app.Environment.IsDevelopment())
{
    // ativa a página de erros detalhados do Entity framework e a página de migrações
    app.UseMigrationsEndPoint();
}
else
{
    // Em produção a página de erro gen´´erica e hsts
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// SWAGGER (sempre ativo) disponível em .../swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "K-Shelf API v1");
    c.RoutePrefix = "swagger"; // Acede em /swagger
});

// tratamento de erros hhtp
// Redireciona códigos 404, 403, etc. para a página de erro personalizada
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

// segurança
// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// ficheiros estáticos
// Serve ficheiros da pasta wwwroot (CSS, JS, imagens, etc.)
app.UseStaticFiles();

// roteamento
app.UseRouting();

// autenticação e autorização
app.UseAuthentication(); // Middleware de autenticação (verifica se o utilizador está logado)
app.UseAuthorization(); // Middleware de autorização (verifica permissões/roles)

// rotas
// Rotas para Controllers MVC (API)
app.MapControllerRoute(
name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Rotas para Razor Pages


// Mapear o endpoint do Hub do SignalR para o Chat e Contador de utilizadores
app.MapHub<K_Shelf.Hubs.KpopChatHub>("/kpopChatHub");

// Mapear o endpoint do Hub de Notificações
app.MapHub<K_Shelf.Hubs.NotificacaoHub>("/notificacaoHub");

// inicializaçáo e alimentação da base de dados (seeding)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // garante que as tabelas existem mesmo sem migrações manuais
        try
        {
            // Criar a tabela de histórico de migrações se não existir
            await context.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY,
                        [ProductVersion] nvarchar(32) NOT NULL
                    );
                END");

            // registar todas as migrações como já aplicadas para evitar conflitos com migrações futuras
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

            // corrigir localmente a coluna Nacionalidade para Pais na tabela Artistas para compatibilidade com o Azure
            await context.Database.ExecuteSqlRawAsync(@"
                IF EXISTS(SELECT * FROM sys.columns 
                          WHERE Name = N'Nacionalidade' AND Object_ID = Object_ID(N'Artistas'))
                   AND NOT EXISTS(SELECT * FROM sys.columns 
                                  WHERE Name = N'Pais' AND Object_ID = Object_ID(N'Artistas'))
                BEGIN
                    EXEC sp_rename 'Artistas.Nacionalidade', 'Pais', 'COLUMN';
                END");

            // criar a tabela Photocards caso não exista
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

            // criar a tabela UtilizadorPhotocards caso não exista (relação muitos-para-muitos)
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

            // criar índices para otimizar consultas
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

            // atualizar as URLs dos photocards de sementeira com as novas imagens locais da pasta
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

            // criar a coluna PreviewAudioUrl na tabela Musicas caso não exista
            await context.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[Musicas]') AND name = 'PreviewAudioUrl'
                )
                BEGIN
                    ALTER TABLE [Musicas] ADD [PreviewAudioUrl] nvarchar(max) NULL;
                END");

        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Aviso ao criar tabelas de photocards.");
        }
        
        // dados iniciais (seeding)
        // Corre o preenchimento automático de dados de teste (bts, blackpink, etc.)
        await DbSeeder.SeedAsync(context);

        // Pesquisa e atualiza automaticamente os links reais das músicas através da API do iTunes
        await DbSeeder.FetchItunesPreviewsAsync(context);

        // criaçáo de cargos de utilizador (Admin e User)
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

// inicio da aplicação
app.Run();
