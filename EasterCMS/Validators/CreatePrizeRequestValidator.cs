using EasterCMS.Endpoints;
using FluentValidation;

namespace EasterCMS.Validators;

public class CreatePrizeRequestValidator : AbstractValidator<PrizeEndpoint.CreatePrizeRequest>
{
    public CreatePrizeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.Value).GreaterThan(0).LessThanOrEqualTo(100_000);
    }
}
