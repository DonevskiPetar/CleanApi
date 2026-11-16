using FluentValidation;

namespace CleanApi.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidProductPrice();
    }
}
