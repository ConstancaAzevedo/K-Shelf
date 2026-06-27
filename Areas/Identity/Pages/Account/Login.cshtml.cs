// licensed to the .net foundation under one or more agreements.
// the .net foundation licenses this file to you under the mit license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using K_Shelf.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace K_Shelf.Areas.Identity.Pages.Account
{
    // pagina de login de utilizadores
    public class LoginModel : PageModel
    {
        // servicos do identity para signin e logging
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        // construtor que recebe os servicos por injecao de dependencias
        public LoginModel(SignInManager<Utilizador> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; } // logins externos (google, facebook, etc)

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; } // url para onde redirecionar apos o login

        /// <summary>
        ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
        ///     directly from your code. this api may change or be removed in future releases.
        /// </summary>
        [TempData] // dados temporarios que persistem entre requisicoes
        public string ErrorMessage { get; set; } // mensagem de erro a exibir

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
            public string Email { get; set; } // email do utilizador

            /// <summary>
            ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
            ///     directly from your code. this api may change or be removed in future releases.
            /// </summary>
            [Required] // campo obrigatorio
            [DataType(DataType.Password)] // tipo de dados password (oculta os caracteres)
            public string Password { get; set; } // password do utilizador

            /// <summary>
            ///     this api supports the asp.net core identity default ui infrastructure and is not intended to be used
            ///     directly from your code. this api may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; } // opcao para manter sessao ativa
        }

        // metodo executado quando a pagina e carregada via get
        public async Task OnGetAsync(string returnUrl = null)
        {
            // se houver uma mensagem de erro, adiciona ao modelstate
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/"); // define a url padrao se nao for fornecida

            // limpa o cookie externo para garantir um processo de login limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // carrega os logins externos disponiveis
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl; // define a url de retorno
        }

        // metodo executado quando o formulario e submetido via post
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/"); // define a url padrao se nao for fornecida

            // carrega os logins externos disponiveis
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // verifica se o modelo e valido
            if (ModelState.IsValid)
            {
                // tenta fazer login com email e password
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                // se o login for bem sucedido
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in."); // regista o login
                    return LocalRedirect(returnUrl); // redireciona para a pagina inicial
                }

                // se o utilizador tiver autenticacao de dois fatores ativa
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                // se a conta estiver bloqueada
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out."); // regista o bloqueio
                    return RedirectToPage("./Lockout"); // redireciona para a pagina de bloqueio
                }
                else
                {
                    // se o login falhar, adiciona erro ao modelstate
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page(); // volta a exibir o formulario com o erro
                }
            }

            // se algo falhou, volta a exibir o formulario com os erros
            return Page();
        }
    }
}