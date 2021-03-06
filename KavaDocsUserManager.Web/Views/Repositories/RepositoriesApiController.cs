﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business;
using KavaDocsUserManager.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
        public async Task<bool> RemoveUserFromRepository(Guid userId, Guid repoId)
        {
            return  await _repoBusiness.RemoveUserFromRepository(userId, repoId);
        }
        
        [HttpGet]
        [Route("api/repositories/{repoId}/add/{userName}/{userType}")]
        public UserRolesResponse AddUserToRespository(Guid repoId, string userName, string userType)
        {            
            Enum.TryParse<RepositoryUserTypes>(userType, true, out RepositoryUserTypes utype);
            
            var repoUser = _repoBusiness.AddContributorToRepository(repoId, userName,utype);
            if (repoUser == null)
                throw new ApiException( _repoBusiness.ErrorMessage);

            return new UserRolesResponse
            {
                UserId = repoUser.UserId,
                RepositoryId = repoUser.RepositoryId,
                Username = repoUser.User.UserDisplayName,
                IsOwner = repoUser.IsOwner,
                RepositoryName = repoUser.Repository.Title,
                UserType = repoUser.UserType
            };
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
        [Route("api/repositories/{id}")]
        public bool DeleteRepository(Guid id)
        {
            var appUser = User.GetAppUser();

            var repo = _repoBusiness.GetRepository(id);
            if (repo == null)            
                throw new ApiException("Can't delete this repository.",404);

            
            // only owner can delete
            if (!repo.IsOwner(appUser.UserId))            
                throw new ApiException("Only the owner can delete this respository.",404);                            
            
            if (!_repoBusiness.DeleteRepository(id))            
                throw new ApiException($"Can't delete this repository: {_repoBusiness.ErrorMessage}");
                                   
            if (!_repoBusiness.DeleteRepository(id))
                throw new ApiException($"Can't delete this repository: {_repoBusiness.ErrorMessage}",500);

            return true;
        }




        [HttpGet]
        [Route("api/repositories/{repoId}/users")]
        public async Task<ActionResult> UsersForRepository(Guid repoId)
        {
            var list = await _repoBusiness.GetUsersForRepositoryAsync(repoId);

            // fix circular ref
            foreach (var repoUser in list)
                // Why TF are these pulled without an Include?
                repoUser.User.Repositories = null;

            return Json(list);
        }

        [HttpGet]
        [Route("api/repositories/{repoId}")]
        public async Task<RepositoryResponse> GetRepository(Guid repoId)
        {
            var response = await _repoBusiness.GetRepositoryWithUsersAndRoles(repoId);
            return response;
        }

        [HttpGet]
        [Route("api/repositories/{repoId}/roles")]
        public async Task<List<Role>> RolesForRepository(Guid repoId)
        {
            var list = await _repoBusiness.GetRolesForRepository(repoId);
            return list;
        }

        [HttpGet]
        [Route("api/repositories/{repoId}/userroles")]
        public async Task<RepositoryResponse> UserRolesForRepository(Guid repoId)
        {
            var response = await _repoBusiness.GetRepositoryWithUsersAndRoles(repoId);
            return response;
        }

        [HttpDelete]
        [Route("api/repositories/{repoId}/userroles/{roleId}")]
        public async Task<bool> DeleteRoleFromRepository(Guid repoId,Guid roleId)
        {
            return  await _repoBusiness.DeleteRoleFromRepository(repoId, roleId);
        }

        [HttpGet]
        [Route("api/repositories/{repoId}/updaterole/{userId}/{roleId}/{isSelected}")]
        public async Task<bool> UpdateUserRoleOnRepository(Guid repoId, Guid userId, 
            Guid roleId, 
            bool isSelected)
        {
            return await _repoBusiness.UpdateUserRoleOnRepository(repoId, userId, roleId, isSelected);
        }

        [HttpPost]
        [Route("api/repositories/{repoId}/role/add")]
        public async Task<Role> AddRoleToRepository([FromRoute] Guid repoId, [FromBody] Dictionary<string, object> parameters)
        {

            var roleName = parameters["roleName"] as string;
            if (string.IsNullOrEmpty(roleName))
                throw new ApiException("Role to add cannot be empty.");
            
            var role = await _repoBusiness.AddRoleToRepository(repoId, roleName);
            if(role == null)
                throw new ApiException(_repoBusiness.ErrorMessage);


            return role;
        }
            
        public class AddRoleRequestModel
        {
            public string roleName { get; set; }
        }


    }
}