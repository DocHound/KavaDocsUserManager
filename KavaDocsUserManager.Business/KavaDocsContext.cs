using System;
using KavaDocsUserManager.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace KavaDocsUserManagerBusiness
{
    public class KavaDocsContext : DbContext
    {

        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    var conn = Database.GetDbConnection();
                    _connectionString = conn?.ConnectionString;
                    conn = null;
                }
                return _connectionString;
            }
            set { _connectionString = value; }
        }
        private string _connectionString;


        public KavaDocsContext(DbContextOptions options) : base(options)
        {
        }

        public static KavaDocsContext GetWeblogContext(string connectionString)
        {
            var options = new DbContextOptionsBuilder<KavaDocsContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new KavaDocsContext(options);
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Repository> Repository { get; set; }

        public DbSet<Repository> Organizations { get; set; }

        public DbSet<Contributors> Contributors { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<User>()
            //    .HasIndex(b => b.Created);
        }

    }
}
