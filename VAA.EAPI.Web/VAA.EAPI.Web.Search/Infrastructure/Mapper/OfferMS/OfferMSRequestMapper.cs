using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Web.Search.Models.GraphQl;
using VAA.EAPI.Web.Search.Models.OfferMS;
using VAA.EAPI.Web.Search.Models.Search.Request;

namespace VAA.EAPI.Web.Search.Infrastructure.Mapper.OfferMS
{
    public class OfferMSRequestMapper
    {
        public static FlightOfferRequest RequestMapper(SearchRequestV2 request)
        {
            IEnumerable<SearchOriginDestination> originDest = request.slices.Select(slice =>
            new SearchOriginDestination()
            {
                DepartureDate = slice.departure.ToString("yyyy-MM-dd"),
                Destination = slice.destination,
                Origin = slice.origin,
                ConnectionAirports = slice.connections
            });

            int adt = 1, gbe = 1, cnn = 1, inf = 1;

            IEnumerable<Customer> customers = Enumerable.Empty<Customer>();
            foreach (var cust in request.passengers)
            {

                switch (cust)
                {
                    case Constants.Application.Adult:
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.ADT}",
                            CustId = $"{cust}_{adt++}"
                        });
                        break;

                    case Constants.Application.Teen:
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.GBE}",
                            CustId = $"{cust}_{gbe++}"
                        });
                        break;

                    case Constants.Application.Child:
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.CNN}",
                            CustId = $"{cust}_{cnn++}"
                        });
                        break;

                    case Constants.Application.Infant:
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.INF}",
                            CustId = $"{cust}_{inf++}"
                        });
                        break;
                }
            }


            return new()
            {
                FlightSearchRequest = new()
                {
                    SearchOriginDestinations = originDest,
                    CurrentTripIndexId = "0",
                    BrandId = request.brandId,
                    PromoCode = request.promoCode,
                    AwardSearch = request.awardSearch
                },
                CustomerDetails = customers
            };
        }
    }
}
