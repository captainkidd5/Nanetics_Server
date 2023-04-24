using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Phones;
using Models.Devices;
using Models.GroupingStuff;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
namespace DatabaseServices
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    { 
        public DbSet<ApplicationUser> ApplicationUser{ get; set; }
        public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<Grouping> Groupings { get; set; }

       public DbSet<Device> Devices { get; set; }


        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
       //     builder.Entity<Grouping>()
       //.HasOne(g => g.User)
       //.WithMany(u => u.Groupings)
       //.OnDelete(DeleteBehavior.Cascade);

       //     builder.Entity<Device>()
       //         .HasOne(c => c.Grouping)
       //         .WithMany(g => g.Devices)
       //         .HasForeignKey(c => c.GroupingId)
       //         .OnDelete(DeleteBehavior.Cascade);

       //     builder.Entity<Device>()
       //         .HasOne(c => c.User)
       //         .WithMany(u => u.Devices)
       //         .HasForeignKey(c => c.UserId)
       //         .OnDelete(DeleteBehavior.NoAction);
   

        }
    }
}