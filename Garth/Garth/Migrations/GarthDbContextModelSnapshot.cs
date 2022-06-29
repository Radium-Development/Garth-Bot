﻿// <auto-generated />
using System;
using Garth.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Garth.Migrations
{
    [DbContext(typeof(GarthDbContext))]
    partial class GarthDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Garth.DAL.DomainClasses.Blacklist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Blacklist");
                });

            modelBuilder.Entity("Garth.DAL.DomainClasses.Context", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("CreatorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Contexts");
                });

            modelBuilder.Entity("Garth.DAL.DomainClasses.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("LONGTEXT");

                    b.Property<DateTime?>("CreationDate")
                        .IsRequired()
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("CreatorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("CreatorName")
                        .IsRequired()
                        .HasMaxLength(37)
                        .HasColumnType("varchar(37)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("Global")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsFile")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<ulong>("Server")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
