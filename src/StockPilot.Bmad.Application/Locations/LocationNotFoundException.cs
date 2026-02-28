namespace StockPilot.Bmad.Application.Locations;

public class LocationNotFoundException : Exception
{
    public LocationNotFoundException(Guid locationId)
        : base($"Location '{locationId}' not found.")
    {
    }
}

