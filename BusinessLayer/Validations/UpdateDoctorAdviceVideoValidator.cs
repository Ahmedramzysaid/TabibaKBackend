using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class UpdateDoctorAdviceVideoValidator : AbstractValidator<UpdateDoctorAdviceVideoDto>
{
    public UpdateDoctorAdviceVideoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description != null && x.Description.Length > 0);
    }
}
