using EasterCMS;
using EasterCMS.Components;
using EasterCMS.Data;
using EasterCMS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();


builder.Services.AddHttpClient("api", 
	x => x.BaseAddress = new Uri(builder.Configuration["Api:Url"] ?? throw new Exception("Missing Api:Url")));
builder.Services.AddTransient<ApiClient>();

builder.AddNpgsqlDbContext<AppDbContext>("db");


builder.Services.Scan(scan => scan
	.FromAssemblyOf<IEndpoint>()
	.AddClasses(x => x.AssignableTo<IEndpoint>())
	.AsImplementedInterfaces()
	.WithSingletonLifetime());


var app = builder.Build();



app.MapDefaultEndpoints();
if(!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
} else
{
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await db.Database.MigrateAsync();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapApiEndpoints("/api");
app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();
app.Run();
