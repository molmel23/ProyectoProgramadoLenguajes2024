﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Signing;
using ProyectoProgramadoLenguajes2024.Data.Repository.Interfaces;
using ProyectoProgramadoLenguajes2024.Models;
using ProyectoProgramadoLenguajes2024.Models.ViewModels;
using ProyectoProgramadoLenguajes2024.Utilities;

namespace ProyectoProgramadoLenguajes2024.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterModel(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> roleList { get; set; }
            public string? Nombre { get; set; }
            public int Cedula { get; set; }
            public int NumeroColegiado { get; set; }
            public IFormFile? Foto { get; set; }
        }

        private void CreateRoles()
        {
            if (!_roleManager.RoleExistsAsync(Roles.Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.Admin)).GetAwaiter().GetResult();
            }

            if (!_roleManager.RoleExistsAsync(Roles.Medico).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.Medico)).GetAwaiter().GetResult();
            }

            if (!_roleManager.RoleExistsAsync(Roles.Usuario).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.Usuario)).GetAwaiter().GetResult();
            }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            CreateRoles();
            Input = new()
            {
                roleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.Nombre = Input.Nombre;
                user.Cedula = Input.Cedula;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (string.IsNullOrEmpty(Input.role))
                    {
                        await _userManager.AddToRoleAsync(user, Roles.Usuario);
                        Paciente paciente = new Paciente
                        {
                            Cedula = user.Cedula,
                            NombreCompleto = user.Nombre,
                            CorreoElectronico = user.Email
                        };
                        _unitOfWork.Pacientes.Add(paciente);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, Input.role);

                        if (Input.role == Roles.Usuario)
                        {
                            Paciente paciente = new Paciente
                            {
                                Cedula = user.Cedula,
                                NombreCompleto = user.Nombre,
                                CorreoElectronico = user.Email
                            };
                            _unitOfWork.Pacientes.Add(paciente);
                            _unitOfWork.Save();
                        } 
                        else if(Input.role == Roles.Admin)
                        {
                            Administrador administrador = new Administrador
                            {
                                Cedula = user.Cedula,
                                NombreCompleto = user.Nombre,
                                CorreoElectronico = user.Email
                            };
                            _unitOfWork.Administradores.Add(administrador);
                            _unitOfWork.Save();
                        }
                        
                        
                        else if (Input.role == Roles.Medico)
                        {
                            MedicoTratante medico = new MedicoTratante
                            {
                                Cedula = user.Cedula,
                                NumeroColegiado = Input.NumeroColegiado,
                                NombreCompleto = user.Nombre,

                            };

                            string wwwRootPath = _webHostEnvironment.WebRootPath;
                            if (Input.Foto != null)
                            {
                                string fileName = Guid.NewGuid().ToString();
                                string extension = Path.GetExtension(Input.Foto.FileName);
                                var uploads = Path.Combine(wwwRootPath, @"images\medicos");

                                using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                                {
                                    Input.Foto.CopyTo(fileStream);
                                }

                                medico.FotoURL = @"images\medicos\" + fileName + extension;
                            }
                            else
                            {
                                medico.FotoURL = @"images\medicos\default.jpg";
                            }
                            _unitOfWork.MedicoTratantes.Add(medico);
                            _unitOfWork.Save();
                        }
                    }
                    //retorna a pagina de confirmación de registro
                        return RedirectToPage("/Account/RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // Vuelve a cargar la lista de roles en caso de error
            Input.roleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
