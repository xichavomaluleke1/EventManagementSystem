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
    // Use your API's HTTPS port. Check your API's launchSettings.json or console for the exact port.
    client.BaseAddress = new Uri("https://localhost:7284/");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var cookieContainer = sp.GetRequiredService<System.Net.CookieContainer>();
    return new HttpClientHandler
    {
        CookieContainer = cookieContainer,
        UseCookies = true
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable Session BEFORE Auth
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();