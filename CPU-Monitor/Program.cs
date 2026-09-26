using CPU_Monitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<CpuMonitoringService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CpuMonitor}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseDeveloperExceptionPage();

app.MapGet("/test", () => "OK");


app.Run();
