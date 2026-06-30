using ClinicAPI.DTOs;
using ClinicAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;
    private readonly ILogger<MedicinesController> _logger;

    public MedicinesController(IMedicineService medicineService, ILogger<MedicinesController> logger)
    {
        _medicineService = medicineService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedMedicinesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedMedicinesResponseDto>> Get(
        [FromQuery] string? name = null,
        [FromQuery] string? arabic_name = null,
        [FromQuery] string? active_ingredient = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _medicineService.SearchAsync(name, arabic_name, active_ingredient, pageNumber, pageSize);
        return Ok(result);
    }


}
