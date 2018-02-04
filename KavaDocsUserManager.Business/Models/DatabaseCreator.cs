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
                        Email="rstrahl@west-wind.com", Password = "testing", UserDisplayName="RickStrahl",
                        FirstName = "Rick", LastName = "Strahl", Company="Westwind", IsAdmin = true
                    },
                    new User()
                    {
                        Id=new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                        Email="megger@eps-software.com", Password = "testing", UserDisplayName="MarkusEgger",
                        FirstName = "Markus", LastName = "Egger", Company="EPS Software", IsAdmin = true
                    },
                });
                
                context.SaveChanges();


                var repository = new Repository()
                {
                    Prefix = "docs",
                    Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/DocHound/DocHoundEngine/master/Docs/\" }",
                    Title="Kava Docs Documentation"                          
                    
                };
                context.Repositories.Add(repository);

                var map = new RepositoryUser()
                {
                    UserId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                    RepositoryId = repository.Id,
                    IsOwner = true
                };
                context.UserRepositories.Add(map);                
                context.SaveChanges();

                repository = new Repository()
                {
                    Prefix = "dn2me",
                    Settings = "{ \"RepositoryType\": \"VSTSGit\", \"VSTSInstance\": \"https://eps-software.visualstudio.com\", \"VSTSProjectName\": \"DN2Me\",\"VSTSDocsFolder\": \"Docs\", \"VSTSPAT\":  \"zi6opdawlmm7mvd3xxaycfxgboz2enbxrdmitjj5nawmbg3ldvtq\" }",
                    Title = "Kava Docs Documentation"
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                    RepositoryId = repository.Id,
                    IsOwner = true
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();

                repository = new Repository()
                {
                    Prefix = "epsdocs",
                    Settings = "{ \"RepositoryType\": \"VSTSGit\", \"VSTSGit\": \"https://eps-software.visualstudio.com\", \"VSTSProjectName\": \"EPSDocs\", \"VSTSPAT\":  \"ow5y2hwaz5igq5y662hmzx3ixckv475624a4wn57ohlcqd3tdyrq\" }",
                    Title = "EPS Docs"                    
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                    RepositoryId = repository.Id,
                    IsOwner = true
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();

                repository = new Repository()
                {
                    Prefix = "codeframework",
                    Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"VSTSDocsFolder\": \"Docs\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/MarkusEggerInc/CodeFrameworkDocs/\" }",
                    Title = "Code Framework Documentation"
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                    RepositoryId = repository.Id,
                    IsOwner = true
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();

                repository = new Repository()
                {
                    Prefix = "raspberrypi",
                    Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/raspberrypi/documentation\" }",
                    Title = "Rasberry Pi Documentation"

                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb"),
                    RepositoryId = repository.Id,
                    IsOwner = true
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();


            }

            return true;
        }
    }
}
