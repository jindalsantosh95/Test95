using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestEnhancedAirBook : IEnhancedAirBook
    {
        public Task<EnhancedAirBookRQResponse> EnhancedAirBookAsync(EnhancedAirBookRQRequest request)
        {
            return Task.FromResult(new EnhancedAirBookRQResponse
            {
                EnhancedAirBookRS = new()
                {
                    ApplicationResults = new()
                    {
                        status = CompletionCodes.Complete
                    },
                    TravelItineraryReadRS = new()
                    {
                        TravelItinerary = new()
                        {
                            ItineraryInfo = new()
                            {
                                ReservationItems = [new() {
                                    FlightSegment = [new(){
                                        Status = "HK"
                                    }]
                                }]
                            }
                        }
                    }
                }
            });
        }
    }
}
