using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KavaDocsUserManager.Models;
using Westwind.AspNetCore;

namespace KavaDocsUserManager.Web.Controllers
{
    public class HomeController : AppBaseController
    {
        public IActionResult Index()
        {
            var appUser = User.GetAppUser();
            if (appUser.IsAuthenticated())
                return RedirectToAction("Index", "Repositories");
            else
                return RedirectToAction("Signin", "Account");
            
            var model = CreateViewModel<AppBaseViewModel>();
            return View(model);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
