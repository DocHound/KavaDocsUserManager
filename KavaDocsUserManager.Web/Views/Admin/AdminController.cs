using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KavaDocsUserManager.Web.Views.Admin
{
    [Authorize(Roles=RoleNames.Administrators)]
    public class AdminController : AppBaseController
    {
        public IActionResult Index()
        {
            var model = CreateViewModel<AppBaseViewModel>();
            return View(model);
        }

   }

    

}