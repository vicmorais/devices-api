using Devices.Application.DTOs;
using Devices.Application.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Devices.Application.Validators;

public class CreateDeviceValidator: AbstractValidator<CreateDeviceDto>
{
    private readonly IDevicesDbContext _context;

    public CreateDeviceValidator(IDevicesDbContext context)
    {
        _context = context;

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

        // Unique Constraint Validation: Name + Brand
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                var exists = await _context.Devices
                    .AnyAsync(d => d.Name == dto.Name && d.Brand == dto.Brand, cancellation);

                return !exists;
            })
            .WithMessage("A device with this name already exists for the specified brand.")
            .WithName("Name");
    }
}
