﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scaphoid.Infrastructure.Data;

#nullable disable

namespace Scaphoid.Migrations.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230407203935_Change")]
    partial class Change
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("Scaphoid.Core.Model.Localization", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DesignType")
                        .HasColumnType("INTEGER");

                    b.HasKey("OrderId");

                    b.ToTable("Localization");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Beam")
                        .HasColumnType("TEXT");

                    b.Property<string>("Company")
                        .HasColumnType("TEXT");

                    b.Property<string>("Designer")
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Project")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Localization", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Order", "Order")
                        .WithOne("Localization")
                        .HasForeignKey("Scaphoid.Core.Model.Localization", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Scaphoid.Core.Model.DeflectionLimit", "DeflectionLimit", b1 =>
                        {
                            b1.Property<int>("LocalizationOrderId")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("TotalLoads")
                                .HasColumnType("REAL");

                            b1.Property<double>("VariableLoads")
                                .HasColumnType("REAL");

                            b1.HasKey("LocalizationOrderId");

                            b1.ToTable("Localization");

                            b1.WithOwner()
                                .HasForeignKey("LocalizationOrderId");
                        });

                    b.OwnsOne("Scaphoid.Core.Model.DesignParameters", "DesignParameters", b1 =>
                        {
                            b1.Property<int>("LocalizationOrderId")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("GammaG")
                                .HasColumnType("REAL");

                            b1.Property<double>("GammaQ")
                                .HasColumnType("REAL");

                            b1.Property<double>("ModificationFactorAllOtherHtoB")
                                .HasColumnType("REAL");

                            b1.Property<double>("ModificationFactorKflHtoBLessThanTwo")
                                .HasColumnType("REAL");

                            b1.Property<double>("ReductionFactorF")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS235Between16and40mm")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS235Between40and63mm")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS235LessThan16mm")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS355Between16and40mm")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS355Between40and63mm")
                                .HasColumnType("REAL");

                            b1.Property<double>("SteelGradeS355LessThan16mm")
                                .HasColumnType("REAL");

                            b1.HasKey("LocalizationOrderId");

                            b1.ToTable("Localization");

                            b1.WithOwner()
                                .HasForeignKey("LocalizationOrderId");
                        });

                    b.Navigation("DeflectionLimit");

                    b.Navigation("DesignParameters");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Order", b =>
                {
                    b.Navigation("Localization");
                });
#pragma warning restore 612, 618
        }
    }
}
