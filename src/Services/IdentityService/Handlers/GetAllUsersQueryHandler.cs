using MediatR;
using IdentityService.Queries;
using IdentityService.Data;
using IdentityService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserResponseDto>>
{
    private readonly IdentityDbContext _context;

    public GetAllUsersQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponseDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Name = u.Name,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}
