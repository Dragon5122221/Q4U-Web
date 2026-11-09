using Q4UPrototypeWeb.Models;
namespace Q4UPrototypeWeb.Services;
public class DocumentWorkflow(WorkspaceService service, IDocumentStorage storage, ILogger<DocumentWorkflow> logger)
{
    public async Task Save(Actor actor, WorkspaceCommand command, IFormFile? file)
    {
        if(file==null || file.Length==0 || file.Length>5*1024*1024) throw new DomainException("Choose a PDF, PNG or JPEG up to 5 MB.");
        var state=await service.Get(actor);
        if(!state.Vehicles.Any(v=>v.Id==command.Id && !v.Archived)) throw new DomainException("Select one of your active vehicles.",403);
        if(state.Version!=command.Version) throw new DomainException("This workspace changed. Refresh before uploading.",409);
        using var stream=new MemoryStream(); await file.CopyToAsync(stream); var bytes=stream.ToArray();
        var type=bytes.AsSpan().StartsWith("%PDF-"u8) ? "application/pdf" : bytes.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10}) ? "image/png" : bytes.AsSpan().StartsWith(new byte[]{255,216,255}) ? "image/jpeg" : "";
        if(type=="") throw new DomainException("The file content must be PDF, PNG or JPEG.");
        var name=Path.GetFileName(file.FileName.Replace('\\','/'));
        if(name.Length is 0 or >150 || name.Any(char.IsControl)) throw new DomainException("Use a valid filename shorter than 150 characters.");
        var expectedExtension=type=="application/pdf" ? new[]{".pdf"} : type=="image/png" ? new[]{".png"} : new[]{".jpg",".jpeg"};
        if(!expectedExtension.Contains(Path.GetExtension(name).ToLowerInvariant())) throw new DomainException("The filename extension must match the file content.");
        var key=$"tenants/{actor.TenantId}/{Guid.NewGuid():N}";
        await storage.Put(key,bytes,type);
        try
        {
            command.Action="upload";
            await service.Apply(actor,command,new DocumentRecord { FileName=name,ContentType=type,StorageKey=key,Bytes=bytes.Length });
        }
        catch
        {
            try { await storage.Delete(key); } catch(Exception e) { logger.LogError(e,"An unreferenced document could not be removed"); }
            throw;
        }
    }
    public async Task<(byte[] Bytes, DocumentRecord Document)> Download(Actor actor,string id)
    {
        var doc=(await service.Get(actor)).Documents.FirstOrDefault(d=>d.Id==id) ?? throw new DomainException("Document not found.",404);
        return (await storage.Get(doc.StorageKey),doc);
    }
}
