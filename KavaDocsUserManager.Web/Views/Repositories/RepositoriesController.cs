using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Westwind.AspNetCore.Errors;
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
        public ActionResult Repository(Guid id, string message = null)
        {
            var model = CreateViewModel<RepositoryViewModel>();
            var appUser = User.GetAppUser();
            bool isNew = id == Guid.Empty;
            
            // just in case - we can't add unless signed in
            if (!isNew && !appUser.IsAuthenticated())            
                return RedirectToAction("SignIn","Account");            
            
            if(!string.IsNullOrEmpty(message))
                model.ErrorDisplay.ShowError(message);
            
            var repo = _repoBusiness.GetRepository(id);
            if (repo == null)
            {
                if (isNew)
                {   
                    repo = _repoBusiness.Create();
                    repo.Title = "New Documentation Repository";                    
                    model.Repository = repo;
                    model.IsNewUser = true;
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
            {
                repo = _repoBusiness.CreateRepository(User.GetAppUser().UserId);
                model.IsNewUser = true;
            }

            DataUtils.CopyObjectData(model.Repository, repo, "Id,Users,Organization");            

            repo.Settings = model.SettingsJson;
            repo.TableOfContents = model.TableOfContentsJson;
            
            bool validationResult = _repoBusiness.Validate(repo);
            if (!validationResult)
            {
                model.ErrorDisplay.AddMessages(_repoBusiness.ValidationErrors);
                model.ErrorDisplay.ShowError("Please fix the following");
                return View("Repository",model);
            }

            
            _repoBusiness.AutoValidate = false;
            if (!_repoBusiness.Save())
                model.ErrorDisplay.ShowError(_repoBusiness.ErrorMessage, "Couldn't save Repository.");
            else            
                model.ErrorDisplay.ShowSuccess("Repository info saved.");


            if (model.IsNewUser)
            {
                model.IsNewUser = false;
                // do a full reload to ensure all dependencies get loaded
                repo = _repoBusiness.GetRepository(repo.Id);                
            }

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
        [Route("repositories/{id}/delete")]
        public ActionResult DeleteRespository(Guid id)
        {            
            var appUser = User.GetAppUser();

            var repo = _repoBusiness.Load(id);
            if (repo == null)            
                return RedirectToAction("Repository", new {id = id, message = "Can't delete this repository."});

            // only owner can delete
            if (!repo.IsOwner(appUser.UserId))
            {
                return RedirectToAction("Repository", new
                    {
                        id = id,
                        message = $"Only the owner can delete this respository."
                    }
                );
            }
            
            if (!_repoBusiness.DeleteRepository(id))
            {
                return RedirectToAction("Repository", new
                    {
                        id = id,
                        message = $"Can't delete this repository: {_repoBusiness.ErrorMessage}"
                    }
                );
            }

            return RedirectToAction("Index");
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
        public bool IsNewUser { get; set; }


        public string SettingsJson { get; set; }
        public string TableOfContentsJson { get; set; }
        public string CssOverride { get; set; }

        public string SelectedOptionalField { get; set; } = "TableOfContents";




    }
}