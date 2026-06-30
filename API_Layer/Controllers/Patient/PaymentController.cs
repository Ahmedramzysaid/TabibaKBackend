using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPayments)]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<PaymentDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var payments = await _paymentService.GetAll(pageNumber, pageSize);
        if (payments.ErrorType != ServiceErrorType.Success)
        {
            return payments.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(payments.Message),
                ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, payments.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };
        }
        return Ok(payments.Data);
    }

    [Authorize(Policy = AuthorizationPolicies.CanViewPayments)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _paymentService.GetById(id);
        return payment.ErrorType switch
        {
            ServiceErrorType.Success => Ok(payment.Data),
            ServiceErrorType.NotFound => NotFound(payment.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, payment.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [Authorize(Policy = AuthorizationPolicies.CanProcessPayment)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> Update(PaymentDto paymentDto)
    {
        var result = await _paymentService.Update(paymentDto);
        return result.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.ValidationError => BadRequest(result.Message),
            ServiceErrorType.NotFound => NotFound(result.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
