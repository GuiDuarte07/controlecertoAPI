﻿// <auto-generated />
using System;
using ControleCerto.Models.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ControleCerto.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ControleCerto.Models.Entities.Account", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<double>("Balance")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Bank")
                        .IsRequired()
                        .HasMaxLength(45)
                        .HasColumnType("character varying(45)");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Deleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("BillType")
                        .HasColumnType("integer");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CategoryDefault", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BillType")
                        .HasColumnType("integer");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasMaxLength(45)
                        .HasColumnType("character varying(45)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)");

                    b.HasKey("Id");

                    b.ToTable("CategoriesDefault");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditCard", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<int>("CloseDay")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("DueDay")
                        .HasColumnType("integer");

                    b.Property<bool>("SkipWeekend")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<double>("TotalLimit")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<double>("UsedLimit")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("CreditCards");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditPurchase", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("CreditCardId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Destination")
                        .HasMaxLength(80)
                        .HasColumnType("character varying(80)");

                    b.Property<int>("InstallmentsPaid")
                        .HasColumnType("integer");

                    b.Property<bool>("Paid")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("PurchaseDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<double>("TotalAmount")
                        .HasColumnType("decimal(10,2)");

                    b.Property<int>("TotalInstallment")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreditCardId");

                    b.ToTable("CreditPurchases");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Invoice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("ClosingDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("CreditCardId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("InvoiceDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("boolean");

                    b.Property<double>("TotalAmount")
                        .HasColumnType("decimal(10,2)");

                    b.Property<double>("TotalPaid")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreditCardId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.InvoicePayment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<double>("AmountPaid")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<long>("InvoiceId")
                        .HasColumnType("bigint");

                    b.Property<bool>("JustForRecord")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("InvoiceId");

                    b.ToTable("InvoicePayments");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Transaction", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<double>("Amount")
                        .HasColumnType("decimal(10,2)");

                    b.Property<long>("CategoryId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long?>("CreditPurchaseId")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Destination")
                        .HasMaxLength(80)
                        .HasColumnType("character varying(80)");

                    b.Property<int?>("InstallmentNumber")
                        .HasColumnType("integer");

                    b.Property<long?>("InvoiceId")
                        .HasColumnType("bigint");

                    b.Property<bool>("JustForRecord")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Observations")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<DateTime>("PurchaseDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CreditPurchaseId");

                    b.HasIndex("InvoiceId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Transference", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("AccountDestinyId")
                        .HasColumnType("bigint");

                    b.Property<long?>("AccountId")
                        .HasColumnType("bigint");

                    b.Property<long>("AccountOriginId")
                        .HasColumnType("bigint");

                    b.Property<double>("Amount")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PurchaseDate")
                        .IsRequired()
                        .HasMaxLength(45)
                        .HasColumnType("character varying(45)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AccountDestinyId");

                    b.HasIndex("AccountId");

                    b.HasIndex("AccountOriginId");

                    b.ToTable("Transferences");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("character varying(60)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Account", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.User", "User")
                        .WithMany("Accounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Category", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.User", "User")
                        .WithMany("Categories")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditCard", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.Account", "Account")
                        .WithOne("CreditCard")
                        .HasForeignKey("ControleCerto.Models.Entities.CreditCard", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditPurchase", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.CreditCard", "CreditCard")
                        .WithMany("CreditPurchases")
                        .HasForeignKey("CreditCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreditCard");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Invoice", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.CreditCard", "CreditCard")
                        .WithMany("Invoices")
                        .HasForeignKey("CreditCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreditCard");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.InvoicePayment", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.Account", "Account")
                        .WithMany("InvoicePayments")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControleCerto.Models.Entities.Invoice", "Invoice")
                        .WithMany("InvoicePayments")
                        .HasForeignKey("InvoiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Invoice");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Transaction", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.Account", "Account")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControleCerto.Models.Entities.Category", "Category")
                        .WithMany("Transactions")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ControleCerto.Models.Entities.CreditPurchase", "CreditPurchase")
                        .WithMany("Transactions")
                        .HasForeignKey("CreditPurchaseId");

                    b.HasOne("ControleCerto.Models.Entities.Invoice", "Invoice")
                        .WithMany("Transactions")
                        .HasForeignKey("InvoiceId");

                    b.Navigation("Account");

                    b.Navigation("Category");

                    b.Navigation("CreditPurchase");

                    b.Navigation("Invoice");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Transference", b =>
                {
                    b.HasOne("ControleCerto.Models.Entities.Account", "AccountDestiny")
                        .WithMany()
                        .HasForeignKey("AccountDestinyId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("ControleCerto.Models.Entities.Account", null)
                        .WithMany("Transferences")
                        .HasForeignKey("AccountId");

                    b.HasOne("ControleCerto.Models.Entities.Account", "AccountOrigin")
                        .WithMany()
                        .HasForeignKey("AccountOriginId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AccountDestiny");

                    b.Navigation("AccountOrigin");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Account", b =>
                {
                    b.Navigation("CreditCard");

                    b.Navigation("InvoicePayments");

                    b.Navigation("Transactions");

                    b.Navigation("Transferences");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Category", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditCard", b =>
                {
                    b.Navigation("CreditPurchases");

                    b.Navigation("Invoices");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.CreditPurchase", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.Invoice", b =>
                {
                    b.Navigation("InvoicePayments");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("ControleCerto.Models.Entities.User", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("Categories");
                });
#pragma warning restore 612, 618
        }
    }
}
