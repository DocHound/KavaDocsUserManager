using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KavaDocsUserManager.Web.Views.Repositories
{
    [Authorize]
    public class RepositoriesController : AppBaseController
    {
        private readonly RepositoryBusiness _repoBusiness;

        public RepositoriesController(RepositoryBusiness repoBusiness)
        {
            _repoBusiness = repoBusiness;
        }

        public async Task<ActionResult> Index()
        {
            var model = CreateViewModel<RepositoriesListViewModel>();

            var appUser = User.GetAppUser();

            var userId = User.GetAppUser().UserId;            
            var repos = await _repoBusiness.GetRepositoriesForUserAsync(userId);
            if (repos == null)
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't load Repositories");
            else
                model.Repositories = repos;

            return View("Repositories", model);
        }

        [HttpGet]
        public ActionResult Repository(Guid id)
        {
            var model = CreateViewModel<RepositoryViewModel>();

            var appUser = User.GetAppUser();
            
            var repos = _repoBusiness.GetRepository(id);
            if (repos == null)
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't load Repositories");
            else
                model.Repository = repos;

            return View(model);
        }
    }

    public class RepositoriesListViewModel : AppBaseViewModel
    {
        public List<Repository> Repositories { get; set; }
    }

    public class RepositoryViewModel : AppBaseViewModel
    {
        public Repository Repository { get; set; }
    }
}