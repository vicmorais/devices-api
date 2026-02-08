using Devices.Application.DTOs;
using FluentValidation;

namespace Devices.Application.Validators;

public class UpdateDeviceValidator: AbstractValidator<UpdateDeviceDto>
{
    public UpdateDeviceValidator()
    {
        // Mesmas regras do Create porque o PUT exige todos os campos
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        RuleFor(x => x.State)
            .IsInEnum().WithMessage("State must be a valid DeviceState (Available, InUse, Inactive).");
    }
}
