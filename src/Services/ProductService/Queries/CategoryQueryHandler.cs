using MediatR;
using ProductService.Queries;
using ProductService.Data;
using ProductService.Dtos;

namespace ProductService.Handlers;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryResponseDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetAllCategoriesQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetAllCategoriesAsync(cancellationToken);
    }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponseDto>
{
    private readonly IProductReadRepository _readRepository;

    public GetCategoryByIdQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<CategoryResponseDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _readRepository.GetCategoryByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Category with ID {request.Id} not found");

        return category;
    }
}

public class GetActiveCategoriesQueryHandler : IRequestHandler<GetActiveCategoriesQuery, List<CategoryResponseDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetActiveCategoriesQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetActiveCategoriesAsync(cancellationToken);
    }
}
