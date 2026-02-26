using StockPilot.Bmad.Domain.Clients;
using Xunit;

namespace StockPilot.Bmad.Tests.Clients;

public class ClientDomainTests
{
    [Fact]
    public void Create_ValidClient_SetsActiveStatus()
    {
        var now = DateTime.UtcNow;

        var client = Client.Create("ACME", "contact@acme.test", now);

        Assert.Equal(ClientStatus.Active, client.Status);
        Assert.Equal("ACME", client.Name);
        Assert.Equal("contact@acme.test", client.ContactEmail);
        Assert.Equal(now, client.CreatedAt);
        Assert.Equal(now, client.UpdatedAt);
    }

    [Fact]
    public void Create_WithoutName_Throws()
    {
        var now = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => Client.Create(" ", "contact@acme.test", now));
    }
}

