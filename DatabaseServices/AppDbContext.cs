//using DatabaseServices.Seeding.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Phones;
using Models.Logging;
//using Services.Seeding.Identity;
using Services.Seeding.Logging;
using Models.Devices;
using Models.GroupingStuff;

namespace DatabaseServices
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<ApplicationUser> ApplicationUser{ get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Grouping> Groupings { get; set; }

        public DbSet<Device> Devices { get; set; }


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