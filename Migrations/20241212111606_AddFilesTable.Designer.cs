﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MobSecLab.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobSecLab.Migrations
{
    [DbContext(typeof(MobSecLabContext))]
    [Migration("20241212111606_AddFilesTable")]
    partial class AddFilesTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("MobSecLab.Models.User", b =>
                {
                    b.Property<int>("UserNo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserNo"));

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

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
