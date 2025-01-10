using FrameworkCompatibility.Models;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameworkCompatibility.Data
{
    public class StateMachineDbContext : DbContext
    {
        public DbSet<State> States { get; set; }
        public DbSet<Transition> Transitions { get; set; }
        public DbSet<FilterCondition> FilterConditions { get; set; }
        public DbSet<Audit> Audits { get; set; }

        public StateMachineDbContext(string connectionString) : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // State entity configuration
            modelBuilder.Entity<State>()
                .ToTable("States", "Registration")
                .Property(e => e.StateId)
                .HasColumnName("StateId")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<State>()
                .Property(e => e.Description).HasColumnName("Description");

            modelBuilder.Entity<State>()
                .Property(e => e.CreatedBy).HasColumnName("CreatedById");

            modelBuilder.Entity<State>()
                .Property(e => e.CreatedDate).HasColumnName("DateStamp");

            modelBuilder.Entity<State>()
               .Property(e => e.ModifiedBy).HasColumnName("ModifiedById");
            modelBuilder.Entity<State>()
               .Property(e => e.ModifiedDate).HasColumnName("ModifiedDateTime");

            // FilterCondition entity configuration
            modelBuilder.Entity<FilterCondition>()
                .ToTable("FilterConditions", "Administration");

            modelBuilder.Entity<FilterCondition>()
              .Property(e => e.Id).HasColumnName("Id");

            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.PropertyName).HasColumnName("PropertyName");

            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.Operator).HasColumnName("Operator");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.Value).HasColumnName("Value");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.ObjectTypeId).HasColumnName("ObjectTypeId");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.ObjectId).HasColumnName("ObjectId");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.IsActive).HasColumnName("IsActive");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.ParentId).HasColumnName("ParentId");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.LogicalOperator).HasColumnName("LogicalOperator");
            modelBuilder.Entity<FilterCondition>()
                .Property(e => e.TransitionId).HasColumnName("TransitionId");

            modelBuilder.Entity<FilterCondition>()
                .HasOptional(e => e.FilterCondition1)
                .WithMany(e => e.FilterConditions1)
                .HasForeignKey(e => e.ParentId) 
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<FilterCondition>()
                .HasRequired(e => e.Transition)
                .WithMany(e => e.FilterConditions)
                .HasForeignKey(e => e.TransitionId)
                .WillCascadeOnDelete(false);

            // Transition entity configuration
            modelBuilder.Entity<Transition>()
                .ToTable("RollBackStates", "Project");

            // Map properties to columns
            modelBuilder.Entity<Transition>()
                .Property(e => e.TransitionId)
                .HasColumnName("RollBackId");

            modelBuilder.Entity<Transition>()
                .Property(e => e.FromStateId)
                .HasColumnName("CurrentStateId");

            modelBuilder.Entity<Transition>()
                .Property(e => e.ToStateId)
                .HasColumnName("RollbackStateId");

            // Define primary key (optional, if not configured via data annotations)
            modelBuilder.Entity<Transition>()
                .HasKey(e => e.TransitionId);

            // Define relationships
            modelBuilder.Entity<Transition>()
                .HasRequired(e => e.FromState)
                .WithMany()
                .HasForeignKey(e => e.FromStateId)
                .WillCascadeOnDelete(false); // Equivalent to Restrict in EF Core

            modelBuilder.Entity<Transition>()
                .HasRequired(e => e.ToState)
                .WithMany()
                .HasForeignKey(e => e.ToStateId)
                .WillCascadeOnDelete(false); // Equivalent to Restrict in EF Core

            // Configure additional properties
            modelBuilder.Entity<Transition>()
                .Property(e => e.CreatedDate)
                .HasColumnName("DateStamp")
                .HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<Transition>()
                .Property(e => e.CreatedBy)
                .HasColumnName("CreatedById");

            modelBuilder.Entity<Transition>()
                .Property(e => e.ModifiedBy)
                .HasColumnName("ModifiedById");

            modelBuilder.Entity<Transition>()
                .Property(e => e.ModifiedDate)
                .HasColumnName("ModifiedDateTime");

            // Define one-to-many relationship with FilterConditions
            modelBuilder.Entity<Transition>()
                .HasMany(e => e.FilterConditions)
                .WithRequired(e => e.Transition)
                .HasForeignKey(e => e.TransitionId)
                .WillCascadeOnDelete(false);

            // Audit entity configuration
            modelBuilder.Entity<Audit>()
                .ToTable("AuditLogs", "Administration");

            base.OnModelCreating(modelBuilder);
        }
    }
}
