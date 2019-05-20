using System;
using System.Linq;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class RepositoryBusinessTests
    {        
        [Test]
        public void GetRepositoryTest()
        {
            //var context = TestHelper.GetContext();
            //context.Database.ExecuteSqlCommand("drop table users; drop table repositories; drop table organizations");

            var repoBus = TestHelper.GetRepositoryBusiness();

            var firstId = repoBus.Context.Repositories.Select(r=> r.Id).FirstOrDefault();
            Console.WriteLine(firstId);

            var repo = repoBus.GetRepository(firstId);            
            Assert.IsNotNull(repo,repoBus.ErrorMessage);
        }


        [Test]
        public void GetRepositoryAndSaveTest()
        {
            //var context = TestHelper.GetContext();
            //context.Database.ExecuteSqlCommand("drop table users; drop table repositories; drop table organizations");

            var repoBus = TestHelper.GetRepositoryBusiness();

            var firstId = repoBus.Context.Repositories.Select(r => r.Id).FirstOrDefault();
            Console.WriteLine(firstId);

            var repo = repoBus.GetRepository(firstId);
            Assert.IsNotNull(repo, repoBus.ErrorMessage);


            repo.Description += "!";

            Assert.IsTrue(repoBus.Save(), repoBus.ErrorMessage);
        }



        [Test]
        public void AddContributorToRepositoryTest()
        {

            var userId = TestHelper.UserId2;
            var repoBus = TestHelper.GetRepositoryBusiness();

            var firstId = repoBus.Context.Repositories.Select(r => r.Id).FirstOrDefault();
            Console.WriteLine(firstId);

            Assert.IsNotNull(repoBus.AddContributorToRepository(firstId, userId,RepositoryUserTypes.Contributor));
        }


        [Test]
        public void DeleteRepositoryTest()
        {
            var repoBus = TestHelper.GetRepositoryBusiness();
            var firstId = repoBus.Context.Repositories.Select(r => r.Id).FirstOrDefault();

            Assert.IsTrue(repoBus.DeleteRepository(firstId));

        }


        [Test]
        public async Task GetRespositoriesForUserTest()
        {
            var repoBusiness = TestHelper.GetRepositoryBusiness();
            var list = await repoBusiness.GetUsersForRepositoryAsync( TestHelper.RepoId);

            foreach(var ru in list)
            {
                Console.WriteLine(ru.User.UserDisplayName + " " + ru.UserType);
            }

        }
    }
}
