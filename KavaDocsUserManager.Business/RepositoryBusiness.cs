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

        protected override bool OnValidate(Repository repo)
        {
            var safeTitle = SafeRepositoryName(repo.Prefix);
            if (string.IsNullOrEmpty(safeTitle) || safeTitle.Length != repo.Prefix.Length)
            {
                ValidationErrors.Add(
                    "The repository prefix can only contain letters and numbers and cannot start with a number.",
                    "Prefix");
            }


            return true;
        }

        public Repository GetRepository(Guid id)
        {
            return Context.Repositories
                .Include(r=> r.Users).ThenInclude(ur=> ur.User)
                .FirstOrDefault(r => r.Id == id);
        }


        public bool AddContributorToRepository(Guid repoId, Guid userId)
        {
            var repo = Context.Repositories.FirstOrDefault(u => u.Id == repoId);
            if (repo == null)
            {
                SetError("Respositor to add user to doesn't exist.");
                return false;
            }

            var user = Context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                SetError("User to add to repository doesn't exist.");
                return false;
            }                
            
            if(Context.UserRepositories.Any(c => c.RepositoryId == repoId && c.UserId == userId))            
                return true; // already an owner or contributor

            var map = new RepositoryUser()
            {
                RepositoryId = repo.Id,                
                UserId = user.Id,
                IsOwner = false
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

            return Save();
        }


        public bool DeleteRepository(Guid repoId)
        {

            // remove users for that repository
            var users = Context.UserRepositories.Where(u => u.Id == repoId);
            Context.UserRepositories.RemoveRange(users);

            var repo = Context.Repositories.FirstOrDefault(r => r.Id == repoId);
            if (repo != null)
                Context.Repositories.Remove(repo);

            AutoValidate = false;
            return Save();

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
            return await Context.Repositories
                .Include(c=> c.Users)
                .Where(r => r.Users.Any(u=> u.UserId == userId))                         
                .ToListAsync();
        }
    }

}
