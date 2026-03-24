using MediatR;
using IdentityService.Commands;
using IdentityService.Data;
using IdentityService.Dtos;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponseDto>
{
    private readonly IdentityDbContext _context;

    public LoginUserCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("Invalid credentials");

        var hashedPassword = HashPassword(request.Password);
        if (user.Password != hashedPassword)
            throw new InvalidOperationException("Invalid credentials");

        if (!user.IsActive)
            throw new InvalidOperationException("User is inactive");

        // Generate a simple token (in production, use JWT)
        var token = GenerateToken(user.Id);

        return new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token
        };
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private string GenerateToken(Guid userId)
    {
        // Simple token generation (replace with JWT in production)
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow.Ticks}"));
    }
}
