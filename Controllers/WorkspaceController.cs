using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;
namespace Q4UPrototypeWeb.Controllers;
[Authorize]
public class WorkspaceController(WorkspaceService service, IdentityService identity, DocumentWorkflow documents, ILogger<WorkspaceController> logger) : Controller
{
    private Actor Actor => identity.Current(User);
    [HttpGet]
    public async Task<IActionResult> Index(string section="Dashboard", string q="", string filter="")
    {
        if(Actor.Role=="Developer" && section=="Dashboard") section="Developer";
        if(!WorkspaceService.Sections.Contains(section)) return NotFound();
        if(!WorkspaceService.CanSection(Actor,section)) return Forbid();
        return View(new WorkspacePage { Actor=Actor, State=await service.Get(Actor), Section=section, Query=q.Length>100 ? q[..100] : q, Filter=filter });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Command(WorkspaceCommand command, string section="Dashboard")
    {
        if(!ModelState.IsValid) TempData["Error"]="Check the form fields and try again. Text or numbers exceed the allowed limits.";
        else try { await service.Apply(Actor,command); TempData["Success"] = command.Action=="invite" ? "Invitation draft saved. No email has been sent and no access has been granted." : "Your change has been saved."; }
        catch(DomainException e) { TempData["Error"]=e.Message; }
        return RedirectToAction(nameof(Index),new { section=WorkspaceService.Sections.Contains(section) ? section : "Dashboard" });
    }
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6*1024*1024)]
    public async Task<IActionResult> Upload(WorkspaceCommand command,IFormFile? file)
    {
        try
        {
            if(!ModelState.IsValid) throw new DomainException("Check the document fields.");
            await documents.Save(Actor,command,file);
            TempData["Success"]="Document uploaded and awaiting review.";
        }
        catch(DomainException e) { TempData["Error"]=e.Message; }
        catch(Exception e) { logger.LogError(e,"Document upload failed"); TempData["Error"]="The upload could not be saved. Try again."; }
        return RedirectToAction(nameof(Index),new { section="Documents" });
    }
    [HttpGet]
    public async Task<IActionResult> Download(string id)
    {
        try { var result=await documents.Download(Actor,id); return File(result.Bytes,result.Document.ContentType,result.Document.FileName); }
        catch(DomainException e) { return StatusCode(e.Status,e.Message); }
        catch { return NotFound("The document is no longer available. Please upload it again."); }
    }
    [HttpGet]
    public async Task<IActionResult> Export(string format="json")
    {
        if(Actor.Role=="Developer") return Forbid();
        var state=await service.Get(Actor);
        if(format=="csv")
        {
            string Cell(string s) => "\""+((s.Length>0 && "=+-@\t\r".Contains(s[0])) ? "'"+s : s).Replace("\"","\"\"")+"\"";
            var csv="Registration,Vehicle,Licence expiry,Driver,Branch,Archived\r\n"+string.Join("\r\n",state.Vehicles.Select(v=>string.Join(",",Cell(v.Registration),Cell(v.Description),v.Expiry.ToString("yyyy-MM-dd"),Cell(v.Driver),Cell(v.Branch),v.Archived)));
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray(),"text/csv","Q4U-vehicles.csv");
        }
        // Storage keys are internal and never included in client exports.
        foreach(var d in state.Documents) d.StorageKey="";
        return File(JsonSerializer.SerializeToUtf8Bytes(state,StateJson.Options),"application/json","Q4U-account-export.json");
    }
    [HttpGet]
    public async Task<IActionResult> Receipt(string id)
    {
        if(!WorkspaceService.CanSection(Actor,"Payments")) return Forbid();
        var state=await service.Get(Actor); var request=state.Requests.FirstOrDefault(r=>r.Id==id && r.Paid);
        if(request==null) return NotFound();
        ViewBag.Demo=Actor.Demo; return View(request);
    }
}
