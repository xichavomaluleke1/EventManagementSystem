using event_management_system.Database;
using event_management_system.Database.Interfaces;
using System.Reflection;
using System.Text.Json.Serialization;
using event_management_system.Services;
using event_management_system.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================
// Swagger with XML comments
// ============================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "Event management API for the South African running, walking, and cycling community. Supports Organiser and Participant roles with session-based authentication."
    });

    var xml_file = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xml_path = System.IO.Path.Combine(AppContext.BaseDirectory, xml_file);
    options.IncludeXmlComments(xml_path);
});

// Database registrations
builder.Services.AddScoped<Iauth_dbconnect, auth_dbconnect>();
builder.Services.AddScoped<Iusers_dbconnect, users_dbconnect>();
builder.Services.AddScoped<Ievent_dbconnect, event_dbconnect>();
builder.Services.AddScoped<Icategory_dbconnect, category_dbconnect>();
builder.Services.AddScoped<Ienrolment_dbconnect, enrolment_dbconnect>();
builder.Services.AddScoped<Iresult_dbconnect, result_dbconnect>();

builder.Services.AddScoped<Iblob_storage_service, blob_storage_service>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Enum → string in JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }