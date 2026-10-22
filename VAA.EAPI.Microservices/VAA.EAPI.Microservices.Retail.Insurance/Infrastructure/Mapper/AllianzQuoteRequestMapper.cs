using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mapper
{
    public static class AllianzQuoteRequestMapper
    {
        public static QuoteRequest MapQuoteRequest(InsuranceQuoteRequest insuranceQuoteRequest)
        {
            return new QuoteRequest()
            {
                Credentials = new() { Username = ConfigReader.Try(Constants.Environment.AllianzQuoteCreateUsername), Password = ConfigReader.Try(Constants.Environment.AllianzQuoteCreatePassword) },

                Gadget = new()
                {
                    DeviceCategory = insuranceQuoteRequest.Gadget.DeviceCategory,
                    DeviceType = insuranceQuoteRequest.Gadget.DeviceType,
                    IpAddress = insuranceQuoteRequest.Gadget.IpAddress,
                    RequestingPlatform = insuranceQuoteRequest.Gadget.RequestingPlatform
                },
                Customers = insuranceQuoteRequest.Customers.Select((c, index) => new Proxy.AllianzQuoteRequest.Customer()
                {
                    CustomerId = c.CustomerId,
                    Address = new Address()
                    {
                        Country = insuranceQuoteRequest.CustomerAddress.Country,
                        Region = insuranceQuoteRequest.CustomerAddress.Region,
                        PostalCode = insuranceQuoteRequest.CustomerAddress.PostalCode
                    },
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    CustomerType = c.CustomerType,
                    Sequence = index,
                    Age = c.Age
                }).ToArray(),
                Partner = new()
                {
                    ExternalCode = ConfigReader.Try(Constants.Environment.AllianzPartnerId)
                },
                RequestDetail = new()
                {
                    BusinessUnit = insuranceQuoteRequest.RequestDetail.BusinessUnit,
                    Currency = insuranceQuoteRequest.RequestDetail.Currency,
                    Language = insuranceQuoteRequest.RequestDetail.Language,
                    OfferVersionId = insuranceQuoteRequest.RequestDetail.OfferVersionId == 0 ? ConfigReader.Try<int>(Constants.Environment.AnzOfferVersionId) : insuranceQuoteRequest.RequestDetail.OfferVersionId,
                    PaymentMethod = insuranceQuoteRequest.RequestDetail.PaymentMethod,
                    RedemptionPoints = insuranceQuoteRequest.RequestDetail.RedemptionPoints,
                    RequestCategories = insuranceQuoteRequest.RequestDetail.RequestCategories.ToArray(),
                    RequestChannel = Constants.Application.AlliazRequestChannel,
                    RequestedDate = DateTime.Now,
                    RequestOrigin = insuranceQuoteRequest.RequestDetail.RequestOrigin,
                    SessionId = insuranceQuoteRequest.RequestDetail.SessionId,
                    TransactionId = AsyncContext.Current.CorrelationId.ToString(),
                },
                Travel = new()
                {
                    ClassOfService = insuranceQuoteRequest.Travel.ClassOfService,
                    BrandedFare = insuranceQuoteRequest.Travel.BrandedFare,
                    DepartureDate = Convert.ToDateTime(insuranceQuoteRequest.Travel.DepartureDate),
                    IncludesConnection = insuranceQuoteRequest.Travel.IncludesConnection,
                    Destination = new OriginDestinationDetails()
                    {
                        City = insuranceQuoteRequest.Travel.Destination.City,
                        Code = insuranceQuoteRequest.Travel.Destination.Code,
                        Region = insuranceQuoteRequest.Travel.Destination.Region,
                        Country = insuranceQuoteRequest.Travel.Destination.Country
                    },
                    IsDomestic = insuranceQuoteRequest.Travel.IsDomestic,
                    Origin = new OriginDestinationDetails()
                    {
                        City = insuranceQuoteRequest.Travel.Origin.City,
                        Code = insuranceQuoteRequest.Travel.Origin.Code,
                        Region = insuranceQuoteRequest.Travel.Origin.Region,
                        Country = insuranceQuoteRequest.Travel.Origin.Country
                    },
                    TravelSegments = insuranceQuoteRequest.Travel.TravelSegments.Select(t => new Travelsegment()
                    {
                        Destination = new OriginDestinationDetails() { Code = t.Destination.Code, Country = t.Destination.Country },
                        Origin = new OriginDestinationDetails() { Code = t.Origin.Code, Country = t.Origin.Country },
                        RouteGroup = t.RouteGroup,
                        Sequence = t.Sequence,
                        TravelLegs = t.TravelLegs.Select(l => new Travelleg()
                        {
                            ClassOfService = l.ClassOfService,
                            Sequence = l.Sequence,
                            FareClass = l.FareClass,
                            Origin = new OriginDestinationDetails() { Code = l.Origin.Code, Country = l.Origin.Country },
                            Destination = new OriginDestinationDetails() { Code = l.Destination.Code, Country = l.Destination.Country  },
                            RouteNumber = l.RouteNumber
                        }).ToArray()
                    }).ToArray(),
                    MembershipStatus = insuranceQuoteRequest.Travel.MembershipStatus,
                    Refundable = insuranceQuoteRequest.Travel.Refundable,
                    ReturnDate = insuranceQuoteRequest.Travel.ReturnDate != null ? Convert.ToDateTime(insuranceQuoteRequest.Travel.ReturnDate) : Convert.ToDateTime(insuranceQuoteRequest.Travel.DepartureDate),
                    TotalCost = insuranceQuoteRequest.Travel.TotalCost,
                    TravelAgent = Constants.Application.AlliazTravelAgent,
                    TripIndicator = insuranceQuoteRequest.Travel.TripIndicator
                }
            };           
        }


        
    }
}