using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MusicStore.DataAccess.IMainRepository;
using MusicStore.Models.Models;
using MusicStore.Utility;

namespace MusicStore.UI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _uow = uow;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostaCode { get; set; }
            public string PhoneNumber { get; set; }
            public int? CompanyId { get; set; }
            public string Role { get; set; }
            public IEnumerable<SelectListItem> CompanyList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            Input = new InputModel()
            {
                // Company tablosundan şirketleri çekiyoruz ve CompanyList'e aktarıyoruz
                CompanyList = _uow.Company.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                // Rol işlemini roleManager üzerinden yapılıyor.
                // Rol tablosunda name'i bireysel müşteri değilse name değerini al ve göster diyoruz.
                RoleList = _roleManager.Roles.Where(x => x.Name != ProjectConstant.Role_User_Indi) // bireysel müşteri olmayan.
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    CompanyId = Input.CompanyId,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    State = Input.State,
                    PostaCode = Input.PostaCode,
                    Name = Input.Name,
                    PhoneNumber = Input.PhoneNumber,
                    Role = Input.Role
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!await _roleManager.RoleExistsAsync(ProjectConstant.Role_Admin)) // rol admin var mı ?
                    {
                        await _roleManager.CreateAsync(new IdentityRole(ProjectConstant.Role_Admin)); // admin rolü yok ise oluşturuluyor
                    }

                    if (!await _roleManager.RoleExistsAsync(ProjectConstant.Role_User_Employee)) // rol admin var mı ?
                    {
                        await _roleManager.CreateAsync(new IdentityRole(ProjectConstant.Role_User_Employee)); // admin rolü yok ise oluşturuluyor
                    }

                    if (!await _roleManager.RoleExistsAsync(ProjectConstant.Role_User_Comp)) // rol admin var mı ?
                    {
                        await _roleManager.CreateAsync(new IdentityRole(ProjectConstant.Role_User_Comp)); // admin rolü yok ise oluşturuluyor
                    }

                    if (!await _roleManager.RoleExistsAsync(ProjectConstant.Role_User_Indi)) // rol admin var mı ?
                    {
                        await _roleManager.CreateAsync(new IdentityRole(ProjectConstant.Role_User_Indi)); // admin rolü yok ise oluşturuluyor
                    }

                    // await _userManager.AddToRoleAsync(user, ProjectConstant.Role_Admin); // kayıt olan kullanıcıya admin rolünü atadık

                    if (user.Role == null)
                    {
                        await _userManager.AddToRoleAsync(user, ProjectConstant.Role_User_Indi); // rol null ise bireysel müşteridir
                    }
                    else
                    {
                        if (user.CompanyId > 0)
                        {
                            await _userManager.AddToRoleAsync(user, ProjectConstant.Role_User_Comp); // companyId 0'dan büyükse ise 
                        }
                        await _userManager.AddToRoleAsync(user, user.Role);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (user.Role == null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            //TODO: Admin is Registering new user
                            return RedirectToAction("Index", "User", new { Area = "Admin" }); // admin ise kullanıcı bu sayfaya gitsin
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
