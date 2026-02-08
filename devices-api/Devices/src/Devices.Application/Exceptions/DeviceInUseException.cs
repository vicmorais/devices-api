namespace Devices.Application.Exceptions;

public class DeviceInUseException: Exception
{
    public DeviceInUseException(string operation)
        : base($"Cannot {operation} a device that is currently in use.")
    {
    }
}
