//used to control how C# objects and enum values are converted to and from JSON
using System.Text.Json.Serialization;
using event_management_system.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register database classes
builder.Services.AddScoped<auth_dbconnect>();
builder.Services.AddScoped<users_dbconnect>();
builder.Services.AddScoped<event_dbconnect>();
builder.Services.AddScoped<category_dbconnect>();

// Configure session services
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure enum values to be represented as string in JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// enable session
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();