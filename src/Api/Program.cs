using Api;
using Carter;
using Core;
using Core.Ods.Abstractions;
using Core.Pds.Abstractions;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi(builder.Configuration)
    .AddJobs(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddCore();

builder.Services.AddCors(p => p.AddPolicy("DexCorsPolicy", policyBuilder =>
{
    if (builder.Environment.IsEnvironment("Local"))
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("DexCorsPolicy");
app.MapHealthChecks("/_health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapGet("/", () => "Welcome to Healthcare Data Exchange. Please refer to the documentation for detailed usage guidelines.").WithName("GetIndex");
app.MapPost("/internal/run/ods", app.Services.GetRequiredService<IOdsService>().IngestCsvDownloads).WithName("IngestCsvDownloads").WithTags("RequiredRole=DataAdministrator");
app.MapPost("/internal/run/pds", app.Services.GetRequiredService<IPdsService>().RetrieveMeshMessages).WithName("RetrievePdsMeshMessages").WithTags("RequiredRole=DataAdministrator");
app.MapCarter();

if (app.Environment.IsEnvironment("Local"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var openApiVersion = builder.Configuration.GetValue<string>("OpenApi:Version")!;
        c.SwaggerEndpoint($"/swagger/{openApiVersion}/swagger.json", openApiVersion);
    });
}

app.Run();

public partial class Program;