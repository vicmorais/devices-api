namespace Devices.Application.Exceptions;

public class DeviceNotFoundException: Exception
{
    public DeviceNotFoundException(Guid id)
        : base($"Device with id '{id}' was not found.")
    {
    }
}
