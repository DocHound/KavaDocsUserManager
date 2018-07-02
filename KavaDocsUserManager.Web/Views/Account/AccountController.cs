using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using KavaDocsUserManager.Web.App;
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
            model.ReturnUrl = Request.Query["ReturnUrl"];
            return View("SignIn",model);
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
                new ClaimsPrincipal(identity), new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(2)
                });

            
            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return Redirect("~/");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/isAuthenticated")]
        public bool IsAuthenthenticated()
        {
            return User.Identity.IsAuthenticated;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("api/isAuthenticated/{userToken}")]
        public bool IsAuthenthenticatedToken(string userToken)
        {
            return User.Identity.IsAuthenticated;
        }



        [AllowAnonymous]
        public ActionResult PasswordRecovery()
        {
            var model = CreateViewModel<PasswordRecoveryModel>();
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
                user.ValidationKey = Guid.NewGuid().ToString("N");
            }

             //DataUtils.CopyObjectData(model.User, user, "Id,Password,IsActive,IsAdmin,ValidationKey,Repositories");

            user.UserDisplayName = model.User.UserDisplayName;
            user.Email = model.User.Email;
            user.Company = model.User.Company;
            user.FirstName = model.User.FirstName;
            user.LastName = model.User.LastName;
            
            bool validationResult = _userBus.Validate(user);
            if (model.IsNewUser)
            {
                if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.PasswordConfirm) ||
                    model.Password != model.PasswordConfirm)
                {
                    model.ErrorDisplay.AddMessage("Passwords are missing or don't match","Password");
                    user.Password = null;
                    validationResult = false;
                }                
            }

            if (!validationResult)
            {
                model.ErrorDisplay.AddMessages(_userBus.ValidationErrors,"User_");
                model.ErrorDisplay.ShowError("Please fix the following");
                return View(model);
            }

            if (!_userBus.Save())
                model.ErrorDisplay.ShowError(_userBus.ErrorMessage, "Couldn't save your Profile information:");
            else
            {
                var msg = "Profile information saved.";
                if (model.IsNewUser)
                {                    
                    string url = KavaDocsConfiguration.Current.ApplicationHomeUrl +  Url.Content($"~/account/validate/{user.ValidationKey}");
                    string title = "Kava Docs Account Email Verification";
                    string body = $@"Please visit the following link to complete your account signup.

{url}

The Kava Docs Team
";

                    AppUtils.SendEmail(user.Email, title, body, out string error);

                    msg +=
                        $" Please check your email account, to validate your account. You'll receive an email from 'info@kavadocs.com' - follow the verification link to complete your account signup. Didn't get it? Click on 'I forgot my password'.";

                    model.ErrorDisplay.ShowSuccess(msg);

                    var loginModel = CreateViewModel<SigninViewModel>();
                    loginModel.ErrorDisplay.ShowSuccess(msg);
                    return View("Signin", loginModel);
                }
                else
                {
                    model.ErrorDisplay.ShowSuccess(msg);
                    Response.Headers.Add("Refresh", "2;url=" + Url.Content("~/"));
                }
            }
           

            return View(model);
        }

        #endregion


        #region Email Validation and Recovery

        [Route("/account/recover")]
        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public ActionResult PasswordRecoverySendEmail(string email)
        {
            var model = CreateViewModel<PasswordRecoveryModel>();

            if (Request.Method == "GET")
                return View(model);


            if (string.IsNullOrEmpty(email))
            {
                model.ErrorDisplay.ShowError("Please specify an email address");
                return View(model);
            }

            var validationId = _userBus.CreateRecoveryValidationId(email);
            if (string.IsNullOrEmpty(validationId))
            {
                //model.ErrorDisplay.ShowError("Email not sent.");
                model.ErrorDisplay.ShowSuccess("Verification email has been sent. Please check your account for a message from 'kavadocs.com'.");
                return View(model);
            }

            var url =KavaDocsConfiguration.Current.ApplicationHomeUrl +  Url.Action("PasswordRecovery", new {validationId = validationId});

            string title = "Kava Docs Password Recovery";
            string body = $@"Please visit the following link to recover your password. On this page you can enter a new password and confirm it.

{url}

The Kava Docs Team
";
            bool success =  AppUtils.SendEmail(email, title, body,out string error);
            if (!success)
                model.ErrorDisplay.ShowError("Unable to send email: " + error);
            else
            {
                model.ErrorDisplay.ShowSuccess("Verification email has been sent. Please check your account for a message from 'kavadocs.com'.");                

                // always log out user and force them to log back in
                User.GetAppUser().LogoutUser(HttpContext);
                Response.Headers.Add("Refresh", "2;url=" + Url.Action("Signin", "Account"));
            }



            return View(model);
        }

        [Route("/account/recover/verify/{validationId}")]
        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public ActionResult PasswordRecovery(string validationId,PasswordRecoveryModel model)
        {
            InitializeViewModel(model);

            if (Request.Method == "GET")
                return View(model);


            if (string.IsNullOrEmpty(validationId))            
                return RedirectToAction("Index", "Home");                
            
            var user = _userBus.GetUserByValidationId(validationId);
            if (user == null)
                return RedirectToAction("Index", "Home");

            // display the entry form
            if (Request.Method == "GET")
                return View(model);

            // validate and save new password
            if (model.Password != model.PasswordRecovery)
            {
                model.ErrorDisplay.ShowError("Please make sure your new passwords entered match.");
                return View(model);
            }
            
            if(!_userBus.RecoverPassword(validationId, model.Password))
            {
                model.ErrorDisplay.AddMessages(_userBus.ValidationErrors);
                model.ErrorDisplay.ShowError("Couldn't save new password");                
            }
            else
            {
                Response.Headers.Add("Refresh", "2;url=" + Url.Action("Signin","Account"));
                model.ErrorDisplay.ShowSuccess("Password updated.");

                // always log out user and force them to log back in
                User.GetAppUser().LogoutUser(HttpContext);
            }

            return View(model);
        }

  
        [Route("account/validate/{validationId}")]
        [AllowAnonymous]
        public ActionResult ValidateEmail(string validationId)
        {
            var model = CreateViewModel<SigninViewModel>();

            // don't pass this out
            if (!_userBus.ValidateEmail(validationId))
                model.ErrorDisplay.ShowError(_userBus.ErrorMessage);
            else
                model.ErrorDisplay.ShowSuccess("Thank you for verifying your email address. You're ready to sign in.");

            return View("Signin", model);
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

    public class PasswordRecoveryModel : AppBaseViewModel
    {
    //    internal User User { get; set; }
        
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string PasswordRecovery { get; set; }

        [Required]
        public string ValidationId { get; set; }

    }
    

    public class ProfileViewModel : AppBaseViewModel
    {
        public User User { get; set; }
        public bool IsNewUser { get; set; }

        public string Password { get; set; }
        public string PasswordConfirm { get; set; }        
    }
}