using EasterCMS.Endpoints;
using FluentValidation;

namespace EasterCMS.Validators;

public class CreateParticipantRequestValidator : AbstractValidator<ParticipantEndpoints.CreateParticipantRequest>
{
    public CreateParticipantRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.City).NotEmpty().MinimumLength(2).MaximumLength(80);
        RuleFor(x => x.Age).InclusiveBetween(0, 120);
    }
}
