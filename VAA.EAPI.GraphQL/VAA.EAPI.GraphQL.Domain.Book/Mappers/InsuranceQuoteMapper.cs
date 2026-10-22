using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Constants;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;


namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class InsuranceQuoteMapper
    {
        public static InsuranceQuoteRequest InsuranceQuoteRequestMapper(OfferResponse offerResponse, Basket basket, IEnumerable<AirportRefData> airportData, IEnumerable<CabinRefData> cabinData = null, int quoteVersionId = 0)
        {
            var sortedSegments = offerResponse.DataList.FlightSegments.OrderBy(x => x.ScheduledDepartureTime.LocalDateTime);
            var outboundOdRefId = sortedSegments.First().OdRefId;

            return new InsuranceQuoteRequest()
            {
                CustomerAddress = new() { Country = offerResponse.Pos.CountryCode },
                Customers = basket.People.Select(p => new Customer()
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    CustomerId = p.Id.GetHashCode().ToString(),
                    CustomerType = GetInsuredType(p)
                }),
                Gadget = GetGadget(),
                RequestDetail = new()
                {
                    BusinessUnit = offerResponse.Pos.CountryCode,
                    Currency = offerResponse.Offers.First().SalePrice.Where(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).First().Amount.BaseCurrency,
                    OfferVersionId = quoteVersionId,
                    Language = Application.AnzLanguage,
                    RequestCategories = new[] { Application.AnzRequestCategory },
                    SessionId = basket.BasketId.ToString(),
                    PaymentMethod = Application.AnzPaymentMethod,
                    RedemptionPoints = basket.CustomerProfile?.MembershipDetails?.FQTPoints,
                    RequestOriginPage = Application.AnzRequestOriginPage,
                    RequestOrigin = Application.AnzRequestOrigin,
                },
                Travel = new()
                {
                    ClassOfService = offerResponse.Offers.First().OfferItems.First().Services.FirstOrDefault()?.FareDetails?.FareSegments?.First().BookingClass,
                    IsDomestic = false,
                    Origin = new()
                    {
                        Code = sortedSegments.FirstOrDefault(s => s.OdRefId == outboundOdRefId).OriginAirportCode,
                        City = sortedSegments.FirstOrDefault(s => s.OdRefId == outboundOdRefId).OriginCityCode,
                        Country = airportData.FirstOrDefault(a => a.AirportCode == sortedSegments.FirstOrDefault(s => s.OdRefId == outboundOdRefId)?.OriginAirportCode)?.CountryCode
                    },
                    Refundable = "false",
                    IncludesConnection = sortedSegments.GroupBy(s => s.OdRefId).Any(g => g.Count() > 1),
                    MembershipStatus = basket.CustomerProfile?.MembershipDetails?.FQTNumber.IsNullOrEmpty() == false ? "yes" : "no",
                    Destination = new()
                    {
                        Code = sortedSegments.LastOrDefault(s => s.OdRefId == outboundOdRefId).DestinationAirportCode,
                        City = sortedSegments.LastOrDefault(s => s.OdRefId == outboundOdRefId).DestinationCityCode,
                        Country = airportData.FirstOrDefault(a => a.AirportCode == sortedSegments.LastOrDefault(s => s.OdRefId == outboundOdRefId)?.DestinationAirportCode)?.CountryCode
                    },
                    DepartureDate = sortedSegments.FirstOrDefault(s => s.OdRefId == outboundOdRefId)?.ScheduledDepartureTime.LocalDateTime,
                    ReturnDate = sortedSegments.FirstOrDefault(s => s.OdRefId != outboundOdRefId)?.ScheduledDepartureTime.LocalDateTime,
                    TotalCost = Convert.ToDouble(offerResponse.Offers.First().SalePrice.Where(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).First().Amount.MonetaryAmount),
                    TravelSegments = AddTravelSegments(offerResponse, airportData),
                    TripIndicator = sortedSegments.Select(s => s.OdRefId).Distinct().Count() > 1 ? Application.AnzTripIndicatorRoundTrip : Constants.Application.AnzTripIndicatorOneWay

                }
            };
        }

        public static Gadget GetGadget()
        {
            var user_agent = AsyncContext.Current.Context.GetHeader(Application.UserAgent);
            var headerValues = AsyncContext.Current.Context.GetHeader(Application.XForwardedFor);
            var ipAddresses = headerValues?.Split(",");

            return new Gadget()
            {
                IpAddress = ipAddresses?.Length > 0 ? ipAddresses[0] : headerValues, //derive from header
                DeviceCategory = "Desktop", //derive from user-agent,
                RequestingPlatform = user_agent.ToLower().Contains("mobile") ? "Mobile Site" : "None" //get from user-agent
            };
        }

        public static string GetInsuredType(BasketPerson basketPerson)
        {
            return basketPerson.Type switch
            {
                "ADULT" => basketPerson.Id == "0" ? AllianzCustomerType.PRIMARY.ToString() : AllianzCustomerType.OTHER.ToString(),
                "CHILD" => AllianzCustomerType.CHILD.ToString(),
                "INFANT" => AllianzCustomerType.INFANT.ToString(),
                "TEEN" => AllianzCustomerType.CHILD.ToString(),         //TEEN in NBF is CHILD in ANZ
                _ => AllianzCustomerType.OTHER.ToString()
            };
        }

        public static IEnumerable<TravelSegment> AddTravelSegments(OfferResponse offerResponse, IEnumerable<AirportRefData> airportData)
        {
            var sortedElements = offerResponse.DataList.FlightSegments.OrderBy(x => x.ScheduledDepartureTime.LocalDateTime);
            IEnumerable<TravelSegment> result = new List<TravelSegment>();

            foreach (var odRefId in sortedElements.Select(o => o.OdRefId).Distinct())
            {
                var segment = new TravelSegment()
                {
                    Origin = new()
                    {
                        Code = sortedElements.FirstOrDefault(s => s.OdRefId == odRefId).OriginAirportCode,
                        Country = sortedElements.FirstOrDefault(s => s.OdRefId == odRefId).OriginCountryCode ?? airportData.FirstOrDefault(a => a.AirportCode == sortedElements.FirstOrDefault(s => s.OdRefId == odRefId).OriginAirportCode).CountryCode,
                    },
                    Destination = new()
                    {
                        Code = sortedElements.LastOrDefault(s => s.OdRefId == odRefId).DestinationAirportCode,
                        Country = sortedElements.LastOrDefault(s => s.OdRefId == odRefId).OriginCountryCode ?? airportData.FirstOrDefault(a => a.AirportCode == sortedElements.LastOrDefault(s => s.OdRefId == odRefId).DestinationAirportCode).CountryCode,
                    },

                    RouteGroup = odRefId,
                    TravelLegs = sortedElements.Where(s => s.OdRefId == odRefId).Select((l, i) => new TravelLeg()
                    {
                        RouteNumber = l.SegmentId,
                        ClassOfService = offerResponse.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(l.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == l.SegmentId).BookingClass,
                        Sequence = i,
                        FareClass = offerResponse.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(l.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == l.SegmentId).CabinName,
                        Origin = new Origin()
                        {
                            Code = l.OriginAirportCode,
                            Country = airportData.FirstOrDefault(a => a.AirportCode == l.OriginAirportCode).CountryCode,
                        },
                        Destination = new Destination
                        {
                            Code = l.DestinationAirportCode,
                            Country = airportData.FirstOrDefault(a => a.AirportCode == l.DestinationAirportCode).CountryCode,
                        }
                    })
                };
                result = result.Append(segment);
            }
            return result;
        }

        public static IEnumerable<QuoteResponse> InsuranceQuoteResponseMapper(InsuranceQuoteResponse insuranceQuoteResponse, string pos)
        {
            return new[]
            {
                new QuoteResponse()
                {
                    Status = new()
                    {
                        Code = insuranceQuoteResponse.Status.Code,
                        ErrorText = insuranceQuoteResponse.Status.ErrorText,
                        Message = insuranceQuoteResponse.Status.Message,
                        Version = insuranceQuoteResponse.Status.Version,
                        ProcessId = insuranceQuoteResponse.Status.ProcessId,
                        Reason = insuranceQuoteResponse.Status.Reason
                    },
                    TransactionDetails = new()
                    {
                        ControlType = insuranceQuoteResponse.TransactionDetails.ControlType,
                        Cxid = insuranceQuoteResponse.TransactionDetails.Cxid,
                        PopulationCode = insuranceQuoteResponse.TransactionDetails.PopulationCode,
                        ResponseId = insuranceQuoteResponse.TransactionDetails.ResponseId,
                        SegmentCode = insuranceQuoteResponse.TransactionDetails.SegmentCode,
                        SegmentName = insuranceQuoteResponse.TransactionDetails.SegmentName,
                        TreatmentId = insuranceQuoteResponse.TransactionDetails.TreatmentId,
                        TreeCode = insuranceQuoteResponse.TransactionDetails.TreeCode
                    },
                    Products = insuranceQuoteResponse.Status.Code.ToUpper() == "SUCCESS" ? insuranceQuoteResponse.Products?.Select(p => new Models.Insurance.Product()
                    {
                        ExternalCode = p.ExternalCode,
                        IsAvailable = p.IsAvailable,
                        OfferId = p.OfferId,
                        PricingModelCode = p.PricingModelCode,
                        PricingModelDescription = p.PricingModelDescription,
                        PricingModelMinAmount = p.PricingModelMinAmount,
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        QuoteId = p.QuoteId,
                        ProductType = p.ProductType,
                        TotalPrice = p.TotalPrice,
                        SupplierCode = p.SupplierCode,
                        ProductSpecificMessages = p.ProductSpecificMessages?.Select(m => new Models.Insurance.ProductSpecificMessage()
                        {
                            Text = m.Text,
                            Type = m.Type,
                            Uri = m.Uri
                        })
                    }) : null,
                    Offers =  insuranceQuoteResponse.Status.Code.ToUpper() == "SUCCESS" ?  insuranceQuoteResponse.Locations?.FirstOrDefault()?.Offers?.Where(of => of.ContentType != "Template").Select(o => new Models.Insurance.Offer()
                    {
                        Sequence = o.Sequence,
                        ContentCode = o.ContentCode,
                        ContentType = o.ContentType,
                        HtmlOutput = insuranceQuoteResponse.Locations?.FirstOrDefault()?.Output,
                        Id = o.Id,
                        QuoteId = insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.QuoteId,
                        PriceDetails = pos == "GB" ? new Price()
                        {
                           Amount =  (double)insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.Prices?.FirstOrDefault()?.Price,
                           IsAvailable = (bool)insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.Prices?.FirstOrDefault()?.IsAvailable,
                           IsIncluded = (bool)insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.Prices?.FirstOrDefault()?.IsIncluded
                        } : new Price()
                        {
                           Amount =  (double)insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.Prices?.Sum(p => p.Price)
                        },
                        ProductCode = insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.ProductCode,
                        Messages =  o.Messages?.Select(m => new Models.Insurance.Message()
                        {
                            CompiledMessage = m.CompiledMessage,
                            MessageSequence = m.MessageSequence,
                            MessageTag = m.MessageTag
                        }),
                        ProductSpecificMessages =  insuranceQuoteResponse.Products?.FirstOrDefault(p => p.OfferId == o.Id)?.ProductSpecificMessages?.Select(m => new Models.Insurance.ProductSpecificMessage()
                        {
                            Text = m.Text,
                            Type = m.Type,
                            Uri = m.Uri
                        })
                    })
                    : null
                }
            }.ToList();
        }
    }
}
