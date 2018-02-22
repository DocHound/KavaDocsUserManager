using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Westwind.Utilities.InternetTools;

namespace KavaDocsUserManager.Business.Tests
{
    [TestFixture]
    public class SmtpTests
    {
        private KavaDocsConfiguration configuration;        
        
        [SetUp]
        public void Init()
        {
            configuration = TestHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void SendEmailTest()
        {           
            var smtp = new SmtpClientNative();
            smtp.MailServer = configuration.Email.MailServer; //"smtp.mailgun.org";
            smtp.Username = configuration.Email.MailServerUsername; // "postmaster@west-wind.com";
            smtp.Password = configuration.Email.MailServerPassword; //"22429016c37a623c68b396f08726084d";

            smtp.SenderEmail = "West Wind Technologies <info@west-wind.com>";
            smtp.Recipient = "rstrahl13@gmail.com";

            smtp.Message = "Hello from Mail Gun. This is a test";
            smtp.Subject = "Mailgun Test Message";

            Assert.IsTrue(smtp.SendMail(),smtp.ErrorMessage);
        }
        

    }
}
