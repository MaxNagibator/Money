using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Data
{
    public partial class DataContext : DbContext
    {
        public DataContext()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<Car> Cars { get; set; }
        public virtual DbSet<CarEvent> CarEvents { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Debt> Debs { get; set; }
        public virtual DbSet<DebtUser> DebtUsers { get; set; }
        public virtual DbSet<DocumentTemplateGroup> DocumentTemplateGroups { get; set; }
        public virtual DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public virtual DbSet<FastOperation> FastOperations { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Place> Places { get; set; }
        public virtual DbSet<RegularTask> RegularTasks { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarEvent>()
                .Property(e => e.Mileage)
                .HasPrecision(18, 3);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Car)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.CarEvent)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Debt)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.DebtUser)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.FastOperation)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.RegularTask)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Log)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);
        }
    }
}
