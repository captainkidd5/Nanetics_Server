//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Microsoft.EntityFrameworkCore;
//using Models.Authentication;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Identity;

//namespace DatabaseServices.Seeding.Identity
//{
//    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
//    {
//        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
//        {
//            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

//            // User1
//            ApplicationUser user = new ApplicationUser
//            {

//                Id = Guid.NewGuid(),
//                ConcurrencyStamp = "f3a6223f-ac2d-47d9-87d7-e297ea1b78e4",
//                Email = "waiikipomm@gmail.com",
//                NormalizedEmail = "WAIIKIPOMM@GMAIL.COM",
//                EmailConfirmed = true,
//                UserName = "waiikipomm@gmail.com",
//                NormalizedUserName = "WAIIKIPOMM@GMAIL.COM",
//                SecurityStamp = Guid.NewGuid().ToString(),
//                ConfirmationCode = "000000",
//            };
//            user.PasswordHash = passwordHasher.HashPassword(user, "Runescape1!");
//            builder.HasData(user);

//            // Assign the user to the "Admin" role
//            builder.HasData(new ApplicationRole
//            {
//                Name = "Admin",
//                Id = user.Id
//            });
//        }
//    }
//}
