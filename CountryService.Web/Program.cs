using Calzolari.Grpc.AspNetCore.Validation;
using CountryService.gRPC.Compression;
using CountryService.Web;
using CountryService.Web.Interceptors;
using CountryService.Web.Services;
using CountryService.Web.Validator;
using Grpc.Core;
using Grpc.Net.Compression;
using System.IO.Compression;
using v1 = CountryService.Web.Services.v1;
using v2 = CountryService.Web.Services.v2;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.IgnoreUnknownServices = true;
    options.MaxReceiveMessageSize = 6291456;
    options.MaxSendMessageSize = 6291456;
    options.CompressionProviders = new List<ICompressionProvider>
    {
        new BrotliCompressionProvider(CompressionLevel.Optimal) // br
    };
    options.ResponseCompressionAlgorithm = "br";    // grpc-accept-encoding, and must match the compression provider declared in CompressionProviders collection
    options.ResponseCompressionLevel = CompressionLevel.Optimal;    // compression level used if not set on the provider
    options.Interceptors.Add<ExceptionInterceptor>(); // Register custom ExceptionInterceptor interceptor

    options.EnableMessageValidation();
});

builder.Services.AddGrpcValidation();
builder.Services.AddSingleton<ProtoService>();
builder.Services.AddValidator<CountryCreateRequestValidator>();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<CountryManagementService>();

var app = builder.Build();
app.MapGrpcReflectionService();
// Configure the HTTP request pipeline.

// API versioning
app.MapGrpcService<v1.CountryGrpcService>();
app.MapGrpcService<v2.CountryGrpcService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGet("/protos", (ProtoService protoService) =>
{
    return Results.Ok(protoService.GetAll());
});

app.MapGet("/protos/v{version:int}/{protoName}", (ProtoService protoSerivce, int version, string protoName) =>
{
    var filePath = protoSerivce.Get(version, protoName);

    if (filePath != null)
    {
        return Results.File(filePath);
    }

    return Results.NotFound();
});

app.MapGet("/protos/v{version:int}/{protoName}/view", async (ProtoService protoService, int version, string protoName) =>
{
    var text = await protoService.ViewAsync(version, protoName);

    if (!string.IsNullOrEmpty(text))
    {
        return Results.Text(text);
    }

    return Results.NotFound();
});

//app.Use(async (context, next) =>
//{
//    context.Response.ContentType = "application/grpc";
//    context.Response.Headers.Add("gprc-status", ((int)StatusCode.NotFound).ToString());
//    await next();
//});

app.Run();
