using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KavaDocsUserManager.Business.Configuration;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace KavaDocsUserManager.Business.Tests
{
    public class TestHelper
    {
        public static string ConnectionString { get; set; } = "server=.;database=KavaDocs;integrated security=true;";

        //public static string ConnectionString { get; set; } = "Server=tcp:kavadocs.database.windows.net,1433;Initial Catalog=kavadocs;Persist Security Info=False;User ID=kavadocs;Password=McbhhUA0FMddt9Qmz77Ogb2A4eiL8F;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public static KavaDocsConfiguration Configuration { get; set; } = KavaDocsConfiguration.Current;
        
        public static Guid UserId1 { get; set; } = new Guid("11111111-0589-4951-ad11-dae7fb1566cb");
        public static Guid UserId2 { get; set; } = new Guid("22222222-0589-4951-ad11-dae7fb1566cb");

        public static Guid RepoId { get; set; } = new Guid("66666666-6666-6666-AD11-DAE7FB1566CB");

        public static KavaDocsContext GetContext()
        {
            var options = new DbContextOptionsBuilder<KavaDocsContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            var ctx = new KavaDocsContext(options);
            DatabaseCreator.EnsureKavaDocsData(ctx);
            return ctx;
        }
        

        public static UserBusiness GetUserBusiness()
        {
            return  new UserBusiness(GetContext(), KavaDocsConfiguration.Current);
        }

        public static RepositoryBusiness GetRepositoryBusiness()
        {
            return new RepositoryBusiness(GetContext(), KavaDocsConfiguration.Current);
        }

        public static OrganizationBusiness GetOrganizationBusiness()
        {
            return new OrganizationBusiness(GetContext(), KavaDocsConfiguration.Current);
        }


        public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {            
            var iConfig = new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("e3ddcccf-0cb3-423a-b302-e3e92e95c128")
                .AddEnvironmentVariables()
                .Build();

            return iConfig;
        }

        public static KavaDocsConfiguration GetApplicationConfiguration(string outputPath)
        {
            var configuration = new KavaDocsConfiguration();

            var iConfig = GetIConfigurationRoot(outputPath);

            iConfig
                .GetSection("KavaDocs")
                .Bind(configuration);

            return configuration;
        }
        

    }
}
