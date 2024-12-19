using Eurocode.Blazor.Demo.Shared;
using Eurocode.Blazor.Demo.Shared.Extensions;
using Eurocode.Blazor.Demo.Shared.SampleData;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddFluentUIComponents();
builder.Services.AddFluentUIDemoClientServices();

builder.Services.AddScoped<DataSource>();

await builder.Build().RunAsync();
