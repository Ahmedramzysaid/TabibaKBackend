using DomainLayer.Constants;
using DomainLayer.DTOs;
using DomainLayer.Helpers;
using DomainLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.SuperAdmin)]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status405MethodNotAllowed)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(string), StatusCodes.Status503ServiceUnavailable)]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost("add-role")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> AddRole([FromBody] RoleDto roleDto)
    {
        if (!string.IsNullOrEmpty(roleDto.Id))
            return BadRequest("Role ID should be empty");

        var response = await _roleService.CreateRole(roleDto);
        return response.ErrorType switch
        {
            ServiceErrorType.Success => CreatedAtAction(
                nameof(GetById),
                new { id = response.Data.Id },
                response.Data),
            ServiceErrorType.ValidationError => BadRequest(response.Message),
            ServiceErrorType.Conflict => Conflict(response.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, response.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.Message)
        };
    }

    [HttpPut("update-role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRole([FromBody] RoleDto roleDto)
    {
        var response = await _roleService.UpdateRole(roleDto);
        return response.ErrorType switch
        {
            ServiceErrorType.Success => NoContent(),
            ServiceErrorType.ValidationError => BadRequest(response.Message),
            ServiceErrorType.NotFound => NotFound(response.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, response.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.Message)
        };
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetById(string id)
    {
        var response = await _roleService.GetRole(id);
        return response.ErrorType switch
        {
            ServiceErrorType.Success => Ok(response.Data),
            ServiceErrorType.NotFound => NotFound(response.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, response.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.Message)
        };
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRole(string id)
    {
        var response = await _roleService.DeleteRole(id);
        return response.ErrorType switch
        {
            ServiceErrorType.Success => Ok(),
            ServiceErrorType.NotFound => NotFound(response.Message),
            ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, response.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.Message)
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<RoleDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _roleService.GetAllRoles(pageNumber, pageSize);
        if (response.ErrorType != ServiceErrorType.Success)
        {
            return response.ErrorType switch
            {
                ServiceErrorType.NotFound => NotFound(response.Message),
                ServiceErrorType.DatabaseError => StatusCode(StatusCodes.Status503ServiceUnavailable, response.Message),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response.Message)
            };
        }
        return Ok(response.Data);
    }
}
