using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferITARequestMapper
    {
        public static search OfferSearchReqMapper(OfferRequest offerRequest, bool isBundledSlnSummary)
        {
            IEnumerable<searchInputsSlice> searchInputsSlices = GetSearchSlice(offerRequest.FlightsearchRequest.SearchOriginDestinations);
            int sliceIndex = searchInputsSlices.ToList().FindIndex(slice => slice.date != null);
            var accountCode = offerRequest.Parties.Participants.First().AccountCode;

            return new()
            {
                api = ConfigReader.Get(Constants.Environment.ItaServiceApi),
                key = ConfigReader.Get(Constants.Environment.ItaServiceKey),
                dataset = (searchDataset)Enum.Parse(typeof(searchDataset), ConfigReader.Try(Constants.Environment.ItaDataset, nameof(searchDataset.production))),
                datasetSpecified = true,
                qpx = searchQpx.cloudprod,
                name = searchName.bundledSolutionSearch,
                version = ConfigReader.Get(Constants.Environment.ItaServiceVersion),
                summarizer = new[] { isBundledSlnSummary ? searchSummarizer.bundledSolutionSummary : searchSummarizer.sliceBySliceSummary },
                inputs = new searchInputs
                {
                    isBundledSearch = true,
                    sliceIndexSpecified = true,
                    paxType = GetSearchInputsPaxType(offerRequest.CustomerDetails).ToArray(),
                    slice = searchInputsSlices.ToArray(),
                    sliceIndex = sliceIndex,
                    carryOnAllowance = offerRequest.FlightsearchRequest.CarryOnBaggageAllowance,
                    baggageAllowance = offerRequest.FlightsearchRequest.CheckInBaggageAllowance,
                    fareFamilyToPrice = offerRequest.FlightsearchRequest.FareFamilies?.Select(farefamily => new searchInputsFareFamilyToPrice { name = farefamily }).ToArray(),
                    accountCode = new[] { accountCode },
                    salesCity = offerRequest.Pos?.CityCode,
                    currency = offerRequest.Pos?.CurrencyCode,
                    requirePrivateFares = accountCode != null
                }
            };
        }

        private static IEnumerable<searchInputsPaxType> GetSearchInputsPaxType(List<Customer> customerDetails)
        {

            return customerDetails.Select(customerDetail => new searchInputsPaxType { paxTypeCode = new[] { customerDetail.ItaPtc = customerDetail?.ItaPtc is null ? customerDetail.Ptc:customerDetail?.ItaPtc }.ToArray() });
        }

        private static IEnumerable<searchInputsSlice> GetSearchSlice(List<SearchOriginDestination> originDestinationAssociations)
        {
            return originDestinationAssociations.Select(sliceDtls => new searchInputsSlice
            {
                destination = sliceDtls.Destination,
                origin = sliceDtls.Origin,
                date = sliceDtls.DepartureDate,
                time = sliceDtls?.DepartureTimeRange?.EarliestTime != null ? DateTime.Parse(sliceDtls?.DepartureTimeRange?.EarliestTime).ToString("htt").ToLower() : "anytime"
            });
        }
    }
}
