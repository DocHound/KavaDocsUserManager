﻿using System;
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
        public void DropDataBaseTest()
        {
            // MAKE SURE THIS ISN"T THE LIVE DATABASE
            if (!KavaDocsConfiguration.Current.ConnectionString.Contains("server=.",
                StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException("Drop is allowed only against a local database.");

            


            string sqlDrop = @"
//drop table AuthTokens
//drop table UserRepositories
//drop table OrganizationRepositories
//drop table UserRoles

//drop table users
//drop table Repositories
//drop table Organizations
//drop table Rows

//drop table __EFMigrationsHistory

//Drop D
Drop Database KavaDocs
";
            var ctx = TestHelper.GetContext();
            ctx.Database.ExecuteSqlCommand(sqlDrop);

        }
        [Test]
        public void RegenerateDataTest()
        {
            var userBus = TestHelper.GetUserBusiness();

            var id = TestHelper.UserId1;
            var user = userBus.GetUser(id);

            Assert.IsNotNull(user);
        }

        [Test]
        public void GetUserTest()
        {
            var userBus = TestHelper.GetUserBusiness();

            var id = TestHelper.UserId1;
            var user = userBus.GetUser(id);

            Assert.IsNotNull(user);
        }

        [Test]
        public void AddRepositoryToUserTest()
        {
            var userBus = TestHelper.GetUserBusiness();

            var id = TestHelper.UserId1;
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

            userBus.CreateRepositoryForUser(user.Id, repo);

            
            Assert.IsTrue(userBus.Save(), userBus.ErrorMessage);
        }

        [Test]
        public void RemoveRepositoryToUserTest()
        {
            var userBus = TestHelper.GetUserBusiness();

            var map = userBus.Context.UserRepositories.FirstOrDefault();


            Assert.IsTrue(userBus.DeleteRepository(map.RepositoryId),userBus.ErrorMessage);
        }


        [Test]
        public void AuthenticateUserTest()
        {
            var userBus = TestHelper.GetUserBusiness();

            var user = userBus.AuthenticateAndRetrieveUser("rstrahL@west-wind.com","testing");
            Assert.IsNotNull(user);
            Assert.AreEqual(user.Email, "rstrahl@west-wind.com");


            Assert.IsTrue(userBus.AuthenticateUser("rstrahL@west-wind.com", "testing"),userBus.ErrorMessage);
        }

        [Test]
        public void GetUserRoles()
        {
            var userBus = TestHelper.GetUserBusiness();
            var user = userBus.GetUser(TestHelper.UserId1);

            Assert.IsNotNull(user);

            var roles = userBus.GetRepositoryRoles(user.Id, TestHelper.RepoId);
            
            Assert.IsNotNull(roles);
            Assert.IsTrue(roles.Count > 0);

        }

    }
}
