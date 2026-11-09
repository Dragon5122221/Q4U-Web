using System.Globalization;
using System.Threading.RateLimiting;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Q4UPrototypeWeb.Services;

var builder = WebApplication.CreateBuilder(args);
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-ZA");
var demo = builder.Configuration["Q4U:Mode"] == "Demo";
if (demo && !builder.Environment.IsDevelopment()) throw new InvalidOperationException("Demo mode is restricted to Development. Configure Firebase before deploying.");
if (!demo && builder.Configuration["Q4U:Mode"] != "Firebase") throw new InvalidOperationException("Choose Q4U:Mode Demo or Firebase.");
builder.Services.AddControllersWithViews(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IdentityService>();
builder.Services.AddScoped<WorkspaceService>();
builder.Services.AddScoped<DocumentWorkflow>();
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddAuthentication(o => { o.DefaultScheme="Q4U"; o.DefaultChallengeScheme="Q4U"; })
    .AddPolicyScheme("Q4U","Cookie or Firebase bearer",o => o.ForwardDefaultSelector = context => context.Request.Path.StartsWithSegments("/api") ? "Firebase" : "Cookies")
    .AddCookie("Cookies", o =>
    {
        o.LoginPath="/Account/Login"; o.AccessDeniedPath="/Account/Denied";
        o.Cookie.HttpOnly=true; o.Cookie.SameSite=SameSiteMode.Lax;
        o.Cookie.SecurePolicy=demo ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        o.ExpireTimeSpan=TimeSpan.FromMinutes(50); o.SlidingExpiration=false;
        o.Events.OnValidatePrincipal=async context =>
        {
            if(demo) return;
            try { context.ReplacePrincipal(await context.HttpContext.RequestServices.GetRequiredService<IdentityService>().Verify(context.Principal!.FindFirst("firebase_token")!.Value)); }
            catch { context.RejectPrincipal(); await context.HttpContext.SignOutAsync("Cookies"); }
        };
    }).AddScheme<AuthenticationSchemeOptions,FirebaseBearerHandler>("Firebase",_ => {});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode=429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit=20, Window=TimeSpan.FromMinutes(1), QueueLimit=0 }));
});
if(demo)
{
    builder.Services.AddSingleton<IWorkspaceRepository,DemoWorkspaceRepository>();
    builder.Services.AddSingleton<IDocumentStorage,LocalDocumentStorage>();
}
else
{
    var project=builder.Configuration["Firebase:ProjectId"];
    if(string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(builder.Configuration["Firebase:StorageBucket"])) throw new InvalidOperationException("Firebase ProjectId and StorageBucket are required.");
    builder.Services.AddSingleton(_ => FirebaseApp.Create(new AppOptions { Credential=GoogleCredential.GetApplicationDefault(), ProjectId=project }));
    builder.Services.AddSingleton(sp => FirebaseAuth.GetAuth(sp.GetRequiredService<FirebaseApp>()));
    builder.Services.AddSingleton(_ => FirestoreDb.Create(project));
    builder.Services.AddSingleton(_ => StorageClient.Create());
    builder.Services.AddScoped<IWorkspaceRepository, FirestoreCollectionRepository>();
    builder.Services.AddSingleton<IDocumentStorage,FirebaseDocumentStorage>();
}
var app=builder.Build();
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("K_SERVICE")))
{
    // Cloud Run handles HTTPS before forwarding requests to the app.
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next(context);
    });
}
if(!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.Use(async (context,next) =>
{
    context.Response.Headers["X-Content-Type-Options"]="nosniff";
    context.Response.Headers["Referrer-Policy"]="strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"]="DENY";
    if(context.Request.Path.StartsWithSegments("/Workspace") || context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/Account")) context.Response.Headers.CacheControl="no-store";
    // Old deep links keep working, but all operational pages use the shared workspace.
    var area=context.Request.Path.Value?.Split('/',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    var sections=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) { ["Portal"]="Dashboard", ["Vehicles"]="Vehicles", ["Renewals"]="Renewals", ["Documents"]="Documents", ["ServiceRequests"]="Requests", ["Payments"]="Payments", ["Settings"]="Settings", ["Notifications"]="Notifications" };
    if(area != null && sections.TryGetValue(area,out var section))
    {
        if(area.Equals("Portal",StringComparison.OrdinalIgnoreCase)) { var leaf=context.Request.Path.Value!.Split('/').Last(); if(WorkspaceService.Sections.Contains(leaf)) section=leaf; }
        context.Response.Redirect("/Workspace?section="+section); return;
    }
    await next();
});
app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
app.MapStaticAssets();
app.MapControllers();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
