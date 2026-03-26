using FluentValidation;
using OrderService.Commands;

namespace OrderService.Validators;

/// <summary>
/// Validator for UpdateOrderCommand
/// </summary>
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Order ID must be a valid GUID");

        RuleFor(x => x.DeliveryAddress)
            .Length(5, 500)
            .WithMessage("Delivery address must be between 5 and 500 characters")
            .When(x => !string.IsNullOrEmpty(x.DeliveryAddress));
    }
}
