using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KavaDocsUserManagerBusiness;

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

            return true;
        }
    }
}
