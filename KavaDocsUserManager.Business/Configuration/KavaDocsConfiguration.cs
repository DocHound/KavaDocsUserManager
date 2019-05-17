using System;
using System.Collections.Generic;
using System.Text;

namespace KavaDocsUserManager.Business.Configuration
{
    public class KavaDocsConfiguration
    {
        public static KavaDocsConfiguration Current { get; set; }

        /// <summary>
        /// Display name for this application/blog
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Sql Server ConnectionString for this application
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The server relative root path for this application
        /// </summary>
        public string ApplicationBasePath { get; set; } = "/";

        public string ApplicationHomeUrl { get; set; } = "/";

        public static Guid EmptyGuid { get; } = Guid.Empty;

        public EmailConfiguration Email { get; set; } = new EmailConfiguration();

    }

    public class EmailConfiguration
    {
        public string MailServer { get; set; }
        public string MailServerUsername { get; set; }

        public string MailServerPassword { get; set; }

        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string AdminSenderEmail { get; set; }
        public bool UseSsl { get; set; }
    }

}
