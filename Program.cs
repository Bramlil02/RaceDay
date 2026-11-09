using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;
using System.Globalization;

// Use one culture everywhere so decimals and dates behave the same locally and in Docker.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Enable MVC controllers and views. Every POST is checked for an anti-forgery token.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Connect to the RaceDay API. The MVC project never touches the database or Azure directly.
string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");

bool allowInvalidCertificate =
    builder.Configuration.GetValue<bool>("ApiSettings:AllowInvalidCertificate");

builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // Only for local Docker development, where the API uses a developer certificate.
    if (allowInvalidCertificate)
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

// Session storage for the logged-in user's token, name and role
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

// Service that talks to the API
builder.Services.AddScoped<RaceDayApiService>();

var app = builder.Build();

// Friendly error pages instead of raw errors
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
