using System.Text.Json;
using DynamicValidation.Domain.Entities;
using DynamicValidation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DynamicValidation.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework DbContext
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<DynamicModelEntity> DynamicModels { get; set; }
        public DbSet<ModelFieldEntity> ModelFields { get; set; }
        public DbSet<ValidationRuleEntity> ValidationRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // DynamicModel configuration
            modelBuilder.Entity<DynamicModelEntity>(entity =>
            {
                entity.ToTable("DynamicModels");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ModelField configuration
            modelBuilder.Entity<ModelFieldEntity>(entity =>
            {
                entity.ToTable("ModelFields");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.DefaultValue).HasMaxLength(500);
                
                entity.HasOne(e => e.Model)
                    .WithMany(m => m.Fields)
                    .HasForeignKey(e => e.ModelId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.NestedModel)
                    .WithMany()
                    .HasForeignKey(e => e.NestedModelId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // ValidationRule configuration
            modelBuilder.Entity<ValidationRuleEntity>(entity =>
            {
                entity.ToTable("ValidationRules");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RuleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Parameters).HasColumnType("nvarchar(max)");
                
                entity.HasOne(e => e.Model)
                    .WithMany(m => m.ValidationRules)
                    .HasForeignKey(e => e.ModelId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
                    
                entity.HasOne(e => e.Field)
                    .WithMany(f => f.ValidationRules)
                    .HasForeignKey(e => e.FieldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
            });
        }
    }
}