using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Models.Airship;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services
{
    public class AirshipService : IAirshipService
    {
        private readonly ILogger<AirshipService> _logger;
        private readonly IHttpConnector _apiConnector;

        public AirshipService(IHttpConnector apiConnector, ILogger<AirshipService> logger)
        {
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
     
        public async Task Handle(AirshipRequest request, CancellationToken cancellationToken)
        {
            var airshipRequest = Json.Serialize((NotifRequest)request);

            //Generate tokaen -- To be implemented
            var airshipToken = ConfigReader.Get(Constants.Environment.AirshipToken);                

            try
            {                    
                var notifResponse = await _apiConnector.SendAsync<NotifResponse>(ConfigReader.Get(Constants.Environment.AirshipApiUri), headers: null, bearerToken: airshipToken, verb: HttpMethod.Post, content: airshipRequest);
           
                if(!notifResponse.Ok)
                {
                    _logger.LogError(notifResponse.Error, $"Error in Airship API: {notifResponse.Details}");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error in AirshipService: {ex.Message}");
            }
        }
    }
}
