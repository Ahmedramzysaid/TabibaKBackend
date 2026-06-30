using DomainLayer.DTOs;
using FluentValidation;

namespace BusinessLayer.Validations;

public class CreateDoctorAdviceVideoValidator : AbstractValidator<CreateDoctorAdviceVideoDto>
{
    public CreateDoctorAdviceVideoValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().WithMessage("Title is required.")
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.VideoUrl)
            .NotNull().WithMessage("Video URL is required.")
            .NotEmpty().WithMessage("Video file must be uploaded.");
    }
}
