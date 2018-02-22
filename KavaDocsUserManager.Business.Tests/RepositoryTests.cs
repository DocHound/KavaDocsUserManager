using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class RepositoryTests
    {

        [Test]
        public void RepositorySafeNameTest()
        {
            var repoBus = TestHelper.GetRepositoryBusiness();

            var prefix = "MyDocs";
            var result = repoBus.SafeRepositoryName(prefix);
            Assert.AreEqual(prefix, result);            

            prefix = "westwind.mydocs";
            result = repoBus.SafeRepositoryName(prefix);
            Console.WriteLine(result);
            Assert.AreEqual("westwindmydocs", result);

            prefix = "1westwinddocs";
            result = repoBus.SafeRepositoryName(prefix);
            Console.WriteLine(result);
            Assert.AreEqual("westwinddocs", result);

            prefix = "westwind-markdownmonster-docs";
            result = repoBus.SafeRepositoryName(prefix);
            Console.WriteLine(result);
            Assert.AreEqual("westwind-markdownmonster-docs", result);

        }

        [Test]
        public async void GetRepositoriesForUserTest()
        {
            var repoBus = TestHelper.GetRepositoryBusiness();

            var respos = await repoBus.GetRepositoriesForUserAsync(TestHelper.UserId2);
            Assert.IsNotNull(respos, repoBus.ErrorMessage);
            Assert.IsTrue(respos.Count > 0);
        }

        [Test]
        public async Task GetRepositoriesForUser2Test()
        {
            var repoBus = TestHelper.GetRepositoryBusiness();

            var userId = TestHelper.UserId2;

            var repos = await repoBus.Context.Repositories
                .Include(c => c.Users)
                .Where(r => r.Users.Any(u => u.UserId == userId))
                .ToListAsync();

            Assert.IsTrue(repos != null && repos.Count > 0, repoBus.ErrorMessage);

        }
    }

    
}
