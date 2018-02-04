using System;
using System.Linq;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class OrganizationBusinessTests
    {        

        [Test]
        public void GetOrganizationTest()
        {
            var orgBusiness = TestHelper.GetOrganizationBusiness();

            var firstId = orgBusiness.Context.Organizations.Select(r=> r.Id).FirstOrDefault();
            Console.WriteLine(firstId);

            var org = orgBusiness.Load(firstId);            
            Assert.IsNotNull(org,orgBusiness.ErrorMessage);
        }

        [Test]
        public void CreateOrganizationTest()
        {
            var orgBus = TestHelper.GetOrganizationBusiness();
            
            var org = new Organization
            {
                Title = "West Wind Organization",
                Description = "Doing great things without actually doing it.",
            };
            orgBus.Context.Organizations.Add(org);            
            Assert.IsTrue(orgBus.Save());
        }

        [Test]
        public void AddRepositoryToOrganization()
        {

            var orgBus = TestHelper.GetOrganizationBusiness();
            var repoId = orgBus.Context.Repositories.Select(r => r.Id).FirstOrDefault();
            Assert.IsNotNull(repoId);
            var orgId = orgBus.Context.Organizations.Select(r => r.Id).FirstOrDefault();
            Assert.IsNotNull(orgId);

            orgBus.AddRepositoryToOrganization(orgId, repoId);
        }
        
    }
}
