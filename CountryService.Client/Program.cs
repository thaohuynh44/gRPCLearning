using Apress.Sample.gRPC.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using static Apress.Sample.gRPC.v1.CountryService;

var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Trace);
});

var channel = GrpcChannel.ForAddress("https://localhost:7029", new GrpcChannelOptions
{
    LoggerFactory = loggerFactory
});

var countryClient = new CountryServiceClient(channel);


// SERVER STREAMING
using var serverStreamingCall = countryClient.GetAll(new Empty());
await foreach(var response in serverStreamingCall.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"{response.Name}: {response.Description}");
}

//Read headers and trailers
var serverStreamingCallHeaders = await serverStreamingCall.ResponseHeadersAsync;
var serverStreamingCallTrailers = serverStreamingCall.GetTrailers();
var myHeaderValue = serverStreamingCallHeaders.GetValue("myHeaderName");
var myTrailerValue = serverStreamingCallTrailers.GetValue("myTrailerName");



// CLIENT STREAMING
using var clientStreamingCall = countryClient.Delete();
var countriesToDelete = new List<CountryIdRequest>
{
    new CountryIdRequest
    {
        Id = 1
    },
    new CountryIdRequest
    {
        Id = 2
    }
};

// Write
foreach(var countryToDelete in countriesToDelete)
{
    await clientStreamingCall.RequestStream.WriteAsync(countryToDelete);
    Console.WriteLine($"Country with Id {countryToDelete.Id} set for deletion");
}

//Tells server that request streaming is done
await clientStreamingCall.RequestStream.CompleteAsync();

//Finish the call by getting the response
var emptyResponse = await clientStreamingCall.ResponseAsync;

// Read headers and trailers
var clientStreamingCallHeaders = await clientStreamingCall.ResponseHeadersAsync;
var clientStreamingCallTrailers = clientStreamingCall.GetTrailers();
var deleteHeaderValue = clientStreamingCallHeaders.GetValue("myHeaderName");
var deleteTrailerValue = clientStreamingCallTrailers.GetValue("myTrailerName");

var anotherWayToReadEmptyResponse = await clientStreamingCall; // works as well but cannot read headers and trailers


// BIDIRECTIONAL STREAMING
using var bidirectionalStreamingCall = countryClient.Create();
var countriesToCreate = new List<CountryCreationRequest>
{
    new CountryCreationRequest
    {
        Name = "France",
        Description = "Western european country",
        CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc))
    },
    new CountryCreationRequest
    {
        Name = "Poland",
        Description = "Eastern european country",
        CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc))
    }
};

//Write 
foreach (var countryToCreate in countriesToCreate)
{
    await bidirectionalStreamingCall.RequestStream.WriteAsync(countyToCreate);
    Console.WriteLine($"Country {countryToCreate.Name} set for creation");
}

