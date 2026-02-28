using Moq;
using StockPilot.Bmad.Application.Clients;
using StockPilot.Bmad.Domain.Clients;

namespace StockPilot.Bmad.Tests.Clients;

public class ClientUpdateServiceTests
{
    private static Client MakeActiveClient()
        => Client.Create("Client Test", "test@test.com", DateTime.UtcNow);

    private static Client MakeInactiveClient()
    {
        var c = Client.Create("Client Test", "test@test.com", DateTime.UtcNow);
        c.SetInactive(DateTime.UtcNow);
        return c;
    }

    // --- UPDATE ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var client = MakeActiveClient();

        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        repo.Setup(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ClientService(repo.Object);
        var request = new UpdateClientRequest { Name = "Nouveau Nom", ContactEmail = "nouveau@test.com" };

        var result = await service.UpdateAsync(client.ClientId, request);

        Assert.Equal("Nouveau Nom", result.Name);
        Assert.Equal("nouveau@test.com", result.ContactEmail);
    }

    [Fact]
    public async Task UpdateAsync_ClientNotFound_ThrowsClientNotFoundException()
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = new ClientService(repo.Object);
        var request = new UpdateClientRequest { Name = "Test", ContactEmail = "test@test.com" };

        await Assert.ThrowsAsync<ClientNotFoundException>(() => service.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ThrowsValidationException()
    {
        var repo = new Mock<IClientRepository>();
        var service = new ClientService(repo.Object);
        var request = new UpdateClientRequest { Name = " ", ContactEmail = "test@test.com" };

        var ex = await Assert.ThrowsAsync<ClientValidationException>(() => service.UpdateAsync(Guid.NewGuid(), request));
        Assert.Contains("name", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_InvalidEmail_ThrowsValidationException()
    {
        var repo = new Mock<IClientRepository>();
        var service = new ClientService(repo.Object);
        var request = new UpdateClientRequest { Name = "Test", ContactEmail = "invalidemail" };

        var ex = await Assert.ThrowsAsync<ClientValidationException>(() => service.UpdateAsync(Guid.NewGuid(), request));
        Assert.Contains("email", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_CallsUpdateRepository()
    {
        var client = MakeActiveClient();

        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        repo.Setup(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ClientService(repo.Object);
        await service.UpdateAsync(client.ClientId, new UpdateClientRequest { Name = "X", ContactEmail = "x@x.com" });

        repo.Verify(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DEACTIVATE ---

    [Fact]
    public async Task DeactivateAsync_ActiveClient_SetsInactive()
    {
        var client = MakeActiveClient();

        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        repo.Setup(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ClientService(repo.Object);
        await service.DeactivateAsync(client.ClientId);

        Assert.Equal(ClientStatus.Inactive, client.Status);
        repo.Verify(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_DoesNotCallUpdate()
    {
        var client = MakeInactiveClient();

        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(client.ClientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var service = new ClientService(repo.Object);
        await service.DeactivateAsync(client.ClientId);

        // Idempotent : UpdateAsync ne doit PAS être appelé
        repo.Verify(r => r.UpdateAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_ClientNotFound_ThrowsClientNotFoundException()
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var service = new ClientService(repo.Object);

        await Assert.ThrowsAsync<ClientNotFoundException>(() => service.DeactivateAsync(Guid.NewGuid()));
    }
}

