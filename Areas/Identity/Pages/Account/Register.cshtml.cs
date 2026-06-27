// licensed to the .net foundation under one or more agreements.
// the .net foundation licenses this file to you under the mit license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using K_Shelf.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace K_Shelf.Areas.Identity.Pages.Account
{
    // pagina de registo de novos utilizadores
    // foi modificada para adicionar a atribuicao automatica do role user
    public class RegisterModel : PageModel
    {
        // servicos do identity para gestao de utilizadores, signin e roles
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IUserStore<Utilizador> _userStore;
        private readonly IUserEmailStore<Utilizador> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager; // gestor de roles (adicionado para atribuir role user)

        // construtor que recebe os servicos por injecao de dependencias
        public RegisterModel(
            UserManager<Utilizador> userManager,
            IUserStore<Utilizador> userStore,
            SignInManager<Utilizador> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager) // rolemanager injetado
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager; // inicializa o rolemanager
        }

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        [BindProperty] // vincula os dados do formulario a esta propriedade
        public InputModel Input { get; set; }

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; } // url para onde redirecionar apos o registo

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; } // logins externos (google, facebook, etc)

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
            ///     directly from your code. this api may change or be removed in future releases.
            /// </summary>
            [Required] // campo obrigatorio
            [EmailAddress] // valida se e um email valido
            [Display(Name = "Email")]
            public string Email { get; set; } // email do utilizador

            /// <summary>
            ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
            ///     directly from your code. this api may change or be removed in future releases.
            /// </summary>
            [Required] // campo obrigatorio
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)] // tamanho entre 6 e 100
            [DataType(DataType.Password)] // tipo de dados password (oculta os caracteres)
            [Display(Name = "Password")]
            public string Password { get; set; } // password do utilizador

            /// <summary>
            ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
            ///     directly from your code. this api may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)] // tipo de dados password
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")] // valida se a confirmacao e igual a password
            public string ConfirmPassword { get; set; } // confirmacao da password
        }

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl; // define a url de retorno
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // carrega os logins externos
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // define a url padrao se nao for fornecida
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // carrega os logins externos

            // verifica se o modelo e valido
            if (ModelState.IsValid)
            {
                // cria um novo utilizador
                var user = CreateUser();

                // define o username e o email do utilizador
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // tenta criar o utilizador com a password fornecida
                var result = await _userManager.CreateAsync(user, Input.Password);

                // se a criacao for bem sucedida
                if (result.Succeeded)
                {
                    _logger.LogInformation("Utilizador criou uma nova conta com password.");

                    // modificacao: atribuicao automatica do role user

                    // verifica se o role "user" existe, caso contrario cria-o
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                        _logger.LogInformation("Role 'User' created.");
                    }

                    // adiciona o role "user" ao novo utilizador
                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation($"User '{user.Email}' assigned to role 'User'.");

                    // ============================================================

                    // gera o token de confirmacao de email
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // constroi a url de confirmacao de email
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // envia o email de confirmacao
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // verifica se e necessario confirmar a conta antes de fazer login
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // redireciona para a pagina de confirmacao de registo
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        // faz login automatico apos o registo
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl); // redireciona para a pagina inicial
                    }
                }

                // adiciona os erros ao modelstate para exibicao ao utilizador
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // se algo falhou, volta a exibir o formulario com os erros
            return Page();
        }

        // cria uma nova instancia do modelo utilizador
        private Utilizador CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Utilizador>();
            }
            catch
            {
                // se nao for possivel criar a instancia, lanca uma excecao
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Utilizador)}'. " +
                    $"Ensure that '{nameof(Utilizador)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // obtem o store de email do utilizador
        private IUserEmailStore<Utilizador> GetEmailStore()
        {
            // verifica se o user manager suporta email
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Utilizador>)_userStore; // retorna o store de email
        }
    }
}