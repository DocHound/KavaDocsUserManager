using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Westwind.Utilities;

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

        [Route("Repositories/{id}")]
        [HttpGet]
        public ActionResult Repository(Guid id)
        {
            var model = CreateViewModel<RepositoryViewModel>();

            var appUser = User.GetAppUser();
            
            var repo = _repoBusiness.GetRepository(id);
            if (repo == null)
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't load Repositories");
            else
                model.Repository = repo;

            model.SettingsJson = repo.Settings;
            if (!string.IsNullOrEmpty(model.SettingsJson))
                model.SettingsJson = JsonSerializationUtils.FormatJsonString(repo.Settings);
            else
                model.SettingsJson = "No repository settings are set yet";

            model.TocJson = repo.TableOfContents;
            if (!string.IsNullOrEmpty(model.TocJson))
                model.TocJson = JsonSerializationUtils.FormatJsonString(repo.TableOfContents);
            else
                model.TocJson = "No table of contents - uses remote _toc.json repository";

            return View(model);
        }

        [Route("Repositories/{id}/edit")]
        [HttpGet]
        public ActionResult EditRepository(Guid id)
        {
            var model = CreateViewModel<RepositoryViewModel>();
           

            var repo = _repoBusiness.GetRepository(id);
            if (repo == null)
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't load Repositories");
            else
                model.Repository = repo;

            model.SettingsJson = repo.Settings;
            if (!string.IsNullOrEmpty(model.SettingsJson))
                model.SettingsJson = JsonSerializationUtils.FormatJsonString(repo.Settings);
            else
                model.SettingsJson = "No repository settings are set yet";

            model.TocJson = repo.TableOfContents;
            if (!string.IsNullOrEmpty(model.TocJson))
                model.TocJson = JsonSerializationUtils.FormatJsonString(repo.TableOfContents);
            else
                model.TocJson = "No table of contents - uses remote _toc.json repository";

            return View(model);
        }

        
        [Route("Repositories/{id?}")]
        [HttpPost]
        public ActionResult EditRepository(RepositoryViewModel model,Guid id)
        {
            InitializeViewModel(model);
            if (id != Guid.Empty)
                model.Repository.Id = id;

            var repo = _repoBusiness.GetRepository(model.Repository.Id);
            if (repo == null)
                repo = _repoBusiness.Create();
            
            DataUtils.CopyObjectData(model.Repository, repo, "Id,Users");

            bool validationResult = _repoBusiness.Validate(repo);
            if (!validationResult)
            {
                model.ErrorDisplay.AddMessages(_repoBusiness.ValidationErrors);
                model.ErrorDisplay.ShowError("Please fix the following");
                return View(model);
            }

            if (!_repoBusiness.Save())
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't save Repository.");
            else
                model.ErrorDisplay.ShowSuccess("Repository info saved.");

            return View(model);
        }

    }

    public class RepositoriesListViewModel : AppBaseViewModel
    {
        public List<Repository> Repositories { get; set; }
    }

    public class RepositoryViewModel : AppBaseViewModel
    {
        public string SettingsJson { get; set; }

        public string TocJson { get; set; }



        public Repository Repository { get; set; }
    }
}