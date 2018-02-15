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

namespace KavaDocsUserManager.Web.Views.Repositories
{

    [Authorize]
    [ApiExceptionFilter]
    public class RepositoriesApiController :Controller
    {
        private readonly RepositoryBusiness _repoBusiness;

        public RepositoriesApiController(RepositoryBusiness repoBusiness)
        {
            _repoBusiness = repoBusiness;
        }
        
        [HttpGet]
        [Route("api/repositories/{repoId}/remove/{userId}")]
        public bool RemoveUserFromRepository(Guid userId, Guid repoId)
        {
            return _repoBusiness.RemoveUserFromRepository(userId, repoId);
        }
        
        [HttpGet]
        [Route("api/repositories/{repoId}/add/{userName}")]
        public RepositoryUser AddUserToRespository(Guid repoId, string userName)
        {
            var repoUser = _repoBusiness.AddContributorToRepository(repoId, userName);
            if (repoUser == null)
                throw new ApiException("Didn't add user to repository: " + _repoBusiness.ErrorMessage);

            return repoUser;
        }

        
        [HttpGet]
        [Route("api/repositories/searchusers/{searchString}")]
        public async Task<List<string>> AddUserToRespository(string searchString)
        {
            var userBus = HttpContext.RequestServices.GetService(typeof(UserBusiness)) as UserBusiness;
            var list = await userBus.Context.Users
                .Where(u => u.UserDisplayName.Contains(searchString))
                .Select(u => u.UserDisplayName)
                .ToListAsync<string>();

            return list;
        }

        [HttpDelete]
        [Route("api/repositories/{repoId}")]
        public bool DeleteRepository(Guid repoId)
        {
            return _repoBusiness.DeleteRepository(repoId);
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
}