namespace StockPilot.Bmad.Domain.Clients;

public class Client
{
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    public ClientStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Client() { }

    private Client(Guid clientId, string name, string contactEmail, ClientStatus status, DateTime createdAt, DateTime updatedAt)
    {
        ClientId = clientId;
        Name = name;
        ContactEmail = contactEmail;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Client Create(string name, string contactEmail, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Client name is required", nameof(name));
        }

        if (!IsValidEmail(contactEmail))
        {
            throw new ArgumentException("Contact email is invalid", nameof(contactEmail));
        }

        var id = Guid.NewGuid();

        return new Client(
            id,
            name.Trim(),
            contactEmail.Trim(),
            ClientStatus.Active,
            utcNow,
            utcNow);
    }

    public void SetInactive(DateTime utcNow)
    {
        Status = ClientStatus.Inactive;
        UpdatedAt = utcNow;
    }

    public void Update(string name, string contactEmail, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name is required", nameof(name));

        if (!IsValidEmail(contactEmail))
            throw new ArgumentException("Contact email is invalid", nameof(contactEmail));

        Name = name.Trim();
        ContactEmail = contactEmail.Trim();
        UpdatedAt = utcNow;
    }

    public void Touch(DateTime utcNow)
    {
        UpdatedAt = utcNow;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmed = email.Trim();

        // Validation minimale pour le MVP
        return trimmed.Contains("@") && trimmed.Contains('.') && trimmed.Length >= 3;
    }
}
