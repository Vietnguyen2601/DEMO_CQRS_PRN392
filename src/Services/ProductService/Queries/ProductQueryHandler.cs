using MediatR;
using ProductService.Queries;
using ProductService.Data;
using ProductService.Dtos;

namespace ProductService.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductResponseDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetAllProductsQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetAllProductsAsync(cancellationToken);
    }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto>
{
    private readonly IProductReadRepository _readRepository;

    public GetProductByIdQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<ProductResponseDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _readRepository.GetProductByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.Id} not found");

        return product;
    }
}

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductResponseDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetProductsByCategoryQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetProductsByCategoryAsync(request.CategoryId, cancellationToken);
    }
}

public class GetProductsByShopQueryHandler : IRequestHandler<GetProductsByShopQuery, List<ProductResponseDto>>
{
    private readonly IProductReadRepository _readRepository;

    public GetProductsByShopQueryHandler(IProductReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<List<ProductResponseDto>> Handle(GetProductsByShopQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetProductsByShopAsync(request.ShopId, cancellationToken);
    }
}
