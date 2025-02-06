using Eurocode.Blazor.Demo.Shared.Extensions;
using Eurocode.Blazor.Demo.Shared.SampleData;
using Eurocode.Grondslagen;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.FluentUI.AspNetCore.Components;


var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient();
builder.Services.AddServerSideBlazor();

builder.Services.AddFluentUIComponents();
builder.Services.AddFluentUIDemoServerServices();

//builder.Services.AddScoped<DemoMainLayout>();

builder.Services.AddScoped<ExportFactory.Services.MigraDocCreator>();

builder.Services.AddScoped<DataSource>();

builder.Services.AddCascadingValue(sp =>
    new SampleProject(new GrondslagenContext()));



//builder.WebHost.UseStaticWebAssets(); // < -- nodig voor wwwroot?

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseStaticFiles(); // <-- nodig voor files in wwwroot?
app.MapStaticAssets();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
//app.UseStaticFiles();
//app.UseAntiforgery();

//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

app.Run();