namespace StockPilot.Bmad.Application.Clients;

public class ClientNotFoundException : Exception
{
    public ClientNotFoundException(Guid clientId)
        : base($"Client '{clientId}' not found.")
    {
    }
}

