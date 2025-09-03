using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace myProducts.Models;

public partial class MyproductsContext : DbContext
{
    public MyproductsContext()
    {
    }

    public MyproductsContext(DbContextOptions<MyproductsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<Quoteitem> Quoteitems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__CATEGORI__19093A0BFB76AD96");

            entity.ToTable("CATEGORIES");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__CLIENTS__E67E1A249B7B6599");

            entity.ToTable("CLIENTS");

            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Complement)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.District)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HouseNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Street)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__PRODUCTS__B40CC6CDC8DFE30F");

            entity.ToTable("PRODUCTS");

            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTS_CATEGORIES");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("PK__QUOTES__AF9688C14E14E4C4");

            entity.ToTable("QUOTES");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Client).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QUOTES_CLIENTS");

            entity.HasOne(d => d.User).WithMany(p => p.Quotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QUOTES_USERS");
        });

        modelBuilder.Entity<Quoteitem>(entity =>
        {
            entity.HasKey(e => e.QuoteItemId).HasName("PK__QUOTEITE__B0ED34FF552DCBFF");

            entity.ToTable("QUOTEITEMS");

            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.Quoteitems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QUOTEITEMS_PRODUCTS");

            entity.HasOne(d => d.Quote).WithMany(p => p.Quoteitems)
                .HasForeignKey(d => d.QuoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QUOTEITEMS_QUOTE");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USERS__1788CC4CCF295CC2");

            entity.ToTable("USERS");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
