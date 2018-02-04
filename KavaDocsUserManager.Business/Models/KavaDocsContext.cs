using Microsoft.EntityFrameworkCore;

namespace KavaDocsUserManager.Business.Models
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

        public DbSet<Repository> Repositories { get; set; }
        
        public DbSet<Organization> Organizations { get; set; }

        public DbSet<RepositoryUser> UserRepositories { get; set; }

        //public DbSet<RepositoryContributor> RepositoryContributors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<UserRepository>()
            //    .HasKey(b => new { b.UserId, b.RepositoryId });
            
            //modelBuilder.Entity<UserRepository>()
            //    .HasOne(b=> b.User)
            //    .WithMany(p => p.Repositories)
            //    .HasForeignKey(pc => pc.UserId);

            //modelBuilder.Entity<UserRepository>()
            //    .HasOne(b => b.Respository)
            //    .WithMany(b => b.Users)
            //    .HasForeignKey(b => b.RepositoryId);


        }

    }
}
