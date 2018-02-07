using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Westwind.AspNetCore;

namespace KavaDocsUserManager.Web.Views.Account
{
    public class AccountController : AppBaseController
    {
        private readonly UserBusiness _userBus;

        public AccountController(UserBusiness userBus)
        {
            _userBus = userBus;
        }

        [HttpGet]
        public ActionResult Login()
        {
            var model = CreateViewModel<LoginViewModel>();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            InitializeViewModel(model);

            if (!ModelState.IsValid)
            {
                model.ErrorDisplay.AddMessages(ModelState);
                model.ErrorDisplay.ShowError("Please correct the following:");
                return View(model);
            }

            var user = _userBus.AuthenticateAndRetrieveUser(model.Email, model.Password);
            if (user == null)
            {
                model.ErrorDisplay.ShowError(_userBus.ErrorMessage);
                return View(model);
            }
            
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim("Email", user.Email));
            identity.AddClaim(new Claim("Username", user.UserDisplayName));
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

            if (user.IsAdmin)
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            
            // Set cookie and attach claims
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
            
            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return Redirect("~/");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/isAuthenticated")]
        public bool IsAuthenthenticated()
        {
            return User.Identity.IsAuthenticated;
        }
    }

    public class LoginViewModel : AppBaseViewModel
    {
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberPassword { get; set; }

        public string ReturnUrl { get; set; }
    }
}