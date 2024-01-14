using Apress.Sample.gRPC.v2;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Apress.Sample.gRPC.v2.CountryService;

namespace CountryService.Web.Services.v2
{
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
            // Stream all found countries to the client
            //var countries = await _countryManagementService.GetAllAsync();
            //foreach (var country in countries)
            //{
            //    await responseStream.WriteAsync(country);
            //}
            //await Task.CompletedTask;

            throw new RpcException(new Status(StatusCode.Unimplemented, ""));
        }

        public override Task<CountryReply> Get(CountryIdRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, ""));
        }

        public override Task<Empty> Update(CountryUpdateRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, ""));
        }

        public override Task Create(IAsyncStreamReader<CountryCreationRequest> requestStream, IServerStreamWriter<CountryCreationReply> responseStream, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, ""));
        }
    }
}
