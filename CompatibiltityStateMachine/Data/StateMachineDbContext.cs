using FundManagerStateMachine.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundManagerStateMachine.Data
{
    public class StateMachineDbContext : DbContext
    {
        public DbSet<State> States { get; set; }
        public DbSet<Transition> Transitions { get; set; }
        public DbSet<FilterCondition> FilterConditions { get; set; }
        public DbSet<Audit> Audits { get; set; }

        public StateMachineDbContext(DbContextOptions<StateMachineDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<State>(entity =>
            {
                // Specify the schema and table name
                entity.ToTable("States", schema: "Registration");

                // Map properties to columns
                entity.Property(e => e.StateId)
                    .HasColumnName("StateId");

                entity.Property(e => e.Description)
                    .HasColumnName("Description");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("CreatedById");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("DateStamp");

                entity.Property(e => e.ModifiedBy)
                .HasColumnName("ModifiedById");

                entity.Property(e => e.ModifiedDate)
                .HasColumnName("ModifiedDateTime");
            });

            modelBuilder.Entity<FilterCondition>(entity =>
            {
                entity.ToTable("FilterConditions", schema: "Administration");

                entity.HasKey(e => e.Id);

                // Configure columns for the existing table
                entity.Property(e => e.PropertyName).HasColumnName("PropertyName");
                entity.Property(e => e.Operator).HasColumnName("Operator");
                entity.Property(e => e.Value).HasColumnName("Value");

                // Handle new or missing columns
                entity.Property(e => e.ObjectTypeId).HasColumnName("ObjectTypeId");
                entity.Property(e => e.ObjectId).HasColumnName("ObjectId");
                entity.Property(e => e.IsActive).HasColumnName("IsActive");
                entity.Property(e => e.EvaluatedLinkedId).HasColumnName("EvaluatedLinkedId");
                entity.Property(e => e.ParentId).HasColumnName("ParentId");
                entity.Property(e => e.LogicalOperator).HasColumnName("LogicalOperator");
                entity.Property(e => e.TransitionId).HasColumnName("TransitionId");

                // Configure hierarchy
                entity.HasOne(e => e.FilterCondition1)
                      .WithMany(e => e.FilterConditions1)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Transition
                entity.HasOne(e => e.Transition)
                      .WithMany(e => e.FilterConditions)
                      .HasForeignKey(e => e.TransitionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transition>(entity =>
            {
                // Default table name for Transition
                entity.ToTable("RollBackStates", schema: "Project");

                // Map properties to columns
                entity.Property(e => e.TransitionId)
                    .HasColumnName("RollBackId");

                // Define primary key
                //entity.HasKey(e => e.TransitionId);

                entity.Property(e => e.FromStateId)
                    .HasColumnName("CurrentStateId");

                // Define relationships
                entity.HasOne(e => e.FromState)
                      .WithMany()
                      .HasForeignKey(e => e.FromStateId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.ToStateId)
                    .HasColumnName("RollbackStateId");

                entity.HasOne(e => e.ToState)
                      .WithMany()
                      .HasForeignKey(e => e.ToStateId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure additional properties
                //entity.Property(e => e.ConditionExpression).HasColumnName("ConditionExpression").IsRequired(false);
                entity.Property(e => e.CreatedDate)
                .HasColumnName("DateStamp")
                .HasDefaultValueSql("GETDATE()").IsRequired();

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("CreatedById");

                entity.Property(e => e.ModifiedBy)
                    .HasColumnName("ModifiedById");

                entity.Property(e => e.ModifiedDate)
                   .HasColumnName("ModifiedDateTime");

                entity.HasMany(e => e.FilterConditions)
               .WithOne(e => e.Transition)
               .HasForeignKey(e => e.TransitionId)
               .OnDelete(DeleteBehavior.Cascade);
            });


            //Audit
            modelBuilder.Entity<Audit>(entity =>
            {
                // Default table name for Transition
                entity.ToTable("AuditLogs", schema: "Administration");


            });
        }
    }
}
