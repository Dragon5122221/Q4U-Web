using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;

namespace Q4UPrototypeWeb.Tests;

// Copies simulate separate reads, so rejected changes cannot alter stored records.
internal class TestRepository : IWorkspaceRepository
{
    public WorkspaceState State { get; set; } = new();
    public int Saves { get; private set; }
    public bool RejectSave { get; set; }

    public Task<WorkspaceState> Read(string tenant) =>
        Task.FromResult(StateJson.Read(StateJson.Write(State)));

    public Task Save(string tenant, WorkspaceState state, long expectedVersion)
    {
        if (RejectSave || State.Version != expectedVersion)
            throw new DomainException("The workspace changed.", 409);
        state.Version = expectedVersion + 1;
        State = StateJson.Read(StateJson.Write(state));
        Saves++;
        return Task.CompletedTask;
    }
}

internal class TestStorage : IDocumentStorage
{
    public Dictionary<string, byte[]> Files { get; } = new();
    public int Writes { get; private set; }
    public int Deletes { get; private set; }
    public Task Put(string key, byte[] bytes, string contentType)
    {
        Files[key] = bytes;
        Writes++;
        return Task.CompletedTask;
    }
    public Task<byte[]> Get(string key) => Task.FromResult(Files[key]);
    public Task Delete(string key)
    {
        Files.Remove(key);
        Deletes++;
        return Task.CompletedTask;
    }
}
