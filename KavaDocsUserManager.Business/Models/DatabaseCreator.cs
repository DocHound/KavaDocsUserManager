using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KavaDocsUserManager.Business.Models
{
    public class DatabaseCreator
    {
        /// <summary>
        /// Ensures that the database and table structure exists
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool EnsureKavaDocsData(KavaDocsContext context)
        {
            bool hasData = false;
            try
            {
                // Force database to get hit
                hasData = context.Users.Any();
            }
            catch
            { }

            if (!hasData)
            {
                hasData = context.Database.EnsureCreated(); // just create the schema - no migrations                
            }
            if (!hasData)
                throw new InvalidOperationException("No data found and no data created...");

            hasData = context.Users.Any();
            if (!hasData)
            {
                context.Users.AddRange(new[]
                {
                    new User()
                    {
                        Id=new Guid("11111111-0589-4951-ad11-dae7fb1566cb"),                        
                        Email="rstrahl@west-wind.com", Password = "wwind", UserDisplayName="RickStrahl",
                        FirstName = "Rick", LastName = "Strahl", Company="Westwind", IsAdmin = true
                    },
                    new User()
                    {
                        Id=new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                        Email="megger@eps-software.com", Password = "wwind", UserDisplayName="MarkusEgger",
                        FirstName = "Markus", LastName = "Egger", Company="EPS Software", IsAdmin = true
                    },
                });
                
                context.SaveChanges();
            }

            return true;
        }
    }
}
