namespace StockPilot.Bmad.Application.Locations;

public class LocationValidationException : Exception
{
    public LocationValidationException(string message) : base(message)
    {
    }
}

