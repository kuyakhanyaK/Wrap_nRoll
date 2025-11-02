using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wrap_nRoll.Models;
using System.Collections.Generic;

namespace Wrap_nRoll.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Wrap> Wraps { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Wrap>()
                .Property(w => w.Price)
                .HasPrecision(10, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(10, 2);

            builder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(10, 2);

            // --- Many-to-Many Wrap <-> Ingredient ---
            builder.Entity<Wrap>()
                .HasMany(w => w.Ingredients)
                .WithMany(i => i.Wraps)
                .UsingEntity(j => j.ToTable("WrapIngredients"));
        }
    }
}
