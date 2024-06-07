﻿using Microsoft.EntityFrameworkCore;
using OptimazedCvStorage.Models;

namespace OptimazedCvStorage.Data
{
    public class CVContext : DbContext
    {
        public CVContext(DbContextOptions<CVContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PersonalInfo> PersonalInfo { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Education> Education { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<WorkExperience> WorkExperience { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.PersonalInfo)
                .WithOne(pi => pi.User)
                .HasForeignKey<PersonalInfo>(pi => pi.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Skills)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Educations)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Certifications)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.WorkExperiences)
                .WithOne(we => we.User)
                .HasForeignKey(we => we.UserID);
        }

    }
}