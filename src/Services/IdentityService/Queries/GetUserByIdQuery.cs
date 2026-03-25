using MediatR;
using IdentityService.Dtos;

namespace IdentityService.Queries;

public class GetUserByIdQuery : IRequest<UserResponseDto?>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}
