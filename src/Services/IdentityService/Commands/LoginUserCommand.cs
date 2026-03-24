using MediatR;
using IdentityService.Dtos;

namespace IdentityService.Commands;

public class LoginUserCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
