using IdentityService.Infrastructure;
using Shared.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddControllers();
builder.Services.AddOpenApi();


await builder.Services.AddInfrastructureServices(builder.Configuration, Assembly.GetExecutingAssembly());
builder.Host.BuildInfrastructure(
    configuration: builder.Configuration,
    serviceVersion: "1.0.0"
    );

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.UseInfrastructure();
_ = app.PublishIdentityResources<Program>();

app.MapControllers();

app.Run();



