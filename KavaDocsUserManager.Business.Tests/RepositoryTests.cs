using System;
using System.Collections.Generic;
using System.Text;
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

    }
}
