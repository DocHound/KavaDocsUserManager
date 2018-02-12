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
    
    //[Authorize]
    public class RepositoriesController : AppBaseController
    {
        private readonly RepositoryBusiness _repoBusiness;

        public RepositoriesController(RepositoryBusiness repoBusiness)
        {
            _repoBusiness = repoBusiness;
        }

        [HttpGet]
        [Route("Repositories")]
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

        [Route("Repositories/new")]
        [Route("Repositories/{id}")]
        [Route("Repository/{id?}")]
        [HttpGet]
        public ActionResult Repository(Guid id)
        {
            var model = CreateViewModel<RepositoryViewModel>();
            var appUser = User.GetAppUser();
            bool isNew = id == Guid.Empty;
            
            var repo = _repoBusiness.GetRepository(id);
            if (repo == null)
            {
                if (isNew)
                {   
                    repo = _repoBusiness.Create();
                    repo.Title = "New Documentation Repository";                    
                    model.Repository = repo;
                }
                else
                {
                    model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't load Repository");
                    return RedirectToAction("Index");
                }
            }
            else
                model.Repository = repo;

            // not a user or contributor
            if (!isNew && !repo.IsUserInRepository(appUser.UserId))           
                return RedirectToAction("Index");

            SharedRepositoryModelDisplay(model);

            return View("Repository",model);
        }      

        
        [Route("Repositories/{id?}")]
        [HttpPost]
        public ActionResult SaveRepository(RepositoryViewModel model, Guid id)
        {
            InitializeViewModel(model);

            if (id != Guid.Empty)
                model.Repository.Id = id;

            var repo = _repoBusiness.GetRepository(model.Repository.Id);
            if (repo == null)
                repo = _repoBusiness.CreateRepository(User.GetAppUser().UserId);                

            DataUtils.CopyObjectData(model.Repository, repo, "Id,Users");

            repo.Settings = model.SettingsJson;
            repo.TableOfContents = model.TableOfContentsJson;
            
            bool validationResult = _repoBusiness.Validate(repo);
            if (!validationResult)
            {
                model.ErrorDisplay.AddMessages(_repoBusiness.ValidationErrors);
                model.ErrorDisplay.ShowError("Please fix the following");
                return View("Repository",model);
            }
            
            if (!_repoBusiness.Save())
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't save Repository.");
            else
                model.ErrorDisplay.ShowSuccess("Repository info saved.");

            model.Repository = repo;
            SharedRepositoryModelDisplay(model);

            return View("Repository", model);
        }

        void SharedRepositoryModelDisplay(RepositoryViewModel model)
        {
            var repo = model.Repository;
            var appUser = User.GetAppUser();

            model.SettingsJson = repo.Settings;
            if (!string.IsNullOrEmpty(model.SettingsJson))
                try
                {
                    model.SettingsJson = JsonSerializationUtils.FormatJsonString(repo.Settings);
                }
                catch { }

            model.TableOfContentsJson = repo.TableOfContents;
            if (!string.IsNullOrEmpty(model.TableOfContentsJson))
                try
                {
                    model.TableOfContentsJson = JsonSerializationUtils.FormatJsonString(repo.TableOfContents);
                }
                catch { }

            if (!appUser.IsEmpty())
                model.IsOwner = _repoBusiness.IsOwner(repo, appUser.UserId);
        }

        [HttpGet]
        [Route("api/repositories/{repoId}/remove/{userId}")]
        public bool RemoveUserFromRepository(Guid userId, Guid repoId)
        {
            return _repoBusiness.RemoveUserFromRepository(userId,repoId);                        
        }

        [HttpGet]
        [Route("api/repositories/{repoId}/add/{userName}")]
        public RepositoryUser AddUserToRespository(Guid repoId, string userName)
        {                           
            var repoUser = _repoBusiness.AddContributorToRepository(repoId, userName);
            if (repoUser == null)
                throw new ArgumentException("Invalid username");

            return repoUser;
        }


        [HttpGet]
        [Route("api/repositories/{repoId}/users")]
        public async Task<ActionResult> UsersForRepository(Guid repoId)
        {
            var list = await _repoBusiness.GetUsersForRepositoryAsync(repoId);

            // fix circular ref
            foreach (var repoUser in list)
            {
                // Why TF are these pulled without an Include?
                repoUser.User.Repositories = null;
            }

            return Json(list);
        }

        [HttpGet]
        [Route("api/repositories/{repoId}")]
        public ActionResult GetUserJson(Guid repoId)
        {
            return Json(_repoBusiness.GetRepository(repoId));            
        }

    }

    public class RepositoriesListViewModel : AppBaseViewModel
    {
        public List<Repository> Repositories { get; set; }
    }

    public class RepositoryViewModel : AppBaseViewModel
    {
        
        public Repository Repository { get; set; }

        public bool IsOwner { get; set; }

        public string SettingsJson { get; set; }

        public string TableOfContentsJson { get; set; }
    }
}