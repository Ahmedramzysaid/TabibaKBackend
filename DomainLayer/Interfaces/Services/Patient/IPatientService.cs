using System.Linq.Expressions;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Models;

namespace DomainLayer.Interfaces.ServicesInterfaces
{
    public interface IPatientService
    {
        Task<Result<PaginatedResult<PatientDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
        Task<Result<PatientDto>> GetById(string id);
        Task<Result<PatientDto>> Add(PatientDto patient, string password);
        Task<Result<PatientDto>> Update(PatientDto patient);
        Task<Result<PatientDto>> Delete(string id);
        Task<Result<Patient>> Delete(Expression<Func<Patient, bool>> predicate);
    }
}
