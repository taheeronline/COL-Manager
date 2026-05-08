using COLManager.Web.Data;
using COLManager.Web.Services;
using COLManager.Web.Services.Implementations;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Create Logs folder if it doesn't exist
var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
Directory.CreateDirectory(logDir);

// File name with current date
var logFileName = $"COLManager-{DateTime.Now:yyyy-MM-dd}.log";
var logPath = Path.Combine(logDir, logFileName);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        logPath,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10485760 // 10 MB
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Configure Kestrel for proper port binding
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    options.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps();
    });
});

var webBuilder = builder;

webBuilder.Services.AddControllersWithViews();
webBuilder.Services.AddRazorPages();
webBuilder.Services.AddServerSideBlazor();

// DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
webBuilder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(conn));

// Services
webBuilder.Services.AddScoped<IColumnService, ColumnService>();
webBuilder.Services.AddScoped<IUsageService, UsageService>();
webBuilder.Services.AddScoped<IProtocolService, ProtocolService>();
webBuilder.Services.AddScoped<IMeasurementService, MeasurementService>();
webBuilder.Services.AddScoped<IStatusService, StatusService>();
webBuilder.Services.AddScoped<IUnitService, UnitService>();
webBuilder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
webBuilder.Services.AddScoped<IAuditService, AuditService>();
webBuilder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}