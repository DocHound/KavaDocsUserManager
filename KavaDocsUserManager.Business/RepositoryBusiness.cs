using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using Westwind.Data.EfCore;
using Westwind.Utilities;

namespace KavaDocsUserManager.Business
{
    public class RepositoryBusiness : EntityFrameworkBusinessObject<KavaDocsContext, Repository>
    {
        private KavaDocsConfiguration Configuration;

        public RepositoryBusiness(KavaDocsContext context, KavaDocsConfiguration config) : base(context)
        {
            Context = context;
            Configuration = config;
        }


        /// <summary>
        /// Overrides Create for a repository that also adds a new
        /// user/owner
        /// </summary>
        /// <returns></returns>
        public Repository CreateRepository(Guid uid)
        {
            var repo = Create();
            if (repo == null)
                return null;

            var repoUser = new RepositoryUser { RepositoryId = repo.Id, UserId = uid, UserTypes = RepositoryUserTypes.Owner };
            //Context.UserRepositories.Add(repoUser);
            repo.Users.Add(repoUser);

            return repo;
        }

        protected override bool OnValidate(Repository repo)
        {
            var safeTitle = SafeRepositoryName(repo.Prefix);
            if (string.IsNullOrEmpty(safeTitle) || safeTitle.Length != repo.Prefix.Length)
            {
                ValidationErrors.Add(
                    "The repository prefix can only contain letters and numbers and cannot start with a number.",
                    "Prefix");
            }


            if (string.IsNullOrEmpty(repo.Title))
                ValidationErrors.Add("Title can't be left blank", "Title");
            
            // TODO: Force JSON Configuration and Validation JSON

            bool? isNew = IsNewEntity(repo);

            if (isNew.HasValue && isNew.Value)
            {
                if (Context.Repositories.Any(r => r.Prefix == safeTitle))
                {
                    ValidationErrors.Add("The repository prefix " + safeTitle +
                                         " already exists. Please choose another prefix.","Prefix");
                }
            }


            return ValidationErrors.Count == 0;
        }

        public Repository GetRepository(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            var repo = Context.Repositories
                .Include(r=> r.Users).ThenInclude(ur=> ur.User)
                .FirstOrDefault(r => r.Id == id);

            if (repo == null)
            {
                SetError("Invalid Repository Id");
                return null;
            }

            repo.Users = repo.Users.OrderByDescending(ur => ur.UserTypes).ToList();
            
            // sort owners to the top
            if (repo.Users != null && repo.Users.Count > 0)
                repo.Users = repo.Users.OrderBy(ur => !ur.IsOwner).ToList();

            return repo;
        }

        public bool IsOwner(Repository repo, Guid userId)
        {
            return repo.Users.Any(r => r.IsOwner && r.UserId == userId);
        }



        public RepositoryUser AddContributorToRepository(Guid repoId, Guid userId, RepositoryUserTypes userTypes)
        {
            var repo = Context.Repositories.FirstOrDefault(u => u.Id == repoId);
            if (repo == null)
            {
                SetError("Respositor to add user to doesn't exist.");
                return null;
            }

            var user = Context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                SetError("User to add to repository doesn't exist.");
                return null;
            }

            var map = Context.UserRepositories
                .Include(ur=> ur.User)
                .FirstOrDefault(c => c.RepositoryId == repoId && c.UserId == userId);
            if (map != null)
            {
                SetError("User is already a contributor or owner of this repository");
                return null;
                ; // already an owner or contributor
            }

            map = new RepositoryUser()
            {
                RepositoryId = repo.Id,                
                UserId = user.Id,
                UserTypes = userTypes                
            };
            repo.Users.Add(map);

            //// already a contributor
            //if (Context.RepositoryUser.Any(c => c.RepositoryId == repoId && c.UserId == userId))
            //    return true;

            //// User is the owner
            //if (Context.UserRepositories.Any(c => c.RepositoryId == repoId && c.UserId == userId))
            //    return true;

            //var map = new RepositoryContributor()
            //{
            //    RepositoryId = repo.Id,
            //    UserId = user.Id
            //};
            //repo.Contributors.Add(map);

            if (!Save())
                return null;

            map.User = user;
            map.Repository = repo;

            return Context.UserRepositories.Include(ur => ur.User).FirstOrDefault(ur => ur.Id == map.Id);
        }

        public RepositoryUser AddContributorToRepository(Guid repoId, string username, RepositoryUserTypes userTypes)
        {

            if (userTypes == RepositoryUserTypes.None)
                userTypes = RepositoryUserTypes.User;

            if (string.IsNullOrEmpty(username))
            {
                SetError("Invalid username");
                return null;
            }

            var userId = Context.Users
                .Where(u => u.UserDisplayName == username)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(username))
            {
                SetError("Invalid username");
                return null;
            }

            return AddContributorToRepository(repoId, userId, userTypes);
        }


        public bool DeleteRepository(Guid repoId)
        {

            // remove users for that repository
            var users = Context.UserRepositories.Where(u => u.Id == repoId);
            Context.UserRepositories.RemoveRange(users);

            var repo = Context.Repositories.FirstOrDefault(r => r.Id == repoId);
            if (repo != null)
                Context.Repositories.Remove(repo);

            var oldValidate = AutoValidate;
            AutoValidate = false;
            return Save();

            AutoValidate = oldValidate;
        }

        public string SafeRepositoryName(string title)
        {            
            if (string.IsNullOrEmpty(title))
                return null;

            title = WebUtility.HtmlDecode(title).Trim();

            StringBuilder sb = new StringBuilder();

            bool stripStartNumber = true;
            foreach (char ch in title)
            {
                if (stripStartNumber)
                {
                    if (char.IsNumber(ch))
                        continue;
                    stripStartNumber = false;
                }

                if (ch == 32)
                    sb.Append("-");    
                else if (ch == '-')
                    sb.Append('-');
                else if (char.IsLetterOrDigit(ch))
                    sb.Append(ch);

                // everything else is stripped
            }

            title = sb.ToString();

            title.Replace("---", "-");
            title.Replace("--", "-");

            if (string.IsNullOrEmpty(title))
                return null;

            return title;
        }

        #region Repository Users

        public async Task<List<Repository>> GetRepositoriesForUserAsync(Guid userId)
        {            
            var list = await Context.Repositories
                .Include(c=> c.Users)
                .Where(r => r.Users.Any(u=> u.UserId == userId))                
                .ToListAsync();

            list = list
                .OrderBy(r =>
                {                       
                    if (r.Users == null)
                        return 1;
                    bool isOwner = r.Users
                            .Any(ur => ur.IsOwner && ur.UserId == userId);
                    return isOwner ? 0 : 1;
                })
                .ThenBy(r=> r.Title)
                .ToList();

            return list;
        }


        /// <summary>
        /// Returns a list of users for this repository
        /// </summary>
        /// <param name="repoId"></param>
        /// <returns></returns>
        public async Task<List<RepositoryUser>> GetUsersForRepositoryAsync(Guid repoId)
        {
                var users = await Context.UserRepositories
                                .Include(c=> c.User)
                                .Where(rep => rep.RepositoryId == repoId)
                                .OrderByDescending(r => r.UserTypes)
                                .ToListAsync();
            return users;
        }


        
        public bool RemoveUserFromRepository(Guid userId, Guid repoId)
        {
            var userRepo = Context.UserRepositories.FirstOrDefault(ur => ur.UserId == userId && ur.RepositoryId == repoId);
            if (userRepo == null)
                return true;

            Context.UserRepositories.Remove(userRepo);
            return Context.SaveChanges() > -1;
        }

#endregion


        #region Roles

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public async Task<List<Role>> GetRolesForRepository(Guid repositoryId)
        {
            var list = await Context.UserRoles
                .Include(r => r.Role)
                .Where(ur => ur.RepositoryId == repositoryId)
                .Select(ur => ur.Role)
                .Distinct()
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Gets a list of users for a repository along with its associated roles
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        public async Task<List<UserRolesResponse>> GetUserRolesForRepository(Guid repositoryId)
        {
            var userRepoList = await
                (from uroles in Context.UserRoles
                    from urepos in Context.UserRepositories
                    where uroles.RepositoryId == repositoryId &&
                          urepos.RepositoryId == repositoryId &&
                          uroles.UserId == urepos.UserId

                    select new
                    {
                        UserId = uroles.User.Id,
                        Username = uroles.User.UserDisplayName,
                        Rolename = uroles.Role.Name,
                        RoleId = uroles.Role.Id,
                        RepositoryName = urepos.Repository.Title,
                        RepositoryId = repositoryId,
                        IsOwner = urepos.IsOwner,
                        UserType = urepos.UserTypes
                    })
                .OrderBy(ur => ur.Username)
                .ToListAsync();

            // get all the roles
            var roles = await Context.UserRoles
                .Where(r => r.RepositoryId == repositoryId)
                .Select(r=> r.Role)
                .Distinct()
                .ToListAsync();


            

            var result = new List<UserRolesResponse>();

            var uList =
                userRepoList
                    .Select(ur => new {ur.UserId, ur.Username, ur.UserType, ur.IsOwner, ur.RepositoryName, ur.RepositoryId})
                    .Distinct();

            foreach (var user in uList)
            {
                var userRole = new UserRolesResponse
                {
                    Username = user.Username,
                    UserId = user.UserId,
                    Roles = new List<RoleResponse>(),
                    IsOwner = user.IsOwner,
                    UserTypes = user.UserType,
                    RepositoryId = user.RepositoryId,
                    RepositoryName = user.RepositoryName
                };

                foreach (var role in roles)
                {
                    var roleResponse = new RoleResponse
                    {
                        RoleId = role.Id,
                        Rolename = role.Name,
                    };
                    roleResponse.Selected = userRepoList.Any(ur => ur.UserId == user.UserId && ur.RoleId == role.Id);

                    userRole.Roles.Add(roleResponse);
                }

                //userRole.Roles = userRepoList
                //    //.Where(ur => ur.UserId == user.UserId)
                //    .Select(ur => new RoleResponse
                //    {
                //        Rolename = ur.Rolename,
                //        RoleId = ur.RoleId,
                //        Selected = ur.UserId == user.UserId
                //    })
                //    .Distinct()
                //    .ToList();

                result.Add(userRole);
            }

            return result;
        }
        

        public async Task<bool> AddRoleToRepository(Guid repositoryId, Guid roleId, Guid userId)
        {
            var role = new RoleUserRepository()
            {
                RepositoryId = repositoryId,
                RoleId = roleId,
                UserId = userId
            };
            Context.UserRoles.Add(role);

            return await SaveAsync();
        }

        public async Task<int> AddRoleToRepository(Guid repositoryId, Guid userId, Role role)
        {
            var existing = Context.UserRoles
                    .FirstOrDefault(ur => ur.RepositoryId == repositoryId &&
                                      ur.Role.Name == role.Name);

            if (existing != null)
                role = existing.Role;
            
            var link = new RoleUserRepository()
            {
                RepositoryId = repositoryId,
                RoleId = role.Id,
                UserId = userId
            };

            bool exists = await Context.UserRoles.AnyAsync(ur => ur.RepositoryId == link.RepositoryId &&
                                        ur.RoleId == link.RoleId &&
                                        ur.UserId == link.UserId);
            if (!exists)
                Context.UserRoles.Add(link);
            else
                Context.Attach(link);

            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveUserRole(Guid repositoryId, Guid userId, Guid roleId)
        {

           var userRole = await Context.UserRoles.FirstOrDefaultAsync( ur=> ur.RepositoryId == repositoryId &&
                                                  ur.RoleId == roleId &&
                                                  ur.UserId == userId);
            if (userRole == null)
                return true;

            Context.UserRoles.Remove(userRole);

            int result = await Context.SaveChangesAsync();
                
            return result > 0 ? true : false;
        }

        #endregion

    }

    public class UserRolesResponse
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public bool IsOwner { get; set;  }
        public RepositoryUserTypes UserTypes { get; set; }

        public Guid RepositoryId { get; set; }

        public string RepositoryName { get; set; }

        public List<RoleResponse> Roles { get; set; } = new List<RoleResponse>();
        
    }

    public class RoleResponse
    {
        public Guid RoleId { get; set; }
        public string Rolename { get; set; }

        public bool Selected { get; set; }
    }
}
