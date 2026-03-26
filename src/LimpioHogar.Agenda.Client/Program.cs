using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using LimpioHogar.Agenda.Client;
using LimpioHogar.Agenda.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

var supabaseUrl = builder.Configuration["Supabase:Url"]!;
var supabaseKey = builder.Configuration["Supabase:AnonKey"]!;

builder.Services.AddScoped(_ =>
    new Supabase.Client(
        supabaseUrl,
        supabaseKey,
        new Supabase.SupabaseOptions { AutoRefreshToken = true }
    )
);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(supabaseUrl),
    DefaultRequestHeaders =
    {
        { "apikey", supabaseKey },
        { "Authorization", $"Bearer {supabaseKey}" },
        { "Prefer", "return=representation" }
    }
});

builder.Services.AddScoped<ISupabaseService, SupabaseService>();
builder.Services.AddScoped<ITrabajadoraService, TrabajadoraService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IPagoService, PagoService>();

await builder.Build().RunAsync();
