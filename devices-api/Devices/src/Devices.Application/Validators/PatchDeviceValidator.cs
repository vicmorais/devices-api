using Devices.Application.DTOs;
using FluentValidation;

namespace Devices.Application.Validators;

public class PatchDeviceValidator: AbstractValidator<PatchDeviceDto>
{
    public PatchDeviceValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.")
            .When(x => x.Brand is not null);

        RuleFor(x => x.State)
            .IsInEnum().WithMessage("State must be a valid DeviceState (Available, InUse, Inactive).")
            .When(x => x.State is not null);
    }
}
