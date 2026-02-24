using Notification.Infrastructure;
using Notification.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddControllers();

builder.Services.AddOpenApi();

//builder.WebHost.ConfigureKestrel(options =>
//{
//    var certDirectory = builder.Configuration.GetValue<string>("Certificates:Directory")
//        ?? throw new InvalidOperationException("Certificates directory is not configured.");
//    var certPassword = builder.Configuration.GetValue<string>("Certificates:Password")
//        ?? throw new InvalidOperationException("Certificate password is not configured.");

//    options.ListenAnyIP(8081);
//    options.ListenAnyIP(8444, listenOptions =>
//    {
//        listenOptions.UseHttps(certDirectory, certPassword);
//    });
//});



await builder.Services.AddInfrastructureServices(builder.Configuration, typeof(Program).Assembly);

builder.Host.BuildNotificationInfrastructure(
    configuration: builder.Configuration,
    serviceVersion: "1.0.0"
    );
var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

await app.SeedDatabaseAsync();
_ = app.PublishNotificationResources<Program>();

app.MapControllers();

app.Run();
