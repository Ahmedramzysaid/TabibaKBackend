using DomainLayer.Interfaces.Repositories;
using DomainLayer.Models;

namespace DomainLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Patient> Patients { get; }
        IGenericRepository<Doctor> Doctors { get; }
        IGenericRepository<Prescription> Prescriptions { get; }
        IGenericRepository<Payment> Payments { get; }

        IGenericRepository<MedicalRecord> MedicalRecords { get; }

        IGenericRepository<Testing> Testings { get; }

        IGenericRepository<Appointment> Appointments { get; }

        IGenericRepository<DoctorRating> DoctorRatings { get; }

        IGenericRepository<RefreshToken> RefreshTokens { get; }
        IGenericRepository<Conversation> Conversations { get; }
        IGenericRepository<Message> Messages { get; }
        IGenericRepository<MessageStatus> MessageStatuses { get; }
        IGenericRepository<ChatAuditLog> ChatAuditLogs { get; }
        IGenericRepository<Call> Calls { get; }
        IGenericRepository<Medicine> Medicines { get; }
        IGenericRepository<DoctorSchedule> DoctorSchedules { get; }

        Task<bool> SaveChanges();

        Task CreateTransaction();

        Task Commit();

        Task Rollback();
    }
}
