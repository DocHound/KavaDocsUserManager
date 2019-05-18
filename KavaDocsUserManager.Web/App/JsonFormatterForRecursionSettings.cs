using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using KavaDocsUserManager.Business.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace KavaDocsUserManager.Web.App
{
    public class AppUtils
    {


        public static JsonSerializerSettings JsonFormatterSettingsForRecursive
        {
            get
            {
                if (jsonSerializerSettings != null)
                    return jsonSerializerSettings;

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
#if DEBUG
                    Formatting = Formatting.Indented
#endif

                };
                settings.Converters.Add(new StringEnumConverter() { NamingStrategy = new CamelCaseNamingStrategy()});
                
                jsonSerializerSettings = settings;
                return settings;
            }
        }
        private static JsonSerializerSettings jsonSerializerSettings;


        /// <summary>
        /// Sends a user administration email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool SendEmail(string email, string title, string body, out string error)
        {
            var config = KavaDocsConfiguration.Current.Email;

            var smtp = new Westwind.Utilities.InternetTools.SmtpClientNative()
            {
                MailServer = config.MailServer,
                Username = config.MailServerUsername,
                Password = config.MailServerPassword,
                UseSsl = config.UseSsl,
                SenderEmail = config.SenderEmail,
                SenderName = config.SenderName,
                Message = body,
                Subject = title,
                Recipient = email
            };

            error = null;
            bool success  = smtp.SendMail();
            if (!success)
                error = smtp.ErrorMessage;
            
            return success;
        }
    }
}
