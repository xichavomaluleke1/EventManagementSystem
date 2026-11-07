var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();

// Add Session (to store our MVC auth state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Stay logged in for an hour
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register a singleton CookieContainer to hold the API session cookies
builder.Services.AddSingleton<System.Net.CookieContainer>();

// Configure HttpClient to point to our API AND use the CookieContainer
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    // Read from environment variable (for Docker), fallback to localhost (for local run)
    string apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://localhost:7284/";
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var cookieContainer = sp.GetRequiredService<System.Net.CookieContainer>();
    return new HttpClientHandler
    {
        CookieContainer = cookieContainer,
        UseCookies = true,
        // Bypass SSL certificate validation (needed for Docker to talk to localhost API over HTTPS)
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

// Register the API service wrapper
builder.Services.AddScoped<RaceDayMVC.Services.IApiService, RaceDayMVC.Services.ApiService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// I removed app.UseHttpsRedirection() because Docker is serving HTTP on port 8080 internally.
// If you want to test HTTPS locally, you can add it back conditionally, but HTTP is fine for the container.

app.UseStaticFiles();
app.UseRouting();

// Enable Session BEFORE Auth
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();