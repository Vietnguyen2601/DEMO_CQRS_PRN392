using MediatR;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Queries;

namespace OrderService.Handlers;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderResponseDto>>
{
    private readonly IOrderReadRepository _readRepository;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(IOrderReadRepository readRepository, ILogger<GetAllOrdersQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<List<OrderResponseDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all orders from MongoDB read store");
        return await _readRepository.GetAllAsync(cancellationToken);
    }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto>
{
    private readonly IOrderReadRepository _readRepository;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(IOrderReadRepository readRepository, ILogger<GetOrderByIdQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<OrderResponseDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving order {OrderId} from MongoDB read store", request.OrderId);
        
        var order = await _readRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        return order;
    }
}

public class GetOrdersByAccountQueryHandler : IRequestHandler<GetOrdersByAccountQuery, List<OrderResponseDto>>
{
    private readonly IOrderReadRepository _readRepository;
    private readonly ILogger<GetOrdersByAccountQueryHandler> _logger;

    public GetOrdersByAccountQueryHandler(IOrderReadRepository readRepository, ILogger<GetOrdersByAccountQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<List<OrderResponseDto>> Handle(GetOrdersByAccountQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving orders for account {AccountId} from MongoDB read store", request.AccountId);
        return await _readRepository.GetByAccountIdAsync(request.AccountId, cancellationToken);
    }
}

public class GetOrderItemByIdQueryHandler : IRequestHandler<GetOrderItemByIdQuery, OrderItemResponseDto>
{
    private readonly IOrderReadRepository _readRepository;
    private readonly ILogger<GetOrderItemByIdQueryHandler> _logger;

    public GetOrderItemByIdQueryHandler(IOrderReadRepository readRepository, ILogger<GetOrderItemByIdQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<OrderItemResponseDto> Handle(GetOrderItemByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving order item {OrderItemId} from MongoDB read store", request.OrderItemId);

        var item = await _readRepository.GetOrderItemByIdAsync(request.OrderItemId, cancellationToken);

        if (item == null)
            throw new InvalidOperationException($"Order item with ID {request.OrderItemId} not found");

        return item;
    }
}

public class GetOrderItemsByOrderIdQueryHandler : IRequestHandler<GetOrderItemsByOrderIdQuery, List<OrderItemResponseDto>>
{
    private readonly IOrderReadRepository _readRepository;
    private readonly ILogger<GetOrderItemsByOrderIdQueryHandler> _logger;

    public GetOrderItemsByOrderIdQueryHandler(IOrderReadRepository readRepository, ILogger<GetOrderItemsByOrderIdQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<List<OrderItemResponseDto>> Handle(GetOrderItemsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving items for order {OrderId} from MongoDB read store", request.OrderId);
        return await _readRepository.GetOrderItemsByOrderIdAsync(request.OrderId, cancellationToken);
    }
}
