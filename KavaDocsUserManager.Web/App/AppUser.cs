using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Models;
using KavaDocsUserManager.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Westwind.AspNetCore;
using Westwind.AspNetCore.Security;

namespace KavaDocsUserManager.Web
{
    public class AppUser : AppUserBase
    {
        public AppUser(ClaimsPrincipal user) : base(user)
        {
        }

        public Guid UserId
        {
            get
            {
                Guid userId = Guid.Empty;

                var strId = GetClaim("UserId");
                if (!string.IsNullOrEmpty(strId))
                    Guid.TryParse(strId, out userId);

                return userId;
            }
        }

        public string Username => GetClaim("Username");
        public string Email => GetClaim("Email");

        public bool IsAdmin => HasRole(RoleNames.Administrators);

        public static ClaimsIdentity GetClaimsIdentityFromUser(User user )
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim("Email", user.Email));
            identity.AddClaim(new Claim("Username", user.UserDisplayName));
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

            if (user.IsAdmin)
                identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Administrators));

            return identity;
        }


    }
    
    public static class ClaimsPrincipalExtensions
    {
        public static AppUser GetAppUser(this ClaimsPrincipal user)
        {
            return new AppUser(user);
        }

    }

    public struct RoleNames
    {

        public const string Administrators = "Administrators";

    }
}


