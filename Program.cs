using K_Shelf.Data;
using K_Shelf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DE SERVIÇOS (Dependency Injection)
// =========================================================================

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
