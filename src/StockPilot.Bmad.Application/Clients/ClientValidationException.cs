namespace StockPilot.Bmad.Application.Clients;

public class ClientValidationException : Exception
{
    public ClientValidationException(string message) : base(message)
    {
    }
}

