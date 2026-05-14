using mmotors_back.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;


namespace mmotors_back.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Application> Applications { get; set; }
   
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //configure ambiguous application entity relationship
            builder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Application>()
                .HasOne(a => a.ReviewedByUser)
                .WithMany()
                .HasForeignKey(a => a.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            //Seed initial roles into the database
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id="role-admin",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "ROLE_ADMIN_STAMP"
                },
                //add new role here
                new IdentityRole
                {
                    Id="role-customer",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp = "ROLE_CUSTOMER_STAMP"
                },
                new IdentityRole
                {
                    Id="role-staff",
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    ConcurrencyStamp = "ROLE_STAFF_STAMP"
                }
            };
            //Seed roles into the IdentityRole entity
            builder.Entity<IdentityRole>().HasData(roles);
        }

    }
}