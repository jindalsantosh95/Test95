using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using Environment = VAA.EAPI.Microservices.Retail.Offer.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class FlexiDateItaReqMapper
    {
        public static search CalendarSearchReqMapper(OfferRequest offerRequest, bool isBundledSlnSummary)
        {
            List<searchInputsSlice> searchInputsSlices = offerRequest.FlightsearchRequest.SearchOriginDestinations.Select(sliceDtls => new searchInputsSlice
            {
                destination = sliceDtls?.Destination,
                origin = sliceDtls?.Origin,
                date = sliceDtls?.DepartureDate
            }).ToList();
            int sliceIndex = searchInputsSlices.FindIndex(slice => slice.date != null);

            return new()
            {
                api = ConfigReader.Get(Environment.ItaServiceApi),
                key = ConfigReader.Get(Environment.ItaServiceKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                name = searchName.flexibleDates,
                version = ConfigReader.Get(Environment.ItaServiceVersion),
                qpx = searchQpx.cloudprod,
                qpxSpecified = true,
                summarizer = new[] { isBundledSlnSummary ? searchSummarizer.flexibleDates : searchSummarizer.flexibleDates },
                inputs = new searchInputs
                {
                    sliceIndexSpecified = true,
                    paxType = offerRequest.CustomerDetails.Select(customerDetail => new searchInputsPaxType { paxTypeCode = new[] { customerDetail.Ptc }.ToArray() }).ToArray(),
                    slice = searchInputsSlices.ToArray(),
                    sliceIndex = sliceIndex,
                    carryOnAllowance = offerRequest.FlightsearchRequest.CarryOnBaggageAllowance,
                    baggageAllowance = offerRequest.FlightsearchRequest.CheckInBaggageAllowance,
                    flexDays = Constants.Application.FlexiDays,
                    salesCity = searchInputsSlices.First().origin,
                    sort = Constants.Application.CustomScore,
                    fareFamilyToPrice = offerRequest.FlightsearchRequest.FareFamilies.Select(farefamily => new searchInputsFareFamilyToPrice { name = farefamily }).ToArray()
                }
            };
        }
    }
}
