//using DatabaseServices.Seeding.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.BusinessStuff;
using Models.Phones;
using Models.Logging;
//using Services.Seeding.Identity;
using Services.Seeding.Logging;

namespace DatabaseServices
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<ApplicationUser> ApplicationUser{ get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Business> Businesses { get; set; }



        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.ApplyConfiguration(new UserConfiguration());

            //builder.ApplyConfiguration(new RoleConfiguration());
            // builder.ApplyConfiguration(new LogConfiguration());

        }
    }
}