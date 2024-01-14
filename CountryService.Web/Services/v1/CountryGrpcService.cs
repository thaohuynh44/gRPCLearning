using static Apress.Sample.gRPC.v1.CountryService;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Apress.Sample.gRPC.v1;

namespace CountryService.Web.Services.v1;

public class CountryGrpcService : CountryServiceBase
{
    private readonly CountryManagementService _countryManagementService;
    private readonly ILogger<CountryGrpcService> _logger;
    public CountryGrpcService(CountryManagementService countryManagementService, ILogger<CountryGrpcService> logger)
    {
        _countryManagementService = countryManagementService;
        _logger = logger;
    }
    public override async Task GetAll(Empty request, IServerStreamWriter<CountryReply> responseStream, ServerCallContext context)
    {

        // Simulating an exception here

        // 1st way to handle error, using try/catch block directly here!
        //try
        //{
        //    throw new Exception("Something got really wrong here!");
        //    // Stream all found countries to the client
        //    var countries = await _countryManagementService.GetAllAsync();
        //    foreach (var country in countries)
        //    {
        //        await responseStream.WriteAsync(country);
        //    }
        //    await Task.CompletedTask;
        //}
        //catch(Exception e)
        //{
        //    var correlationId = Guid.NewGuid();
        //    _logger.LogError(e, "CorrelationId: {0}", correlationId);

        //    var trailers = new Metadata();
        //    trailers.Add("CorrelationId", correlationId.ToString());

        //    //Adding the correlation to response trailers

        //    throw new RpcException(new Status(StatusCode.Internal,
        //        $"Error message sent to the client with a CorrelationId {correlationId}"), trailers, 
        //                                                    "Error message that will appear in log server");
        //}

        // 2nd way to handle error, using Custom Interceptor:

        throw new TimeoutException("Something got really wrong here");

        //Streams all found countries to the client
        var countries = await _countryManagementService.GetAllAsync();
        foreach (var country in countries)
        {
            await responseStream.WriteAsync(country);
        }

        await Task.CompletedTask;
    }

    public override async Task<CountryReply> Get(CountryIdRequest request, ServerCallContext context)
    {
        // Send a single country to the client in the gRPC response
        return await _countryManagementService.GetAsync(request);
    }

    public virtual async Task<Empty> Delete(IAsyncStreamReader<CountryIdRequest> requestStream, ServerCallContext context)
    {
        //throw new RpcException(new Status(StatusCode.Unimplemented, ""));

        // Read and store all streamed input messages
        var countryIdRequestList = new List<CountryIdRequest>();
        await foreach (var countryIdRequest in requestStream.ReadAllAsync())
        {
            countryIdRequestList.Add(countryIdRequest);
        }

        // Delete in one shot all streamed countries
        await _countryManagementService.DeleteAsync(countryIdRequestList);
        return new Empty();
    }

    public override async Task<Empty> Update(CountryUpdateRequest request, ServerCallContext context)
    {
        // Read input message from the gRPC request
        await _countryManagementService.UpdateAsync(request);
        return new Empty();
    }

    public override async Task Create(IAsyncStreamReader<CountryCreationRequest> requestStream, IServerStreamWriter<CountryCreationReply> responseStream, ServerCallContext context)
    {
        // Read and store all streamed input messages before performing any action
        var countryCreationRequestList = new List<CountryCreationRequest>();
        await foreach (var countryCreationRequest in requestStream.ReadAllAsync())
        {
            countryCreationRequestList.Add(countryCreationRequest);
        }

        // Call in one shot the countryManagementService that will perform creation operations
        var createdCountries = await _countryManagementService.CreateAsync(countryCreationRequestList);

        // Stream all created countries to the client
        foreach (var country in createdCountries)
        {
            await responseStream.WriteAsync(country);
        }
    }
}
