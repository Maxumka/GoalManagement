using System;
using Microsoft.EntityFrameworkCore;
using GoalManagement.Domain.Entities;

namespace GoalManagement.Repository.Context
{
    public class GoalManagementContext : DbContext
    {
        public DbSet<Goal> Goals { get; set; }

        public GoalManagementContext(DbContextOptions options) : base(options) 
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>().ToTable("Goal");
            modelBuilder.Entity<Goal>().HasKey(g => g.Id);
            modelBuilder.Entity<Goal>().Property(g => g.Name);
            modelBuilder.Entity<Goal>().Property(g => g.Description);
            modelBuilder.Entity<Goal>().Property(g => g.ArtistList);
            modelBuilder.Entity<Goal>().Property(g => g.RegistrationDate);
            modelBuilder.Entity<Goal>().Property(g => g.Status);
            modelBuilder.Entity<Goal>().Property(g => g.PlannedWorkLoad);
            modelBuilder.Entity<Goal>().Property(g => g.ActualExecutionTime);
            modelBuilder.Entity<Goal>().Property(g => g.EndDate);
            modelBuilder.Entity<Goal>().HasOne(g => g.Parent)
                                       .WithMany(g => g.SubGoals)
                                       .HasForeignKey(g => g.ParentId)
                                       .IsRequired(false)
                                       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
