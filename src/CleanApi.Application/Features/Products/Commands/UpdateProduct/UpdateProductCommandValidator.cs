using FluentValidation;

namespace CleanApi.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).ValidProductName();
        RuleFor(x => x.Description).ValidProductDescription();
        RuleFor(x => x.Price).ValidProductPrice();
    }
}
