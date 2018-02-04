using System;
using System.Linq;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class UserBusinessTests
    {        
        [Test]
        public void GetUserTest()
        {
            //var context = TestHelper.GetContext();
            //context.Database.ExecuteSqlCommand("drop table users; drop table repositories; drop table organizations");

            var userBus = TestHelper.GetUserBusiness();

            var id = new Guid("11111111-0589-4951-ad11-dae7fb1566cb");  
            var user = userBus.GetUser(id);

            Assert.IsNotNull(user);
        }

        [Test]
        public void AddRepositoryToUser()
        {
            var userBus = TestHelper.GetUserBusiness();

            var id = new Guid("11111111-0589-4951-ad11-dae7fb1566cb");
            var user = userBus.Load(id);

            Assert.IsNotNull(user);

            var repo = new Repository()
            {
                Prefix = "markdownmonster",
                Title = "Markdown Monster Documentation",
                Description = "User documentation for the Markdown Monster Desktop Application",
                IsActive = true,
                Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/RickStrahl/MarkdownMonster/master/Docs/\" }"
            };

            userBus.AddRepositoryToUser(user.Id, repo);

            
            Assert.IsTrue(userBus.Save(), userBus.ErrorMessage);
        }

        [Test]
        public void RemoveRepositoryToUser()
        {
            var userBus = TestHelper.GetUserBusiness();

            var map = userBus.Context.UsersRepositories.FirstOrDefault();


            Assert.IsTrue(userBus.RemoveRepositoryFromUser(map.UserId, map.RepositoryId),userBus.ErrorMessage);
        }


    }
}
