using FluentValidation;

namespace CleanApi.Application.Features.Products;

/// <summary>Validation rules shared by the product commands.</summary>
internal static class ProductRules
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public static IRuleBuilderOptions<T, string> ValidProductName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(NameMaxLength);

    public static IRuleBuilderOptions<T, string?> ValidProductDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(DescriptionMaxLength);

    public static IRuleBuilderOptions<T, decimal> ValidProductPrice<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
}
