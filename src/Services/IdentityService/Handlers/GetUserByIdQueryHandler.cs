using MediatR;
using IdentityService.Queries;
using IdentityService.Data;
using IdentityService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto?>
{
    private readonly IdentityDbContext _context;

    public GetUserByIdQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponseDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
