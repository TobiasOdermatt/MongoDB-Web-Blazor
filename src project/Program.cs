using MongoDB_Web.Controllers;
using MongoDB_Web.Data.Helpers;
using MongoDB_Web.Data.Hubs;
using MongoDB_Web.Data.OTP;
using static MongoDB_Web.Data.Helpers.LogManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

IConfiguration config = new ConfigurationBuilder()
                    .AddIniFile(Directory.GetCurrentDirectory()+"/config.properties", optional: false, reloadOnChange: false).Build();
ConfigManager configManager = new(config);
builder.Services.AddSingleton<ConfigManager>(configManager);
OTPFileManagement OTP = new();
OTP.CleanUpOTPFiles();
builder.Services.AddScoped<DBController>();
builder.Services.AddScoped<ImportManager>();
builder.Services.AddSingleton<AppData>();
builder.Services.AddSingleton<LogManager>();
LogManager log = new(LogType.Info, "Server started");

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers();
app.MapHub<ProgressHub>("/progressHub");
app.MapFallbackToPage("/_Host");

app.Run();