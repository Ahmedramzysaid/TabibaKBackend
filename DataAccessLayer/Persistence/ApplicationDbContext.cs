using DomainLayer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NetTopologySuite.Geometries;

namespace DataAccessLayer.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }




        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<DigitalPrescription> DigitalPrescriptions { get; set; }
        public DbSet<DigitalPrescriptionItem> DigitalPrescriptionItems { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Testing> Testings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DoctorRating> DoctorRatings { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageStatus> MessageStatuses { get; set; }
        public DbSet<ChatAuditLog> ChatAuditLogs { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<DoctorAdviceVideo> DoctorAdviceVideos { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<DoctorAdvicePost> DoctorAdvicePosts { get; set; }
        public DbSet<AdviceLike> AdviceLikes { get; set; }
        public DbSet<AdviceComment> AdviceComments { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<DoctorEarning> DoctorEarnings { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<ApiAuditLog> ApiAuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Location)
                    .HasColumnType("geography");

            });
        }
    }
}
