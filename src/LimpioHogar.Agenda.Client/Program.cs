using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using LimpioHogar.Agenda.Client;
using LimpioHogar.Agenda.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();

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
        { "Prefer", "return=representation" }
    }
});

builder.Services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();
builder.Services.AddScoped<SupabaseAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SupabaseAuthStateProvider>());

builder.Services.AddScoped<ISupabaseService, SupabaseService>();
builder.Services.AddScoped<ITrabajadoraService, TrabajadoraService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IPagoService, PagoService>();

var host = builder.Build();

var authService = host.Services.GetRequiredService<ISupabaseAuthService>();
await authService.InitializeAsync();

await host.RunAsync();
