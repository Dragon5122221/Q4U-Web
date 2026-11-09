using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;
using Xunit;

namespace Q4UPrototypeWeb.Tests;

public class WorkspaceServiceTests
{
    private static readonly Actor Customer = new("customer-one", "test-tenant", "Customer", "Test Customer", false);
    private readonly TestRepository repository = new();

    private static WorkspaceCommand NewVehicle() => new()
    {
        Action = "vehicle", Value = "  ab 123 gp  ", Details = "Suzuki Swift 2025",
        Date = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1)
    };

    [Fact]
    public async Task AddingVehicleNormalisesRegistrationAndSetsOwner()
    {
        await new WorkspaceService(repository).Apply(Customer, NewVehicle());
        var vehicle = Assert.Single(repository.State.Vehicles);
        Assert.Equal("AB 123 GP", vehicle.Registration);
        Assert.Equal(Customer.Uid, vehicle.OwnerUid);
        Assert.Equal(1L, repository.State.Version);
        Assert.Equal("vehicle", Assert.Single(repository.State.Audit).Action);
    }

    [Fact]
    public async Task DuplicateRegistrationDoesNotSave()
    {
        repository.State.Vehicles.Add(new() { Registration = "AB 123 GP" });
        await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, NewVehicle()));
        Assert.Equal(0, repository.Saves);
        Assert.Single(repository.State.Vehicles);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("ABC<script>")]
    [InlineData("")]
    public async Task InvalidRegistrationIsRejected(string registration)
    {
        var command = NewVehicle();
        command.Value = registration;
        await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, command));
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task StaleVersionReturnsConflict()
    {
        repository.State.Version = 2;
        var error = await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, NewVehicle()));
        Assert.Equal(409, error.Status);
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task CustomerCannotEditAnotherCustomersVehicle()
    {
        repository.State.Vehicles.Add(new() { Id = "other-car", OwnerUid = "customer-two" });
        var command = NewVehicle();
        command.Id = "other-car";
        var error = await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, command));
        Assert.Equal(404, error.Status);
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task CustomerOnlyReceivesOwnRecords()
    {
        foreach (var owner in new[] { Customer.Uid, "customer-two" })
        {
            repository.State.Vehicles.Add(new() { OwnerUid = owner });
            repository.State.Requests.Add(new() { OwnerUid = owner });
            repository.State.Documents.Add(new() { OwnerUid = owner });
        }
        var state = await new WorkspaceService(repository).Get(Customer);
        Assert.Equal(Customer.Uid, Assert.Single(state.Vehicles).OwnerUid);
        Assert.Equal(Customer.Uid, Assert.Single(state.Requests).OwnerUid);
        Assert.Equal(Customer.Uid, Assert.Single(state.Documents).OwnerUid);
        Assert.Equal(2, repository.State.Vehicles.Count);
    }

    [Fact]
    public async Task OpenRequestPreventsArchiving()
    {
        repository.State.Vehicles.Add(new() { Id = "car", OwnerUid = Customer.Uid });
        repository.State.Requests.Add(new() { VehicleId = "car", Status = "Submitted" });
        await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, new() { Action = "archive", Id = "car" }));
        Assert.False(repository.State.Vehicles[0].Archived);
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task CustomerCannotIssueQuote()
    {
        var error = await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, new() { Action = "quote", Amount = 100 }));
        Assert.Equal(403, error.Status);
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task AdministratorCanQuoteSubmittedRequest()
    {
        var request = new RequestRecord { OwnerUid = Customer.Uid };
        repository.State.Requests.Add(request);
        await new WorkspaceService(repository).Apply(Customer with { Role = "Administrator" }, new() { Action = "quote", Id = request.Id, Amount = 250 });
        Assert.Equal("Quoted", repository.State.Requests[0].Status);
        Assert.Equal(250m, repository.State.Requests[0].Amount);
        Assert.Equal(Customer.Uid, Assert.Single(repository.State.Notices).Uid);
    }

    [Fact]
    public async Task FirebaseUserCannotSimulatePayment()
    {
        var error = await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, new() { Action = "payDemo" }));
        Assert.Equal(403, error.Status);
        Assert.Equal(0, repository.Saves);
    }

    [Fact]
    public async Task DeliveryRequestRequiresAddress()
    {
        repository.State.Vehicles.Add(new() { Id = "car", OwnerUid = Customer.Uid });
        await Assert.ThrowsAsync<DomainException>(() => new WorkspaceService(repository).Apply(Customer, new() { Action = "request", Id = "car", Value = "Licence renewal", Extra = "Delivery" }));
        Assert.Empty(repository.State.Requests);
        Assert.Equal(0, repository.Saves);
    }
}
