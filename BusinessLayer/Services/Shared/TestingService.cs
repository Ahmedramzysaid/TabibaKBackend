using AutoMapper;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using DomainLayer.Interfaces.Services;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class TestingService : ITestingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly DataAccessLayer.Persistence.ApplicationDbContext _context;

    public TestingService(IUnitOfWork unitOfWork, IMapper mapper, DataAccessLayer.Persistence.ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PaginatedResult<TestingDto>>> GetAll(string? doctorId = null, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Testings.Include(t => t.MedicalRecord).AsNoTracking();

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientIdsWithPendingAppointment = await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.AppointmentStatus == (short)DomainLayer.Enums.AppointmentStatus.Pending)
                .Select(a => a.PatientID)
                .Distinct()
                .ToListAsync();
            query = query.Where(t => t.MedicalRecord != null && patientIdsWithPendingAppointment.Contains(t.MedicalRecord.PatientId));
        }

        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return Result<PaginatedResult<TestingDto>>.Failure("No testings found", ServiceErrorType.NotFound);

        var testings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var testingDtos = testings.Select(t => new TestingDto
        {
            TestingId = t.TestingId,
            Title = t.Title,
            DateExam = t.DateExam.ToString("MM-dd-yyyy"),
            FilePath = t.FilePath,
            FileType = t.FileType,
            MedicalRecordId = t.MedicalRecordId
        });

        var paged = PaginatedResult<TestingDto>.Create(testingDtos, totalCount, pageNumber, pageSize);
        return Result<PaginatedResult<TestingDto>>.Success(paged);
    }

    public async Task<Result<TestingDto>> GetById(Guid id, string? doctorId = null)
    {
        var testing = await _context.Testings
            .Include(t => t.MedicalRecord)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TestingId == id);

        if (testing == null)
            return Result<TestingDto>.Failure("Testing not found", ServiceErrorType.NotFound);

        if (!string.IsNullOrEmpty(doctorId) && testing.MedicalRecord != null)
        {
            var patientHasPendingAppointmentWithDoctor = await _context.Appointments
                .AnyAsync(a => a.DoctorID == doctorId && a.PatientID == testing.MedicalRecord.PatientId && a.AppointmentStatus == (short)DomainLayer.Enums.AppointmentStatus.Pending);
            if (!patientHasPendingAppointmentWithDoctor)
                return Result<TestingDto>.Failure("Testing not found", ServiceErrorType.NotFound);
        }

        var testingDto = new TestingDto
        {
            TestingId = testing.TestingId,
            Title = testing.Title,
            DateExam = testing.DateExam.ToString("MM-dd-yyyy"),
            FilePath = testing.FilePath,
            FileType = testing.FileType,
            MedicalRecordId = testing.MedicalRecordId
        };

        return Result<TestingDto>.Success(testingDto);
    }

    public async Task<Result<IEnumerable<TestingDto>>> GetByMedicalRecordId(Guid medicalRecordId)
    {
        var testings = await _context.Testings
            .Include(t => t.MedicalRecord)
            .AsNoTracking()
            .Where(t => t.MedicalRecordId == medicalRecordId)
            .ToListAsync();

        if (testings == null || !testings.Any())
            return Result<IEnumerable<TestingDto>>.Failure("No testings found for this medical record", ServiceErrorType.NotFound);

        var testingDtos = testings.Select(t => new TestingDto
        {
            TestingId = t.TestingId,
            Title = t.Title,
            DateExam = t.DateExam.ToString("MM-dd-yyyy"),
            FilePath = t.FilePath,
            FileType = t.FileType,
            MedicalRecordId = t.MedicalRecordId
        });

        return Result<IEnumerable<TestingDto>>.Success(testingDtos);
    }

    public async Task<Result<IEnumerable<TestingDto>>> GetByPatientId(string patientId, string? doctorId = null)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return Result<IEnumerable<TestingDto>>.Failure("Patient ID is required", ServiceErrorType.ValidationError);

        var query = _context.Testings
            .Include(t => t.MedicalRecord)
            .AsNoTracking()
            .Where(t => t.MedicalRecord != null && t.MedicalRecord.PatientId == patientId);

        if (!string.IsNullOrEmpty(doctorId))
        {
            var patientIdsWithPendingAppointment = await _context.Appointments
                .Where(a => a.DoctorID == doctorId && a.AppointmentStatus == (short)DomainLayer.Enums.AppointmentStatus.Pending)
                .Select(a => a.PatientID)
                .Distinct()
                .ToListAsync();
            query = query.Where(t => t.MedicalRecord != null && patientIdsWithPendingAppointment.Contains(t.MedicalRecord.PatientId));
        }

        var testings = await query.OrderByDescending(t => t.DateExam).ToListAsync();
        var testingDtos = testings.Select(t => new TestingDto
        {
            TestingId = t.TestingId,
            Title = t.Title,
            DateExam = t.DateExam.ToString("MM-dd-yyyy"),
            FilePath = t.FilePath,
            FileType = t.FileType,
            MedicalRecordId = t.MedicalRecordId
        });

        return Result<IEnumerable<TestingDto>>.Success(testingDtos);
    }

    public async Task<Result<TestingDto>> Add(CreateTestingDto createTestingDto)
    {
        return await Add(createTestingDto, null, null);
    }

    public async Task<Result<TestingDto>> Add(CreateTestingDto createTestingDto, string? filePath, string? fileType)
    {
        var medicalRecordExists = await _context.MedicalRecords
            .AnyAsync(m => m.MedicalRecordId == createTestingDto.MedicalRecordId);
        
        if (!medicalRecordExists)
            return Result<TestingDto>.Failure("Medical record not found", ServiceErrorType.NotFound);

        if (!DateTime.TryParseExact(createTestingDto.DateExam, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var dateExam))
            return Result<TestingDto>.Failure("Invalid date format. Date must be in MM-dd-yyyy format", ServiceErrorType.ValidationError);

        var testing = new Testing
        {
            TestingId = Guid.NewGuid(),
            Title = createTestingDto.Title,
            DateExam = dateExam.Date, // Store only date part
            FilePath = filePath,
            FileType = fileType,
            MedicalRecordId = createTestingDto.MedicalRecordId
        };
        
        await _unitOfWork.Testings.Add(testing);
        bool saveResult = await _unitOfWork.SaveChanges();

        if (!saveResult)
            return Result<TestingDto>.Failure("Failed to add testing to database", ServiceErrorType.DatabaseError);

        var testingDto = new TestingDto
        {
            TestingId = testing.TestingId,
            Title = testing.Title,
            DateExam = testing.DateExam.ToString("MM-dd-yyyy"),
            FilePath = testing.FilePath,
            FileType = testing.FileType,
            MedicalRecordId = testing.MedicalRecordId
        };

        return Result<TestingDto>.Success(testingDto);
    }

    public async Task<Result<TestingDto>> Update(TestingDto testingDto)
    {
        return await Update(testingDto, null, null);
    }

    public async Task<Result<TestingDto>> Update(TestingDto testingDto, string? filePath, string? fileType)
    {
        var existingTesting = await _unitOfWork.Testings.GetById(testingDto.TestingId);
        
        if (existingTesting == null)
            return Result<TestingDto>.Failure("Testing not found", ServiceErrorType.NotFound);

        if (!DateTime.TryParseExact(testingDto.DateExam, "MM-dd-yyyy", null, System.Globalization.DateTimeStyles.None, out var dateExam))
            return Result<TestingDto>.Failure("Invalid date format. Date must be in MM-dd-yyyy format", ServiceErrorType.ValidationError);

        existingTesting.Title = testingDto.Title;
        existingTesting.DateExam = dateExam.Date; // Store only date part

        if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(fileType))
        {
            existingTesting.FilePath = filePath;
            existingTesting.FileType = fileType;
        }

        _unitOfWork.Testings.Update(existingTesting);
        bool saveResult = await _unitOfWork.SaveChanges();

        if (!saveResult)
            return Result<TestingDto>.Failure("Failed to update testing in database", ServiceErrorType.DatabaseError);

        var updatedDto = new TestingDto
        {
            TestingId = existingTesting.TestingId,
            Title = existingTesting.Title,
            DateExam = existingTesting.DateExam.ToString("MM-dd-yyyy"),
            FilePath = existingTesting.FilePath,
            FileType = existingTesting.FileType,
            MedicalRecordId = existingTesting.MedicalRecordId
        };

        return Result<TestingDto>.Success(updatedDto);
    }

    public async Task<Result<bool>> Delete(Guid id)
    {
        var testing = await _unitOfWork.Testings.GetById(id);
        
        if (testing == null)
            return Result<bool>.Failure("Testing not found", ServiceErrorType.NotFound);

        _unitOfWork.Testings.Delete(testing);
        bool saveResult = await _unitOfWork.SaveChanges();
        
        if (!saveResult)
            return Result<bool>.Failure("Failed to delete testing from database", ServiceErrorType.DatabaseError);
        
        return Result<bool>.Success(true);
    }
}
