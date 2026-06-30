using DomainLayer.DTOs;
using DomainLayer.Helpers;

namespace DomainLayer.Interfaces.Services;

public interface IPaymentService
{
    Task<Result<PaginatedResult<PaymentDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
    
    Task<Result<PaymentDto>> GetById(int id);
    
    Task<Result<bool>> Update(PaymentDto paymentDto);
    
}
