using BlazorServerMyMongo.Data.DB;
using BlazorServerMyMongo.Data.Helpers;
using BlazorServerMyMongo.Data.OTP;
using static BlazorServerMyMongo.Data.Helpers.LogManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();

IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("config.json", optional: false, reloadOnChange: false).Build();
ConfigManager configManager = new(config);
builder.Services.AddSingleton<ConfigManager>(configManager);
OTPFileManagement OTP = new();
OTP.CleanUpOTPFiles();
builder.Services.AddScoped<DBController>();
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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();