﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MobSecLab.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobSecLab.Migrations
{
    [DbContext(typeof(MobSecLabContext))]
    partial class MobSecLabContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MobSecLab.Models.FileEntity", b =>
                {
                    b.Property<int>("FileNum")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FileNum"));

                    b.Property<int>("FileSeq")
                        .HasColumnType("integer");

                    b.Property<string>("File_Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("File_md5")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserNo")
                        .HasColumnType("integer");

                    b.HasKey("FileNum");

                    b.HasIndex("UserNo");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("MobSecLab.Models.Results", b =>
                {
                    b.Property<int>("ResultsNo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ResultsNo"));

                    b.Property<int>("FileSeq")
                        .HasColumnType("integer");

                    b.Property<string>("File_Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SecurityScore")
                        .HasColumnType("integer");

                    b.Property<int>("SeverityHigh")
                        .HasColumnType("integer");

                    b.Property<int>("StatusDangerous")
                        .HasColumnType("integer");

                    b.Property<int>("TotalMalwarePermission")
                        .HasColumnType("integer");

                    b.Property<int>("TotalPermission")
                        .HasColumnType("integer");

                    b.Property<int>("UserNo")
                        .HasColumnType("integer");

                    b.Property<string>("md5")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("minSdk")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ResultsNo");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("MobSecLab.Models.User", b =>
                {
                    b.Property<int>("UserNo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserNo"));

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserNo");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MobSecLab.Models.FileEntity", b =>
                {
                    b.HasOne("MobSecLab.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserNo")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
