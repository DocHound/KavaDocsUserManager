﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

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
                hasData = false;
                
                try
                {
                    if( context.Users.Any())
                        return true;

                    hasData =false;
                }
                catch (Exception ex)
                {
                    context.Database.Migrate(); // just create the schema - no migrations      

                    //if (!hasData)
                    //    throw new InvalidOperationException("Couldn't create the database...");                    
                    try
                    {
                        if (context.Users.Any())
                            return true;
                    }
                    catch
                    {
                        throw new InvalidOperationException("Couldn't create database.", ex);
                    }
                }
            }

            




            var rickId = new Guid("11111111-0589-4951-ad11-dae7fb1566cb");
            var markusId = new Guid("22222222-0589-4951-ad11-dae7fb1566cb");

            var roleUserId = new Guid("11111111-2222-4951-ad11-dae7fb1566ca");
            var roleAdminId =    new Guid("22222222-1111-4951-ad11-dae7fb1566cb");
            var rolePrivateId = new Guid("33333333-3333-4951-ad11-dae7fb1566cc");


            hasData = context.Users.Any();
            if (!hasData)
            {

            

                context.Users.AddRange(new[]
                {
                    new User()
                    {
                        Id=rickId,                        
                        Email="rstrahl@west-wind.com", Password = "testing", UserDisplayName="RickStrahl",
                        FirstName = "Rick", LastName = "Strahl", Company="Westwind", IsAdmin = true
                    },
                    new User()
                    {
                        Id=markusId,
                        Email="megger@eps-software.com", Password = "testing", UserDisplayName="MarkusEgger",
                        FirstName = "Markus", LastName = "Egger", Company="EPS Software", IsAdmin = true
                    },

                    new User()
                    {
                        Email="duesentriev@eps-software.com", Password = "testing", UserDisplayName="DanielDuesentrieb",
                        FirstName = "Daniel", LastName = "Duesentrieb", Company="EPS Software", IsAdmin = false
                    },
                    new User()
                    {
                        Email="dbubb@gorge.net", Password = "testing", UserDisplayName="DanBub",
                        FirstName = "Dan", LastName = "Bubb", Company="Gorge .NET", IsAdmin = false
                    },
                });                
                context.SaveChanges();


                var repository = new Repository()
                {
                    Id = new Guid("66666666-6666-6666-ad11-dae7fb1566cb"),
                    Prefix = "docs",
                    Settings =
                        "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/DocHound/DocHoundEngine/master/Docs/\" }",
                    Title = "Kava Docs Documentation"
                };
                context.Repositories.Add(repository);


                context.Roles.AddRange(new[]
                {
                    new Role
                    {
                        Id = roleUserId,
                        Name = "User",
                        Level = 1,
                        RepositoryId = repository.Id
                    },
                    new Role
                    {
                        Id = roleAdminId,
                        Name = "RepoAdmin",
                        Level = 10,
                        RepositoryId = repository.Id
                    },
                    new Role
                    {
                        Id = rolePrivateId,
                        Name = "Private",
                        Level = 3,
                        RepositoryId = repository.Id
                    }
                });

                var map = new RepositoryUser()
                {
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
                };
                context.UserRepositories.Add(map);

                map = new RepositoryUser()
                {
                    UserId = rickId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Contributor
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();

                context.UserRoles.Add(new RoleUserRepository
                {
                    UserId = rickId,
                    RoleId = roleAdminId,
                    RepositoryId = repository.Id
                });
                context.UserRoles.Add(new RoleUserRepository
                {
                    UserId = rickId,
                    RoleId = roleUserId,
                    RepositoryId = repository.Id
                });
                context.UserRoles.Add(new RoleUserRepository
                {
                    UserId = markusId,
                    RoleId = roleUserId,
                    RepositoryId = repository.Id
                });
                context.SaveChanges();

                repository = new Repository()
                {                    
                    Prefix = "dn2me",
                    Settings = "{ \"RepositoryType\": \"VSTSGit\", \"VSTSInstance\": \"https://eps-software.visualstudio.com\", \"VSTSProjectName\": \"DN2Me\",\"VSTSDocsFolder\": \"Docs\", \"VSTSPAT\":  \"zi6opdawlmm7mvd3xxaycfxgboz2enbxrdmitjj5nawmbg3ldvtq\" }",
                    Title = "Doctors near to Me"
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
                };                
                context.UserRepositories.Add(map);
                context.SaveChanges();

                context.UserRoles.Add(new RoleUserRepository
                {
                    UserId = rickId,
                    RoleId = roleUserId,
                    RepositoryId = repository.Id
                });
                context.UserRoles.Add(new RoleUserRepository
                {
                    UserId = markusId,
                    RoleId = roleAdminId,
                    RepositoryId = repository.Id
                });
                

                context.SaveChanges();


                repository = new Repository()
                {
                    Prefix = "eps",
                    Settings = "{ \"RepositoryType\": \"VSTSGit\", \"VSTSInstance\": \"https://eps-software.visualstudio.com\", \"VSTSProjectName\": \"EPSDocs\", \"VSTSPAT\":  \"ow5y2hwaz5igq5y662hmzx3ixckv475624a4wn57ohlcqd3tdyrq\" }",
                    Title = "EPS Doc"                    
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();

                repository = new Repository()
                {
                    Prefix = "codeframework",
                    Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/codeframework/docs/master/\" }",                    
                    Title = "Code Framework Documentation"
                };
                context.Repositories.Add(repository);

                map = new RepositoryUser()
                {
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
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
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
                };
                context.UserRepositories.Add(map);
                context.SaveChanges();


                repository = new Repository()
                {
                    Id = new Guid("55555555-5555-5555-5555-dae7fb1566cb"),
                    Prefix = "markdownmonster",
                    Settings = "{ \"RepositoryType\": \"GitHubRaw\", \"GitHubMasterUrl\":  \"https://raw.githubusercontent.com/markdownmonster/docs\" }",
                    Title = "Markdown Monster"

                };
                context.Repositories.Add(repository);

                context.Roles.AddRange(new[]
                {
                    new Role
                    {
                        Name = "User",
                        Level = 1,
                        RepositoryId = repository.Id
                    },
                    new Role
                    {
                        Name = "RepoAdmin",
                        Level = 10,
                        RepositoryId = repository.Id
                    },
                    new Role
                    {
                        Name = "PrivateUser",
                        Level = 3,
                        RepositoryId = repository.Id
                    }
                });

                map = new RepositoryUser()
                {
                    UserId = rickId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Owner
                };
                context.UserRepositories.Add(map);

                map = new RepositoryUser()
                {
                    UserId = markusId,
                    RepositoryId = repository.Id,
                    UserType = RepositoryUserTypes.Contributor
                };
                context.UserRepositories.Add(map);
                
                context.SaveChanges();

            }

            return true;
        }
    }
}
