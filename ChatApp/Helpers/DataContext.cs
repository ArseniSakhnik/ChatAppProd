using ChatApp.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Dialog> Dialogs { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UserDialog>().HasKey(k => new { k.Username, k.DialogId });

            modelBuilder.Entity<UserDialog>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDialog)
                .HasForeignKey(ud => ud.Username);

            modelBuilder.Entity<UserDialog>()
                .HasOne(ud => ud.Dialog)
                .WithMany(d => d.UserDialog)
                .HasForeignKey(ud => ud.DialogId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>().HasData(
                    new { Username = "test", Password = "test" },
                    new { Username = "test2", Password = "test2" }
                );

            modelBuilder.Entity<Dialog>().HasData(
                    new { Id = 1, Name = "test, test2"}
                );

            modelBuilder.Entity<UserDialog>().HasData(
                    new { Username = "test", DialogId = 1 },
                    new { Username = "test2", DialogId = 1 }
                );

            modelBuilder.Entity<Message>().HasData(
                    new { Id = 1, SenderUsername = "test", DialogId = 1, Text = "Hi"  },
                    new { Id = 2, SenderUsername = "test2", DialogId = 1, Text = "Hi, how are you?" },
                    new { Id = 3, SenderUsername = "test", DialogId = 1, Text = "I'm fine, what about you?" },
                    new { Id = 4, SenderUsername = "test2", DialogId = 1, Text = "I'm fine too. It's cool" }
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
