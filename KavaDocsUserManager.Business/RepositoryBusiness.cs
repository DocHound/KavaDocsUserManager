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

            var repoUser = new RepositoryUser { RepositoryId = repo.Id, UserId = uid, UserType = RepositoryUserType.Owner };
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

            repo.Users = repo.Users.OrderByDescending(ur => ur.UserType).ToList();
            
            // sort owners to the top
            if (repo.Users != null && repo.Users.Count > 0)
                repo.Users = repo.Users.OrderBy(ur => !ur.IsOwner).ToList();

            return repo;
        }

        public bool IsOwner(Repository repo, Guid userId)
        {
            return repo.Users.Any(r => r.IsOwner && r.UserId == userId);
        }



        public RepositoryUser AddContributorToRepository(Guid repoId, Guid userId, RepositoryUserType userType)
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
                UserType = userType                
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

        public RepositoryUser AddContributorToRepository(Guid repoId, string username, RepositoryUserType userType)
        {

            if (userType == RepositoryUserType.None)
                userType = RepositoryUserType.User;

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

            return AddContributorToRepository(repoId, userId, userType);
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

        public async Task<List<RepositoryUser>> GetUsersForRepositoryAsync(Guid repoId)
        {
                var users = await Context.UserRepositories
                                .Include(c=> c.User)
                                .Where(rep => rep.RepositoryId == repoId)
                                .OrderByDescending(r => r.UserType)
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

        
    }

}
