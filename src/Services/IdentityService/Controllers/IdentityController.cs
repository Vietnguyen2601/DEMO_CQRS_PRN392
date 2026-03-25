using MediatR;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Commands;
using IdentityService.Queries;
using IdentityService.Dtos;

namespace IdentityService.Controllers;

/// <summary>
/// Identity Service Controller - Handles user authentication and management
/// </summary>
[ApiController]
[Route("api/identity")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="dto">Registration details (username, email, password)</param>
    /// <returns>Created user details with status 201</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/identity/register
    ///     {
    ///        "username": "john",
    ///        "email": "john@example.com",
    ///        "password": "123456"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "username": "john",
    ///        "email": "john@example.com",
    ///        "role": "USER",
    ///        "isActive": true,
    ///        "createdAt": "2024-01-20T10:30:00Z"
    ///     }
    /// </remarks>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid input or duplicate user</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserDto dto)
    {
        try
        {
            var command = new RegisterUserCommand
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Login user and get authentication token
    /// </summary>
    /// <param name="dto">Login credentials (email, password)</param>
    /// <returns>User ID, username, and authentication token</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/identity/login
    ///     {
    ///        "email": "john@example.com",
    ///        "password": "123456"
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "username": "john",
    ///        "token": "M2ZhODVmNjQtNTcxNy00NTYyLWIzZmMtMmM5NjNmNjZhZmE2OjEzMjE3NjQwMDAwMDAwMDAw"
    ///     }
    /// </remarks>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginUserDto dto)
    {
        try
        {
            var command = new LoginUserCommand
            {
                Email = dto.Email,
                Password = dto.Password
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all users in the system
    /// </summary>
    /// <returns>List of all users</returns>
    /// <remarks>
    /// Sample response:
    /// 
    ///     [
    ///       {
    ///          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///          "username": "john",
    ///          "email": "john@example.com",
    ///          "role": "USER",
    ///          "isActive": true
    ///       }
    ///     ]
    /// </remarks>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
    {
        try
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID (GUID)</param>
    /// <returns>User details</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/identity/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// 
    /// Sample response:
    /// 
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "username": "john",
    ///        "email": "john@example.com",
    ///        "role": "USER",
    ///        "isActive": true
    ///     }
    /// </remarks>
    /// <response code="200">User found</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponseDto>> GetUserById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            if (result == null)
                return NotFound(new { message = "User not found" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
