using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebPaperAnalyzer.Models
{
    public sealed class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ResultCriterion> Criteria { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            try
            {
                Database.EnsureCreated();
            }
            catch (Exception)
            {
                // ignored
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminLogin = "admin";
            var adminPassword = "123456";

            Role adminRole = new Role { Id = 1, Name = "admin" };
            Role studentRole = new Role { Id = 2, Name = "student" };
            Role teacherRole = new Role {Id = 3, Name = "teacher" };
            User adminUser = new User { Id = 1, Login = adminLogin, Password = adminPassword, RoleId = adminRole.Id};
            ResultCriterion defaultCriterion = new ResultCriterion()
            {
                Id = 1, Name = "Default", ErrorCost = 2,
                KeyWordsCriterionFactor = 35, ZipfFactor = 30,
                WaterCriterionFactor = 35
            };

            modelBuilder.Entity<Role>().HasData(adminRole, studentRole, teacherRole);
            modelBuilder.Entity<User>().HasData(adminUser);
            modelBuilder.Entity<ResultCriterion>().HasData(defaultCriterion);
            base.OnModelCreating(modelBuilder);
        }
    }
}
