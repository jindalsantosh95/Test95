using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Delta.FlexiDateSearch
{
    public class FlexiDateSearchQuery : IRequest<Result<Response<FlexiDateSearchResponse>>>
    {
        public OfferSearchCriteria Request { get; set; }
        public bool AwardSearch { get; set; }

        public FlexiDateSearchQuery(FlexiDateOfferRequest request)
        {
            AwardSearch = request.FlightSearchRequest.AwardSearch;
            Request = new OfferSearchCriteria()
            {
                ProductGroups = new ProductCategory[]
                {
                    new ProductCategory { ProductCategoryCode = ProductCategory.ProductCategoryCodeEnum.FLIGHTS },
                },
                OffersCriteria = new OffersCriteria
                {
                    PricingCriteria = new PricingCriteria { PriceableIn = new List<string> { request.FlightSearchRequest.AwardSearch ? Constants.Application.Miles : Constants.Application.Currency }, CurrencyCode = request.CurrencyCode },
                    Preferences = new(),
                    Qualifiers = request.FlightSearchRequest.PromoCode.IsNullOrEmpty() ? null : new Qualifiers
                    {
                        DiscountCertificate = new List<DiscountCertificate>
                        {
                            new DiscountCertificate
                            {
                                CertificateTypeAccountCode = request.FlightSearchRequest.PromoCode.ToUpper()
                            }
                        }
                    },
                    FlightRequestCriteria = new FlightOfferCriteria
                    {
                        CurrentTripIndexId = request.FlightSearchRequest.CurrentTripIndexId,
                        SortByBrandId = request.FlightSearchRequest.BrandId ?? Constants.Application.VS_ECON_LIGHT,
                        SearchOriginDestination = (from Ond in request.FlightSearchRequest.SearchOriginDestinations
                                                   select new SearchOriginDestination
                                                   {
                                                       Destinations = new Airport[] { new Airport { AirportCode = Ond.Destination } },
                                                       Origins = new Airport[] { new Airport { AirportCode = Ond.Origin } },
                                                       ConnectionAirportCode = Ond.ConnectionAirports,
                                                       DepartureLocalTs = DateTime.Parse(Ond.DepartureDate, System.Globalization.CultureInfo.InvariantCulture).ToString(Constants.Application.DateTimeFormat),
                                                       CalenderDateRequest = new CalendarDateCriteria { DaysAfterCnt = Constants.Application.FlexiDays, DaysBeforeCnt = Constants.Application.FlexiDays },
                                                   }),
                    }
                },
                Sender = (Party)request.LoyaltyMember,
                Customers = (from cus in request.CustomerDetails
                             select new Passenger
                             {
                                 CustomerId = cus.CustId,
                                 PassengerTypeCode = cus.Ptc,
                                 PassengerId = cus.CustId
                             }).ToList()
            };
        }
    }
}