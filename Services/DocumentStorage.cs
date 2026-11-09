using Google.Cloud.Storage.V1;
namespace Q4UPrototypeWeb.Services;
public interface IDocumentStorage
{
    Task Put(string key, byte[] bytes, string contentType);
    Task<byte[]> Get(string key);
    Task Delete(string key);
}
public class LocalDocumentStorage(IWebHostEnvironment environment) : IDocumentStorage
{
    private string PathFor(string key) => Path.Combine(environment.ContentRootPath, "App_Data", "Documents", key.Replace('/', Path.DirectorySeparatorChar));
    public async Task Put(string key, byte[] bytes, string contentType) { var path=PathFor(key); Directory.CreateDirectory(Path.GetDirectoryName(path)!); await File.WriteAllBytesAsync(path, bytes); }
    public Task<byte[]> Get(string key) => File.ReadAllBytesAsync(PathFor(key));
    public Task Delete(string key) { File.Delete(PathFor(key)); return Task.CompletedTask; }
}
public class FirebaseDocumentStorage(StorageClient client, IConfiguration config) : IDocumentStorage
{
    private string Bucket => config["Firebase:StorageBucket"]!;
    public async Task Put(string key, byte[] bytes, string contentType) { using var stream=new MemoryStream(bytes); await client.UploadObjectAsync(Bucket,key,contentType,stream); }
    public async Task<byte[]> Get(string key) { using var stream=new MemoryStream(); await client.DownloadObjectAsync(Bucket,key,stream); return stream.ToArray(); }
    public async Task Delete(string key) => await client.DeleteObjectAsync(Bucket,key);
}
