using Devices.Application.DTOs;
using FluentValidation;

namespace Devices.Application.Validators;

public class CreateDeviceValidator: AbstractValidator<CreateDeviceDto>
{
    public CreateDeviceValidator()
    {
        // O Name é obrigatório e não pode ter mais de 100 caracteres
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        // O Brand é obrigatório e não pode ter mais de 100 caracteres
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        // O State tem que ser um valor válido do enum
        RuleFor(x => x.State)
            .IsInEnum().WithMessage("State must be a valid DeviceState (Available, InUse, Inactive).");
    }
}
