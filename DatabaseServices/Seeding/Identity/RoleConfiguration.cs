//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Models.Authentication;

//namespace Services.Seeding.Identity
//{
//    public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
//    {
//        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
//        {
//            builder.HasData(
//                new ApplicationRole
//                {
//                    Id = new Guid("cde3eeaf-aa76-4388-b308-3f06d55f15ae"),
//                    Name = "User",
//                    NormalizedName = "USER"
//                },
//                new ApplicationRole
//                {
//                    Id = new Guid("c75eb049-416a-4611-a3b9-8e48608978a5"),
//                    Name = "Admin",
//                    NormalizedName = "ADMIN"
//                }
//            );
//        }
//    }
//}
