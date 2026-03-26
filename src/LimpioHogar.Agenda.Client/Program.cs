using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using LimpioHogar.Agenda.Client;
using LimpioHogar.Agenda.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["Supabase:Url"] ?? builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<ISupabaseService, SupabaseService>();
builder.Services.AddScoped<ITrabajadoraService, TrabajadoraService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IPagoService, PagoService>();

await builder.Build().RunAsync();
