using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DiscordBot.Models;

namespace DiscordBotCore.Migrations
{
    [DbContext(typeof(BotContext))]
    partial class BotContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("DiscordBot.Models.UserInput", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("STRING");

                    b.Property<string>("Input")
                        .HasColumnType("STRING");

                    b.HasKey("Id");

                    b.ToTable("UserInput");
                });

            modelBuilder.Entity("DiscordBot.Models.UserInputRelationship", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("STRING");

                    b.Property<Guid>("BotOutputId")
                        .HasColumnType("STRING");

                    b.Property<int>("TimesReplied");

                    b.Property<Guid>("UserReplyId")
                        .HasColumnType("STRING");

                    b.HasKey("Id");

                    b.HasIndex("BotOutputId");

                    b.HasIndex("UserReplyId");

                    b.ToTable("UserInputRelationship");
                });

            modelBuilder.Entity("DiscordBot.Models.UserInputRelationship", b =>
                {
                    b.HasOne("DiscordBot.Models.UserInput", "BotOutput")
                        .WithMany("UserInputRelationshipBotOutput")
                        .HasForeignKey("BotOutputId");

                    b.HasOne("DiscordBot.Models.UserInput", "UserReply")
                        .WithMany("UserInputRelationshipUserReply")
                        .HasForeignKey("UserReplyId");
                });
        }
    }
}
