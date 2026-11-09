using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;
namespace Q4UPrototypeWeb.Controllers;
[ApiController, Route("api/v1/workspace"), Authorize(AuthenticationSchemes="Firebase")]
public class WorkspaceApiController(WorkspaceService service,IdentityService identity,DocumentWorkflow documents, ILogger<WorkspaceApiController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var state=await service.Get(identity.Current(User));
        foreach(var d in state.Documents) d.StorageKey="";
        Response.Headers.ETag=$"\"{state.Version}\"";
        return Ok(state);
    }
    [HttpPost("commands")]
    public async Task<IActionResult> Command(WorkspaceCommand command)
    {
        try { await service.Apply(identity.Current(User),command); return Ok(new { message="Saved. Fetch the latest workspace before the next command." }); }
        catch(DomainException e) { return Problem(title:e.Message,statusCode:e.Status); }
    }
    [HttpPost("documents"), RequestSizeLimit(6*1024*1024)]
    public async Task<IActionResult> Upload([FromForm] WorkspaceCommand command, IFormFile? file)
    {
        try { await documents.Save(identity.Current(User),command,file); return Ok(new { message="Uploaded for review. Fetch the latest workspace." }); }
        catch(DomainException e) { return Problem(title:e.Message,statusCode:e.Status); }
        catch(Exception e) { logger.LogError(e,"Document upload failed"); return Problem(title:"Document upload failed. Try again.",statusCode:503); }
    }
    [HttpGet("documents/{id}")]
    public async Task<IActionResult> Download(string id)
    {
        try { var result=await documents.Download(identity.Current(User),id); return File(result.Bytes,result.Document.ContentType,result.Document.FileName); }
        catch(DomainException e) { return Problem(title:e.Message,statusCode:e.Status); }
        catch { return Problem(title:"The document is not available.",statusCode:404); }
    }
    [HttpGet("me")]
    public IActionResult Me() => Ok(identity.Current(User));
}
