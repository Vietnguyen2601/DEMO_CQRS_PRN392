using FluentValidation;
using OrderService.Commands;
using OrderService.Dtos;

namespace OrderService.Validators;

/// <summary>
/// Validator for AddOrderItemCommand
/// </summary>
public class AddOrderItemCommandValidator : AbstractValidator<AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Order ID must be a valid GUID");

        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("Must provide at least one item to add")
            .Must(items => items.Count > 0)
            .WithMessage("Must provide at least one item to add");

        RuleForEach(x => x.OrderItems)
            .SetValidator(new CreateOrderItemValidator());
    }
}
