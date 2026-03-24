using MediatR;
using IdentityService.Dtos;

namespace IdentityService.Commands;

public class RegisterUserCommand : IRequest<UserResponseDto>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
