/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */

using Microsoft.EntityFrameworkCore;
using CarRental.Api.Models;

namespace CarRental.Api.Data;

public class CarRentalDbContext : DbContext
{
    public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : base(options)
    {
    }

    public DbSet<RentalCompany> RentalCompanies { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<VehicleCategory> VehicleCategories { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CustomerPaymentMethod> CustomerPaymentMethods { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UUID generation
        modelBuilder.Entity<RentalCompany>()
            .Property(e => e.CompanyId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Customer>()
            .Property(e => e.CustomerId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<VehicleCategory>()
            .Property(e => e.CategoryId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Vehicle>()
            .Property(e => e.VehicleId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Reservation>()
            .Property(e => e.ReservationId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Rental>()
            .Property(e => e.RentalId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Payment>()
            .Property(e => e.PaymentId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<CustomerPaymentMethod>()
            .Property(e => e.PaymentMethodId)
            .HasDefaultValueSql("uuid_generate_v4()");

        modelBuilder.Entity<Review>()
            .Property(e => e.ReviewId)
            .HasDefaultValueSql("uuid_generate_v4()");

        // Configure relationships
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Company)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Category)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Company)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Reservation)
            .WithMany(res => res.Rentals)
            .HasForeignKey(r => r.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Rentals)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.Rentals)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rental>()
            .HasOne(r => r.Company)
            .WithMany(c => c.Rentals)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Company)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerPaymentMethod>()
            .HasOne(pm => pm.Customer)
            .WithMany(c => c.PaymentMethods)
            .HasForeignKey(pm => pm.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Rental)
            .WithMany(rental => rental.Reviews)
            .HasForeignKey(r => r.RentalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Company)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.Reviews)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.CompanyId);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.Status);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.CategoryId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.CustomerId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.VehicleId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.CompanyId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.PickupDate, r.ReturnDate });

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<Rental>()
            .HasIndex(r => r.CustomerId);

        modelBuilder.Entity<Rental>()
            .HasIndex(r => r.VehicleId);

        modelBuilder.Entity<Rental>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<Rental>()
            .HasIndex(r => new { r.ActualPickupDate, r.ExpectedReturnDate });

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.CustomerId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.CompanyId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.ReservationId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.Status);

        modelBuilder.Entity<CustomerPaymentMethod>()
            .HasIndex(pm => pm.CustomerId);

        // Configure array properties for PostgreSQL
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Features)
            .HasColumnType("text[]");

        // Configure decimal precision
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.DailyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.DailyRate)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Subtotal)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.TaxAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.InsuranceAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.AdditionalFees)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Reservation>()
            .Property(r => r.TotalAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Rental>()
            .Property(r => r.AdditionalCharges)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);
    }
}
