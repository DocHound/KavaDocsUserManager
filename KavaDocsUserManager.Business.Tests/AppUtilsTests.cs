using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using KavaDocsUserManager.Web.App;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class AppUtilsTests
    {

        [Test]
        public void EmailTest()
        {
            var config = KavaDocsConfiguration.Current = new KavaDocsConfiguration();

            config.Email.MailServer = "localhost";  // use PaperCut https://github.com/ChangemakerStudios/Papercut/releases
            config.Email.MailServerUsername = null;
            config.Email.MailServerPassword = null;
            config.Email.UseSsl = false;
            config.Email.SenderEmail = "info@kavadocs.com";
            config.Email.SenderName = "Kava Docs";

            Assert.IsTrue(AppUtils.SendEmail("rstrahl13@gmail.com", "Test Email", "Please validate your email.", out string error), error);
        }

    }
}
