﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PnFData.Model;

#nullable disable

namespace PnFData.Migrations
{
    [DbContext(typeof(PnFDataContext))]
    [Migration("20220215171715_Updating_Index_Schema")]
    partial class Updating_Index_Schema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PnFData.Model.Eod", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Close")
                        .HasColumnType("float");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("Day")
                        .HasColumnType("datetime2");

                    b.Property<double>("High")
                        .HasColumnType("float");

                    b.Property<double>("Low")
                        .HasColumnType("float");

                    b.Property<double>("Open")
                        .HasColumnType("float");

                    b.Property<Guid>("ShareId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double>("Volume")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("Day");

                    b.HasIndex("ShareId");

                    b.ToTable("EodPrices");
                });

            modelBuilder.Entity("PnFData.Model.Index", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("ExchangeCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ExchangeSubCode")
                        .HasMaxLength(4)
                        .HasColumnType("nvarchar(4)");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.ToTable("Indices");
                });

            modelBuilder.Entity("PnFData.Model.IndexValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Contributors")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("Day")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("IndexId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("Value")
                        .HasColumnType("float");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("Day");

                    b.HasIndex("IndexId");

                    b.ToTable("IndexValues");
                });

            modelBuilder.Entity("PnFData.Model.PnFBox", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("BoxType")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("MonthIndicator")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("PnFColumnId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Size")
                        .HasColumnType("float");

                    b.Property<DateTime>("Ticked")
                        .HasColumnType("datetime2");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("Value")
                        .HasColumnType("float");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("PnFColumnId");

                    b.ToTable("PnFBoxes");
                });

            modelBuilder.Entity("PnFData.Model.PnFChart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double?>("BoxSize")
                        .HasColumnType("float");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Reversal")
                        .HasColumnType("int");

                    b.Property<int>("Source")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.ToTable("PnFCharts");
                });

            modelBuilder.Entity("PnFData.Model.PnFColumn", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime?>("EndAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PnFChartId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double>("Volume")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("PnFChartId");

                    b.ToTable("PnFColumns");
                });

            modelBuilder.Entity("PnFData.Model.Share", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("getdate()");

                    b.Property<bool>("EodError")
                        .HasColumnType("bit");

                    b.Property<string>("ExchangeCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ExchangeSubCode")
                        .HasMaxLength(4)
                        .HasColumnType("nvarchar(4)");

                    b.Property<DateTime?>("LastEodDate")
                        .HasColumnType("datetime2");

                    b.Property<double>("MarketCapMillions")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PricesCurrency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.Property<string>("Sector")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShareDataSource")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("ShareDataSourceId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("SharesInIssueMillions")
                        .HasColumnType("float");

                    b.Property<string>("SuperSector")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tidm")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTimeOffset?>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Version")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("Tidm")
                        .IsUnique();

                    b.HasIndex("ShareDataSource", "ShareDataSourceId")
                        .IsUnique();

                    b.ToTable("Shares");
                });

            modelBuilder.Entity("PnFData.Model.Eod", b =>
                {
                    b.HasOne("PnFData.Model.Share", "Share")
                        .WithMany("EodPrices")
                        .HasForeignKey("ShareId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Share");
                });

            modelBuilder.Entity("PnFData.Model.IndexValue", b =>
                {
                    b.HasOne("PnFData.Model.Index", "Index")
                        .WithMany("Dates")
                        .HasForeignKey("IndexId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Index");
                });

            modelBuilder.Entity("PnFData.Model.PnFBox", b =>
                {
                    b.HasOne("PnFData.Model.PnFColumn", "PnFColumn")
                        .WithMany("Boxes")
                        .HasForeignKey("PnFColumnId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PnFColumn");
                });

            modelBuilder.Entity("PnFData.Model.PnFColumn", b =>
                {
                    b.HasOne("PnFData.Model.PnFChart", "PnFChart")
                        .WithMany("Columns")
                        .HasForeignKey("PnFChartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PnFChart");
                });

            modelBuilder.Entity("PnFData.Model.Index", b =>
                {
                    b.Navigation("Dates");
                });

            modelBuilder.Entity("PnFData.Model.PnFChart", b =>
                {
                    b.Navigation("Columns");
                });

            modelBuilder.Entity("PnFData.Model.PnFColumn", b =>
                {
                    b.Navigation("Boxes");
                });

            modelBuilder.Entity("PnFData.Model.Share", b =>
                {
                    b.Navigation("EodPrices");
                });
#pragma warning restore 612, 618
        }
    }
}
