using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{

    public static class RewardSeatCalendarItaRequestMapper
    {
        public static search Map(RewardSeatCalendarRequest request, RewardSeatCalendarPolicy policy,
            IEnumerable<string> fareFamilyNames)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(fareFamilyNames);

            var families = fareFamilyNames.Take(4).ToArray();
            if (families.Length != 3 || families.Distinct(StringComparer.Ordinal).Count() != 3)
                throw new ArgumentException("Exactly three distinct reward fare families are required.", nameof(fareFamilyNames));
            var passengerTypes = policy.PassengerTypes?.ToArray()
                ?? throw new ArgumentException("Passenger types are required.", nameof(request));

            return new search
            {
                api = ConfigReader.Get(Constants.Environment.ItaServiceApi),
                key = ConfigReader.Get(Constants.Environment.ItaServiceKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                qpx = searchQpx.cloudprod,
                qpxSpecified = true,
                name = searchName.calendar,
                version = ConfigReader.Get(Constants.Environment.RewardSeatAvailabilityItaVersion),
                summarizer = [searchSummarizer.availSummary, searchSummarizer.sliceBySliceSummary],
                inputs = new searchInputs
                {
                    slice = [new searchInputsSlice
                    {
                        origin = request.Origin,
                        destination = request.Destination,
                        date = request.AnchorDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
                    }],
                    sliceIndex = 0,
                    sliceIndexSpecified = true,
                    paxType = passengerTypes.Select(type => new searchInputsPaxType { paxTypeCode = [type] }).ToArray(),
                    passengers = passengerTypes.Length,
                    permittedCarrier = [policy.PermittedCarrier],
                    fareFamilyToPrice = families.Select(name => new searchInputsFareFamilyToPrice { name = name }).ToArray(),
                    awardType = "CINCO",
                    awardRedeemerPTC = "ADT",
                    countryIssued = policy.CountryIssued,
                    countrySold = policy.CountrySold,
                    currency = policy.Currency,
                    salesCity = policy.SalesCity,
                    accountCode = null,
                    requirePrivateFares = false,
                    nonstopOnly = true,
                    allowCuba = true,
                    page = 1,
                    pageSize = 100
                }
            };
        }
    }
}
