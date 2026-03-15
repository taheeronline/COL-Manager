using COLManager.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace COLManager.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AuditTrail> Audit_Trail { get; set; }
        public DbSet<ColumnMaintenanceLog> Column_Maintenance_Log { get; set; }
        public DbSet<ColumnMaster> Column_Master { get; set; }
        public DbSet<ColumnUsageLog> Column_Usage_Log { get; set; }
        public DbSet<MeasurementType> Measurement_Type { get; set; }
        public DbSet<Protocol> Protocol { get; set; }
        public DbSet<StatusMaster> Status_Master { get; set; }
        public DbSet<UnitMaster> Unit_Master { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditTrail>().ToTable("Audit_Trail");
            modelBuilder.Entity<ColumnMaintenanceLog>().ToTable("Column_Maintenance_Log");
            modelBuilder.Entity<ColumnMaster>().ToTable("Column_Master");
            modelBuilder.Entity<ColumnUsageLog>().ToTable("Column_Usage_Log");
            modelBuilder.Entity<MeasurementType>().ToTable("Measurement_Type");
            modelBuilder.Entity<Protocol>().ToTable("Protocol");
            modelBuilder.Entity<StatusMaster>().ToTable("Status_Master");
            modelBuilder.Entity<UnitMaster>().ToTable("Unit_Master");

            // Configure keys and constraints where defaults won't pick them up
            modelBuilder.Entity<ColumnMaster>(entity =>
            {
                entity.HasKey(e => e.ColumnID);
                entity.HasIndex(e => e.SerialNumber).IsUnique();
                // ProtocolID is optional; remove the foreign key constraint or make it optional
                entity.HasOne<Protocol>()
                    .WithMany()
                    .HasForeignKey(e => e.ProtocolID)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MeasurementType>(entity =>
            {
                entity.HasIndex(e => e.MeasurementName).IsUnique();
            });

            modelBuilder.Entity<StatusMaster>(entity =>
            {
                entity.HasIndex(e => e.StatusName).IsUnique();
            });

            // ...additional configuration can be added as needed

            base.OnModelCreating(modelBuilder);
        }
    }
}
