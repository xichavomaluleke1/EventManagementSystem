var builder = WebApplication.CreateBuilder(args);

//Add MVC
builder.Services.AddControllersWithViews();

//Add Session (to store our auth state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Stay logged in for an hour
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Configure HttpClient to point to our API
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    // Use your API's HTTPS port. Check your API's launchSettings.json or console for the exact port.
    client.BaseAddress = new Uri("https://localhost:7284/");
});

//Register the API service wrapper
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