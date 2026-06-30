using DataAccessLayer.Persistence;
using DataAccessLayer.Repositories;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Repositories;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessLayer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Patients = new GenericRepository<Patient>(_context);
            Doctors = new GenericRepository<Doctor>(_context);
            Appointments = new GenericRepository<Appointment>(_context);
            DoctorRatings = new GenericRepository<DoctorRating>(_context);
            Prescriptions = new GenericRepository<Prescription>(_context);
            Payments = new GenericRepository<Payment>(_context);
            MedicalRecords = new GenericRepository<MedicalRecord>(_context);
            Testings = new GenericRepository<Testing>(_context);
            RefreshTokens = new GenericRepository<RefreshToken>(_context);
            Conversations = new GenericRepository<Conversation>(_context);
            Messages = new GenericRepository<Message>(_context);
            MessageStatuses = new GenericRepository<MessageStatus>(_context);
            ChatAuditLogs = new GenericRepository<ChatAuditLog>(_context);
            Calls = new GenericRepository<Call>(_context);
            Medicines = new GenericRepository<Medicine>(_context);
            DoctorSchedules = new GenericRepository<DoctorSchedule>(_context);
        }

        public IGenericRepository<Patient> Patients { get; }
        public IGenericRepository<Doctor> Doctors { get; }
        public IGenericRepository<Prescription> Prescriptions { get; }
        public IGenericRepository<Payment> Payments { get; }
        public IGenericRepository<MedicalRecord> MedicalRecords { get; }

        public IGenericRepository<Testing> Testings { get; }

        public IGenericRepository<Appointment> Appointments { get; }
        public IGenericRepository<DoctorRating> DoctorRatings { get; }
        public IGenericRepository<RefreshToken> RefreshTokens { get; }
        public IGenericRepository<Conversation> Conversations { get; }
        public IGenericRepository<Message> Messages { get; }
        public IGenericRepository<MessageStatus> MessageStatuses { get; }
        public IGenericRepository<ChatAuditLog> ChatAuditLogs { get; }
        public IGenericRepository<Call> Calls { get; }
        public IGenericRepository<Medicine> Medicines { get; }
        public IGenericRepository<DoctorSchedule> DoctorSchedules { get; }

        public async Task<bool> SaveChanges()
        {
            int affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }

        public async Task CreateTransaction()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            await _transaction.CommitAsync();
        }

        public async Task Rollback()
        {
            await _transaction.RollbackAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
