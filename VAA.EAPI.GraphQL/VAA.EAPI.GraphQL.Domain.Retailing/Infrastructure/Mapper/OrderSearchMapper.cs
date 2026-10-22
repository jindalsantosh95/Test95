using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Mapper
{
    public static class OrderSearchMapper
    {
        public static PNR PnrMapper(OrderSearchResponse response)
        {
            // Find all Meal Order instructions 
            var mlInstructions = response.OrderInstructions?
                .Where(oi => oi.ItemCode != null &&
                             oi.ItemCode.EndsWith("ML", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(oi => oi.InstructionId, oi => oi.ItemCode)
                ?? new Dictionary<string, string>();

            return new PNR
            {

                BookingDate = Convert.ToDateTime(response.BookingReferences.RLOCCreateDate),
                RecordLocator = response.BookingReferences.RLOC,
                Segments = response.SegmentDetails.Select(x => new Segment
                {
                    SegmentID = x.SegmentId,
                    MarketingFlight = new FlightNumber
                    {
                        AirlineCode = x.MarketingCarrier,
                        Number = Convert.ToInt32(x.MarketingFlightNumber)
                    },
                    OperatingFlight = new FlightNumber
                    {
                        AirlineCode = x.OperatingCarrier ?? x.MarketingCarrier,
                        Number = (x.OperatingFlightNumber ?? x.MarketingFlightNumber).Parse(0)
                    },
                    Origin = x.OriginAirportCode,
                    Destination = x.DestinationAirportCode,
                    Departure = Convert.ToDateTime(x.ScheduledDepartureTime.LocalDateTime),
                    Cabin = x.ClassOfService,
                }),
                Passengers = response.CustomerDetails.Select(x => new Passenger
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    PassengerId = string.Concat(x.FirstNameNumber, ".", x.LastNameNumber),
                    FrequentFlyerNumber = x.LoyaltyMemberDetails?.MemberNumber,
                    SelectedMeal = (mlInstructions?.Any() == true && response.OrderItems?.Any() == true) ? GetMealItemCodeForBeneficiary(x.CustId, mlInstructions, response.OrderItems) : null
                })

            };
        }


        private static string GetMealItemCodeForBeneficiary(string beneficiaryId, Dictionary<string, string> mlInstructions, IEnumerable<OrderItems> orderItems)
        {
            var mlItemCode =
                (from item in orderItems
                 from svc in item.Services ?? Enumerable.Empty<Service>()
                 where svc.ServiceBeneficiaries != null &&
                       svc.ServiceBeneficiaries.Contains(beneficiaryId)
                 from instrRef in svc.OrderInstructionRefs ?? Enumerable.Empty<string>()
                 where mlInstructions.ContainsKey(instrRef)
                 select mlInstructions[instrRef])
                .FirstOrDefault();

            return mlItemCode;
        }

        public static object RequestMapper(string bookingRef, string firstName, string lastName)
        {
            return new
            {
                SearchFilterCriteria = new
                {
                    bookingReferenceCriteria = new
                    {
                        BookingReference = new
                        {
                            RLOC = bookingRef,
                            AirlineDesignator = "",
                            RLOCCreateDate = DateTime.MinValue,
                            ExternalRecordLocator = ""
                        },
                        customerName = new
                        {
                            GivenName = firstName,
                            SurName = lastName
                        }
                    },
                    accountableDocumentFilterCriteria = new { },
                    OrderReferenceCriteria = new { }
                }
            };
        }

    }
}
