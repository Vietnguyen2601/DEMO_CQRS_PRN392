using FluentValidation;
using OrderService.Commands;
using OrderService.Dtos;

namespace OrderService.Validators;

/// <summary>
/// Validator for CreateOrderCommand
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Account ID must be a valid GUID");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .WithMessage("Delivery address is required")
            .Length(5, 500)
            .WithMessage("Delivery address must be between 5 and 500 characters");

        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item")
            .Must(items => items.Count > 0)
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.OrderItems)
            .SetValidator(new CreateOrderItemValidator());
    }
}

/// <summary>
/// Validator for order items in create command
/// </summary>
public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Product ID must be a valid GUID");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0")
            .LessThanOrEqualTo((double)decimal.MaxValue)
            .WithMessage("Unit price is too large");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Quantity cannot exceed 10000");
    }
}
