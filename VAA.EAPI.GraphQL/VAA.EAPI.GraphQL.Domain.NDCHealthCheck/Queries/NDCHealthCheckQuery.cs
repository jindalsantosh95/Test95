using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Mapper;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class NDCHealthCheckQuery
    {
        private readonly IHeartBeatService _heartBeatService;
        private readonly IHealthReportService _healthReportService;

        public NDCHealthCheckQuery(IHeartBeatService heartBeatService, IHealthReportService healthReportService) 
        { 
            _heartBeatService = heartBeatService ?? throw new ArgumentNullException(nameof(heartBeatService));
            _healthReportService = healthReportService ?? throw new ArgumentNullException(nameof(healthReportService));
        }

        [UseFiltering]
        //[Authorize(Roles = new[] { "NDC.HealthCheck.Read" })]
        [GraphQLName("healthReport")]
        [GraphQLDescription("Retrieves health report of NDC APIs")]
        public async Task<IEnumerable<HealthCheckResponse>> GetHealthReport(List<string> apiList)
        {
            var offerHeartBeatStatus = new HeartBeatResponse();
            var orderHeartBeatStatus = new HeartBeatResponse();
            var paymentFacadeHeartBeatStatus = new HeartBeatResponse();

            if (apiList.Any(x => Enum.GetNames(typeof(OfferAPIs)).Contains(x))) { 
                offerHeartBeatStatus = await _heartBeatService.GetOfferHeartBeatStatus();}

            if (apiList.Any(x => Enum.GetNames(typeof(OrderAPIs)).Contains(x)))
            {
                orderHeartBeatStatus = await _heartBeatService.GetOrderHeartBeatStatus();
                paymentFacadeHeartBeatStatus = await _heartBeatService.GetPaymentFacadeHeartBeatStatus();
            }

            var realTimeHealthReport = await _healthReportService.GetHealthReportStatus(new HealthReportRequest() { ApiList = apiList});

            var response = HeartBeatResponseMapper.MapResponse(apiList, offerHeartBeatStatus, orderHeartBeatStatus, realTimeHealthReport, paymentFacadeHeartBeatStatus);

            return response;
        }
    }
}
