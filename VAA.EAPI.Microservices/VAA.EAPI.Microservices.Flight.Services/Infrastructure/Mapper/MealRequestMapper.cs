using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;
using SpecialService = VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.SpecialService;
using PnrCommon = VAA.EAPI.Microservices.Flight.Services.Infrastructure.Proxy.PNRCommon;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Mapper
{
    public class MealRequestMapper
    {
        public static SpecialService.AddRequestType AddMealRequest(FlightSegment segment, string mealCode, List<Domain.PopsMeal.Passenger> passengers, string session)
        {
            return new SpecialService.AddRequestType
            {
                RequestInfo = new SpecialService.RequestInfoType
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    SessionId = session,
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                },
                SpecialServiceRequestInfo = new SpecialService.SpecialServiceRequestInfoType
                {
                    RequestCode = mealCode,
                    AirlineCode = Constants.Application.AirlineCode,
                    StatusCode = "HK",
                    SegmentNumberInfo =
                    [
                        new SpecialService.SegmentNumberInfoType { SegmentNumber = segment.SegmentId }
                    ],
                    NameNumberInfo = passengers.Select(p =>
                    {
                        var idParts = p.PassengerId?.Split('.');
                        return new SpecialService.NameNumberInfoType
                        {
                            FirstNameNumber = idParts[0],
                            LastNameNumber = idParts[1]
                        };
                    }).ToArray()
                }
            };
        }

        public static PnrCommon.SetupContextRequestType SetupContextMapper(string pnr)
        {
            return new PnrCommon.SetupContextRequestType()
            {
                RequestInfo = new PnrCommon.RequestInfoType()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),

                },
                RetrieveInfo = new PnrCommon.RetrieveInfoType()
                {
                    PassengerInfo = new PnrCommon.PassengerInfoType { RecordLocator = pnr }
                }
            };
        }

        public static PnrCommon.TeardownContextRequestType TearDownMapper(string sessionId)
        {
            return new()
            {
                RequestInfo = new()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    SessionId = sessionId,
                },
                EndRequest = new() { RedisplayFlag = "N" }

            };
        }

        public static SpecialService.DeleteRequestType DeleteMealRequest(IEnumerable<FlightSegment> segments, string session)
        {
            return new SpecialService.DeleteRequestType
            {
                RequestInfo = new SpecialService.RequestInfoType
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    SessionId = session,
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                },
                SpecialServiceRequestInfo = segments.SelectMany(segment => segment.Passengers ?? Enumerable.Empty<Passenger>(),
                    (segment, pax) => new SpecialService.SpecialServiceRequestInfoType
                    {
                        AirlineCode = Constants.Application.AirlineCode,
                        RecordNumber = pax.Meals.First().SsrId,
                        SegmentNumberInfo = [new SpecialService.SegmentNumberInfoType { SegmentNumber = segment.SegmentId }],
                        NameNumberInfo = [
                             new SpecialService.NameNumberInfoType
                            {
                                FirstNameNumber = pax.PassengerId?.Split('.').First(),
                                LastNameNumber = pax.PassengerId?.Split('.').Last()
                            }]
                    }).ToArray()
            };
        }
    }
}