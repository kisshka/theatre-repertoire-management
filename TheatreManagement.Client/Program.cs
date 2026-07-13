using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TheatreManagement.Client;
using TheatreManagement.Client.Helpers;
using TheatreManagement.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiUrl;
if (builder.HostEnvironment.IsDevelopment())
{
    apiUrl = builder.Configuration["WebApiAdress"]!;
}
else
{
    apiUrl = builder.Configuration["WebApiAdressRelease"]!;
}

// Сервисы
builder.Services.AddScoped<PlayService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<InstitutionService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<StatementService>();
builder.Services.AddScoped<EventConflictCheckerService>();

// Аутентификация
builder.Services.AddScoped<AuthHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiUrl);
})
.AddHttpMessageHandler<AuthHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddSingleton(new ApiSettings { BaseUrl = apiUrl });
builder.Services.AddAntDesign();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();
await app.RunAsync();