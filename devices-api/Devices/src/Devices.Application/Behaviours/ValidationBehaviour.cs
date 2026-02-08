using FluentValidation;

namespace Devices.Application.Behaviours;

public static class ValidationBehaviour
{
    public static async Task ValidateAndThrowAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {

        var result = await validator.ValidateAsync(instance, cancellationToken);

        if(!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
