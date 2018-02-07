using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KavaDocsUserManager.Web;
using Westwind.AspNetCore;

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
        public bool IsAdmin => HasRole("Admin");


    }
    
    public static class ClaimsPrincipalExtensions
    {
        public static AppUser GetAppUser(this ClaimsPrincipal user)
        {
            return new AppUser(user);
        }

    }
}


