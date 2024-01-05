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
    [Migration("20240101165141_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.4");

            modelBuilder.Entity("Scaphoid.Core.Model.AxialForceLoad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadingId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LoadingId");

                    b.ToTable("AxialForceLoad");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Beam", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BottomFlangeSteel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BottomFlangeThickness")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BottomFlangeWidth")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FixedBottomFlange")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FixedTopFlange")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsUniformDepth")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Span")
                        .HasColumnType("REAL");

                    b.Property<int>("TopFlangeSteel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TopFlangeThickness")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TopFlangeWidth")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WebDepth")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WebDepthRight")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("WebLocalBuckle")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WebSteel")
                        .HasColumnType("INTEGER");

                    b.Property<double>("WebThickness")
                        .HasColumnType("REAL");

                    b.HasKey("OrderId");

                    b.ToTable("Beam");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.DistributeLoad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadingId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LoadingId");

                    b.ToTable("DistributeLoad");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.EndMomentLoad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LeftValue")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadingId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RightValue")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LoadingId");

                    b.ToTable("EndMomentLoad");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Loading", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CombinationType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadType")
                        .HasColumnType("INTEGER");

                    b.HasKey("OrderId");

                    b.ToTable("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Localization", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DesignType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PsiValue")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SteelType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ULSLoadExpression")
                        .HasColumnType("INTEGER");

                    b.HasKey("OrderId");

                    b.ToTable("Localization");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Designer")
                        .HasColumnType("TEXT");

                    b.Property<int>("ElementType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Note")
                        .HasColumnType("TEXT");

                    b.Property<string>("Project")
                        .HasColumnType("TEXT");

                    b.Property<string>("SectionId")
                        .HasColumnType("TEXT");

                    b.Property<double>("Span")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.PointLoad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Load")
                        .HasColumnType("REAL");

                    b.Property<int>("LoadingId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("PermanentAction")
                        .HasColumnType("REAL");

                    b.Property<double>("Position")
                        .HasColumnType("REAL");

                    b.Property<double>("VariableAction")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("LoadingId");

                    b.ToTable("PointLoad");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Restraint", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("BottomFlangeRestraints")
                        .HasColumnType("TEXT");

                    b.Property<bool>("FullRestraintBottomFlange")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FullRestraintTopFlange")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TopFlangeRestraints")
                        .HasColumnType("TEXT");

                    b.HasKey("OrderId");

                    b.ToTable("Restraint");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.XPointLoad", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoadingId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Position")
                        .HasColumnType("REAL");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LoadingId");

                    b.ToTable("XPointLoad");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.AxialForceLoad", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Loading", "Loading")
                        .WithMany("AxialForceLoads")
                        .HasForeignKey("LoadingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Beam", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Order", "Order")
                        .WithOne("BeamInfo")
                        .HasForeignKey("Scaphoid.Core.Model.Beam", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.DistributeLoad", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Loading", "Loading")
                        .WithMany("DistributeLoads")
                        .HasForeignKey("LoadingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.EndMomentLoad", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Loading", "Loading")
                        .WithMany("EndMomentLoads")
                        .HasForeignKey("LoadingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Loading", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Order", "Order")
                        .WithOne("Loading")
                        .HasForeignKey("Scaphoid.Core.Model.Loading", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Scaphoid.Core.Model.LoadParameters", "PermanentLoads", b1 =>
                        {
                            b1.Property<int>("LoadingOrderId")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("AxialForce")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentLeft")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentRight")
                                .HasColumnType("REAL");

                            b1.Property<int>("PartialUdl")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlEnd")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlStart")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("Udl")
                                .HasColumnType("REAL");

                            b1.HasKey("LoadingOrderId");

                            b1.ToTable("Loading");

                            b1.WithOwner()
                                .HasForeignKey("LoadingOrderId");
                        });

                    b.OwnsOne("Scaphoid.Core.Model.LoadParameters", "UltimateLoads", b1 =>
                        {
                            b1.Property<int>("LoadingOrderId")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("AxialForce")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentLeft")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentRight")
                                .HasColumnType("REAL");

                            b1.Property<int>("PartialUdl")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlEnd")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlStart")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("Udl")
                                .HasColumnType("REAL");

                            b1.HasKey("LoadingOrderId");

                            b1.ToTable("Loading");

                            b1.WithOwner()
                                .HasForeignKey("LoadingOrderId");
                        });

                    b.OwnsOne("Scaphoid.Core.Model.LoadParameters", "VariableLoads", b1 =>
                        {
                            b1.Property<int>("LoadingOrderId")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("AxialForce")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentLeft")
                                .HasColumnType("REAL");

                            b1.Property<double>("EndMomentRight")
                                .HasColumnType("REAL");

                            b1.Property<int>("PartialUdl")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlEnd")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("PartialUdlStart")
                                .HasColumnType("INTEGER");

                            b1.Property<double>("Udl")
                                .HasColumnType("REAL");

                            b1.HasKey("LoadingOrderId");

                            b1.ToTable("Loading");

                            b1.WithOwner()
                                .HasForeignKey("LoadingOrderId");
                        });

                    b.Navigation("Order");

                    b.Navigation("PermanentLoads");

                    b.Navigation("UltimateLoads");

                    b.Navigation("VariableLoads");
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

            modelBuilder.Entity("Scaphoid.Core.Model.PointLoad", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Loading", "Loading")
                        .WithMany("PointLoads")
                        .HasForeignKey("LoadingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Restraint", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Order", "Order")
                        .WithOne("Restraint")
                        .HasForeignKey("Scaphoid.Core.Model.Restraint", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.XPointLoad", b =>
                {
                    b.HasOne("Scaphoid.Core.Model.Loading", "Loading")
                        .WithMany("XPointLoads")
                        .HasForeignKey("LoadingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Loading");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Loading", b =>
                {
                    b.Navigation("AxialForceLoads");

                    b.Navigation("DistributeLoads");

                    b.Navigation("EndMomentLoads");

                    b.Navigation("PointLoads");

                    b.Navigation("XPointLoads");
                });

            modelBuilder.Entity("Scaphoid.Core.Model.Order", b =>
                {
                    b.Navigation("BeamInfo");

                    b.Navigation("Loading");

                    b.Navigation("Localization");

                    b.Navigation("Restraint");
                });
#pragma warning restore 612, 618
        }
    }
}