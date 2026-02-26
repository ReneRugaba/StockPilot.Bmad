using Moq;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Domain.Clients;
using Xunit;

namespace StockPilot.Bmad.Tests.Clients;

public class ClientServiceTests
{
    [Fact]
    public async Task CreateClient_ValidRequest_ReturnsActiveClientDto()
    {
        var repoMock = new Mock<IClientRepository>();
        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client c, CancellationToken _) => c);

        var service = new ClientService(repoMock.Object);

        var request = new CreateClientRequest
        {
            Name = "ACME",
            ContactEmail = "contact@acme.test"
        };

        var result = await service.CreateClientAsync(request);

        Assert.Equal("ACME", result.Name);
        Assert.Equal("CONTACT@ACME.TEST", result.ContactEmail.ToUpperInvariant());
        Assert.Equal("ACTIVE", result.Status);
        Assert.NotEqual(Guid.Empty, result.ClientId);
    }

    [Fact]
    public async Task CreateClient_WithoutName_ThrowsValidationException()
    {
        var repoMock = new Mock<IClientRepository>();
        var service = new ClientService(repoMock.Object);

        var request = new CreateClientRequest
        {
            Name = " ",
            ContactEmail = "contact@acme.test"
        };

        await Assert.ThrowsAsync<ClientValidationException>(() => service.CreateClientAsync(request));
    }

    [Fact]
    public async Task GetClientById_NotFound_ReturnsNull()
    {
        var repoMock = new Mock<IClientRepository>();
        repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = new ClientService(repoMock.Object);

        var result = await service.GetClientByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}

