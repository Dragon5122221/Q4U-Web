using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;
using Xunit;

namespace Q4UPrototypeWeb.Tests;

public class DocumentWorkflowTests
{
    private readonly Actor customer = new("customer-one", "test-tenant", "Customer", "Test Customer", false);
    private readonly TestRepository repository = new();
    private readonly TestStorage storage = new();
    private DocumentWorkflow Workflow => new(new WorkspaceService(repository), storage, NullLogger<DocumentWorkflow>.Instance);
    private static WorkspaceCommand Command() => new() { Id = "car", Value = "Registration" };
    private static FormFile File(string name, byte[] bytes) => new(new MemoryStream(bytes), 0, bytes.Length, "file", name);

    public DocumentWorkflowTests()
    {
        repository.State.Vehicles.Add(new() { Id = "car", OwnerUid = customer.Uid });
    }

    [Fact]
    public async Task ValidPdfStoresBytesAndOwnedMetadata()
    {
        var bytes = "%PDF-1.7 test content"u8.ToArray();
        await Workflow.Save(customer, Command(), File("registration.pdf", bytes));
        var document = Assert.Single(repository.State.Documents);
        Assert.Equal(customer.Uid, document.OwnerUid);
        Assert.Equal("car", document.VehicleId);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.Equal(bytes, storage.Files[document.StorageKey]);
    }

    [Theory]
    [InlineData("registration.pdf", "not a PDF")]
    [InlineData("registration.png", "%PDF-1.7")]
    public async Task InvalidContentOrExtensionDoesNotUpload(string name, string content)
    {
        await Assert.ThrowsAsync<DomainException>(() => Workflow.Save(customer, Command(), File(name, System.Text.Encoding.UTF8.GetBytes(content))));
        Assert.Equal(0, storage.Writes);
        Assert.Empty(repository.State.Documents);
    }

    [Fact]
    public async Task OversizedFileDoesNotUpload()
    {
        var file = new FormFile(Stream.Null, 0, 5 * 1024 * 1024 + 1, "file", "large.pdf");
        await Assert.ThrowsAsync<DomainException>(() => Workflow.Save(customer, Command(), file));
        Assert.Equal(0, storage.Writes);
    }

    [Fact]
    public async Task FailedMetadataSaveRemovesUploadedFile()
    {
        repository.RejectSave = true;
        await Assert.ThrowsAsync<DomainException>(() => Workflow.Save(customer, Command(), File("registration.pdf", "%PDF-1.7"u8.ToArray())));
        Assert.Equal(1, storage.Writes);
        Assert.Equal(1, storage.Deletes);
        Assert.Empty(storage.Files);
        Assert.Empty(repository.State.Documents);
    }

    [Fact]
    public async Task CustomerCannotDownloadAnotherCustomersDocument()
    {
        repository.State.Documents.Add(new() { Id = "private-file", OwnerUid = "customer-two", StorageKey = "private-key" });
        var error = await Assert.ThrowsAsync<DomainException>(() => Workflow.Download(customer, "private-file"));
        Assert.Equal(404, error.Status);
    }
}
