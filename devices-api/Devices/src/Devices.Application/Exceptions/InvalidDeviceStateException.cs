namespace Devices.Application.Exceptions;

public class InvalidDeviceStateException: Exception
{
    public InvalidDeviceStateException(string state)
        : base($"Invalid device state: '{state}'. Valid states are: Available, InUse, Inactive.")
    {
    }
}