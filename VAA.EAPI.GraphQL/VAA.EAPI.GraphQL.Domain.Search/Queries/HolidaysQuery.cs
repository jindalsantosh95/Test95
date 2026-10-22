using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Enums;
using VAA.EAPI.GraphQL.Domain.Search.Models.Holiday;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class HolidaysQuery
    {
        [AllowAnonymous]
        [UseFiltering]
        [UseSorting]
        [GraphQLName("searchHolidays")]
        public async Task<HolidaySearchResponse> GetHolidays(string location, DateTime depDate, string gateway, int duration, int adults, [Service] IOAuthTokenProvider tokenProvider, [Service] IHttpConnector connector, [Service] ILogger<HolidayCJS> logger)
        {
            if (tokenProvider is null)
                throw new ArgumentNullException(nameof(tokenProvider));

            if (connector is null)
                throw new ArgumentNullException(nameof(connector));


            HolidaySearchRequest sr = new()
            {
                BookingType = BookingTypeEnum.Holiday.ToString(),
                Location = location,
                Gateway = gateway,
                DepartureDate = depDate.ToString("dd-MM-yyyy"),
                Duration = duration.ToString(),
                PartyCompositions = new List<PartyComposition>()
                {
                    new PartyComposition()
                    {
                        Adults = adults.ToString(),
                        ChildAges = new List<string>(),
                        Infants = "0"
                    }
                }
            };


            try
            {
                string body = Common.Serialisation.Json.Serialize(sr, true);
                string holidySearchUrl = ConfigReader.Get(Common.Constants.Environment.HolidaySearchUrl);
                var response = await connector.SendAsync<HolidaySearchResponse>(holidySearchUrl, new Dictionary<string, string>(), string.Empty, HttpMethod.Post, body);

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
