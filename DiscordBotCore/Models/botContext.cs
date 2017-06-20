using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public partial class BotContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Filename=./corebot.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInput>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("STRING");

                entity.Property(e => e.Input).HasColumnType("STRING");
            });

            modelBuilder.Entity<UserInputRelationship>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("STRING");

                entity.Property(e => e.BotOutputId)
                    .IsRequired()
                    .HasColumnType("STRING");

                entity.Property(e => e.UserReplyId)
                    .IsRequired()
                    .HasColumnType("STRING");

                entity.HasOne(d => d.BotOutput)
                    .WithMany(p => p.UserInputRelationshipBotOutput)
                    .HasForeignKey(d => d.BotOutputId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.UserReply)
                    .WithMany(p => p.UserInputRelationshipUserReply)
                    .HasForeignKey(d => d.UserReplyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public virtual DbSet<UserInput> UserInput { get; set; }
        public virtual DbSet<UserInputRelationship> UserInputRelationship { get; set; }
    }
}
