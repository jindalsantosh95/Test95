using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Enums;
using VAA.EAPI.GraphQL.Domain.Search.Models.CarHire;
using VAA.EAPI.GraphQL.Domain.Search.Models.Holiday;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CarHireQuery
    {
        [AllowAnonymous]
        [UseFiltering]
        [UseSorting]
        [GraphQLName("searchCarHire")]
        public async Task<CarHireSearchResponse> GetCarHire(string pickUpLocation, string dropOffLocation, DateTime departureDate, int duration, string pickUpTime, string dropOfftime, [Service] IOAuthTokenProvider tokenProvider, [Service] IHttpConnector connector, [Service] ILogger<HolidayCJS> logger)
        {
            if (tokenProvider is null)
                throw new ArgumentNullException(nameof(tokenProvider));

            if (connector is null)
                throw new ArgumentNullException(nameof(connector));


            CarHireSearchRequest sr = new()
            {
                BookingType = BookingTypeEnum.Car.ToString(),
                PickUpDepot = pickUpLocation,
                DropOffDepot = dropOffLocation,
                DepartureDate = departureDate.ToString("dd-MM-yyyy"),
                Duration = duration.ToString(),
                PickUpTime = pickUpTime,
                DropOffTime = dropOfftime
            };


            try
            {
                string body = Common.Serialisation.Json.Serialize(sr, false);
                string holidySearchUrl = ConfigReader.Get(Common.Constants.Environment.HolidaySearchUrl);
                var response = await connector.SendAsync<CarHireSearchResponse>(holidySearchUrl, new Dictionary<string, string>(), string.Empty, HttpMethod.Post, body);

                return response.Response;
            }
            catch (HttpRequestException ex)
            {
                var content = await ex.ReadResponseContentAsStringAsync();

                logger.LogError(ex, $"Search execution failed \r\n{content}");

                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Unahandled Exception in Search \r\n{ex.Message}");
                throw;
            }

        }
    }
}
