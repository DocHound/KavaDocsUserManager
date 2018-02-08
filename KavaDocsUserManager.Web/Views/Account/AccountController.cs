using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Westwind.AspNetCore;
using Westwind.Utilities;

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
        public ActionResult Signin()
        {
            var model = CreateViewModel<SigninViewModel>();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> SignIn(SigninViewModel model)
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

            var identity = AppUser.GetClaimsIdentityFromUser(user);
            
            // Set cookie and attach claims
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
            
            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);



            return Redirect("~/");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Signin");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/isAuthenticated")]
        public bool IsAuthenthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [AllowAnonymous]
        public ActionResult PasswordRecovery()
        {
            var model = CreateViewModel<ProfileViewModel>();
            return View(model); 
        }

        #region Profile Editing and Creation

        [Route("account/profile/new")]
        [AllowAnonymous]
        public ActionResult NewProfile()
        {
            return Profile(Guid.Empty);
        }

        [HttpGet]
        [Route("account/profile/{id?}")]        
        public ActionResult Profile(Guid id)
        {
            var  model = CreateViewModel<ProfileViewModel>();

            User user;
            if (id == Guid.Empty)
            {
                user = _userBus.Create();

                // TODO: REMOVE ME!
                user.UserDisplayName = "NewUser-Addred";
                user.Email = "newuser@west-wind.com";
                user.Company = "Westwind";
                user.LastName = "NewUser";
                user.FirstName = "Frank";
                model.Password = "wwind";
                model.PasswordConfirm = "wwind";
            }
            else
                user = _userBus.GetUser(id);

            model.IsNewUser = _userBus.IsNewEntity(user).Value;

            model.User = user;
            return View(model);
        }

        [HttpPost]
        [Route("account/profile/{id?}")]
        public ActionResult Profile(ProfileViewModel model, Guid id)
        {
            InitializeViewModel(model);
            if (id != Guid.Empty)
                model.User.Id = id;

            //if (!ModelState.IsValid)
            //{
            //    model.ErrorDisplay.AddMessages(ModelState);
            //    model.ErrorDisplay.ShowError("Please correct the following:");
            //    return View(model);
            //}

            model.IsNewUser = false;
            var user = _userBus.GetUser(model.User.Id);
            if (user == null)
            {
                user = _userBus.Create();                
                model.IsNewUser = true;
                user.Password = model.Password;                
            }

            DataUtils.CopyObjectData(model.User, user, "Id,Password");
            
            bool validationResult = _userBus.Validate(user);
            if (model.IsNewUser)
            {
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.PasswordConfirm) ||
                    model.Password != model.PasswordConfirm)
                {
                    _userBus.ValidationErrors.Add("Passwords are missing or don't match");
                    user.Password = null;
                    validationResult = false;
                }                
            }

            if (!validationResult)
            {
                model.ErrorDisplay.AddMessages(_userBus.ValidationErrors);
                model.ErrorDisplay.ShowError("Please fix the following");
                return View(model);
            }

            if (!_userBus.Save())
                model.ErrorDisplay.ShowError(_userBus.ErrorMessage, "Couldn't save your Profile information:");
            else
            {
                model.ErrorDisplay.ShowSuccess("Profile information saved.");
                Response.Headers.Add("Refresh", "2;url=" + Url.Content("~/"));
            }


            if (model.IsNewUser)
            {
                var identity = AppUser.GetClaimsIdentityFromUser(user);

                // Set cookie and attach claims
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)).GetAwaiter().GetResult();
            }

            return View(model);
        }

        #endregion


        #region Email Validation and Recovery

        //    [AllowAnonymous]
        //    [Route("Account/Validate")]
        //    public ActionResult ValidationNotice()
        //    {
        //        var model = CreateViewModel<SigninViewModel>();

        //        var appUser = User.GetAppUser()


        //        return View();
        //}

        [Route("Account/Validate/{email}/{id}")]
        [AllowAnonymous]
        public ActionResult ValidateEmail(string email, string id)
        {
            var model = CreateViewModel<AppBaseViewModel>();

            var user = _userBus.GetUserByEmail(email);
            if (user != null)
            {
                model.ErrorDisplay.ShowError("Sorry, but this email address validation code is not valid.");
                return View("ValidateEmailComplete",model);
            }

            // don't pass this out
            user.Password = null;

            return View("ValidateEmailComplete", model);
        }

        #endregion
    }

    public class SigninViewModel : AppBaseViewModel
    {
        internal User User { get; set; }

        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberPassword { get; set; }

        public string ReturnUrl { get; set; }


    }

    public class ProfileViewModel : AppBaseViewModel
    {
        public User User { get; set; }
        public bool IsNewUser { get; set; }

        public string Password { get; set; }

        public string PasswordConfirm { get; set; }
    }
}