using MediatR;
using IdentityService.Dtos;

namespace IdentityService.Queries;

public class GetAllUsersQuery : IRequest<List<UserResponseDto>>
{
}
