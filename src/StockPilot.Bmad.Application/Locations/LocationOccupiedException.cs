namespace StockPilot.Bmad.Application.Locations;

public class LocationOccupiedException : Exception
{
    public LocationOccupiedException(Guid locationId)
        : base($"Location '{locationId}' is OCCUPIED and cannot be disabled.")
    {
    }
}

