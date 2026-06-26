# K-Shelf - Plataforma de Gestão de Coleções de K-Pop

## Informações do Projeto

**Curso** : Licenciatura em Engenharia Informática |
**Disciplina** : Desenvolvimento Web |
**Ano Letivo** : 2025/2026 |
**Instituição** : Instituto Politécnico de Tomar |

## Autores

Constança Azevedo : 25969 
Rui Dias : 25957 

---

## Aplicação Publicada

[https://kshelf-app-asbhf5a3ezcyb9aa.canadacentral-01.azurewebsites.net/](https://kshelf-app-asbhf5a3ezcyb9aa.canadacentral-01.azurewebsites.net/)

---

## Tecnologias Utilizadas

**ASP.NET Core 10.0** : Framework principal 
**Razor Pages** : Interface Web 
**Entity Framework Core 10.0** : ORM para acesso à BD 
**ASP.NET Core Identity** : Autenticação e Autorização 
**SignalR** : Comunicação em tempo real 
**Swagger/OpenAPI** : Documentação da API 
**Bootstrap 5.3.0** : Framework CSS 
**SQL Server** : Base de dados 

---

## Funcionalidades Implementadas

### Interface Web
- CRUD completo de **Artistas**, **Álbuns** e **Coleções**
- CRUD de **Photocards** (painel Admin)
- **Binder Virtual** de Photocards (3x3 com 3D Flip)
- **Leitor de áudio** (previews de 30s via iTunes API)
- **Chat Global** com SignalR
- **Notificações em tempo real** (SignalR)
- Dashboard de Administração

### API REST
- Endpoints para Artistas, Álbuns, Coleções e Photocards
- Documentação Swagger disponível em `/swagger`

### Segurança
- Autenticação com ASP.NET Core Identity
- Cargos: **Admin** e **User**
- Controlo de acessos diferenciado

### Publicação
- Publicado no **Microsoft Azure App Service**
- Base de dados SQL Server no Azure

---

## Credenciais de Acesso

**Admin** : admin@kshelf.com | Admin@123 
**User** : user@kshelf.com	 | User@123

---

## Como Executar Localmente

### Pré-requisitos
- .NET 10.0 SDK
- SQL Server (LocalDB ou Azure)

### Passos

# 1. Clonar o repositório
git clone https://github.com/ConstancaAzevedo/K-Shelf.git

# 2. Navegar para a pasta
cd K-Shelf

# 3. Restaurar pacotes
dotnet restore

# 4. Atualizar a base de dados
dotnet ef database update

# 5. Executar a aplicação
dotnet run