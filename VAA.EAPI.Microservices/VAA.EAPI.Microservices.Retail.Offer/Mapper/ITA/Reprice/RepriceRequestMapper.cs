using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice
{
    public class RepriceRequestMapper
    {
        public static search RepriceSearchReqMapper(RepriceRequest repriceRequest, string fareFamilyName, List<Cabins> cabins, string shoppingResponseId)
        {
            search reshopSearchReq = new()
            {
                api = ConfigReader.Get(Environment.ItaServiceApi),
                key = ConfigReader.Get(Environment.ItaServiceKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                name = searchName.solutionSellup,
                version = ConfigReader.Get(Environment.ItaServiceVersion),
                qpx = (searchQpx)System.Enum.Parse(typeof(searchQpx), ConfigReader.Get(Environment.ItaQpx)),
                qpxSpecified = true,
                session = shoppingResponseId.Split("|").FirstOrDefault(),
                summarizer = new[] { searchSummarizer.tripDetails },
                inputs = new searchInputs
                {
                    fareFamily = fareFamilyName,
                    solution = $"{shoppingResponseId.Split("|").LastOrDefault()}/{repriceRequest.PricedOffer.SelectedOffer.OfferRefID.Split("|").LastOrDefault()}",
                    paxType = GetSearchInputsPaxType(repriceRequest.CustomerDetails).ToArray(),
                    fareFamilyToPrice = repriceRequest.RepriceQueries.SearchCriteria.FareFamilies != null ? FareFamilyMapper.GetFareFamiliesToPrice(repriceRequest.RepriceQueries.SearchCriteria.FareFamilies).ToArray() : new List<searchInputsFareFamilyToPrice> { new() { name = cabins.FirstOrDefault(c => c.CabinName == repriceRequest.PricedOffer.SelectedOffer.OfferItems.FirstOrDefault().Product.ProductName).ItaCabinName } }.ToArray(),
                    salesCity = repriceRequest.Pos?.CityCode,
                    currency = repriceRequest.Pos?.CurrencyCode,
                    requirePrivateFares = repriceRequest?.Parties?.Participants?.FirstOrDefault().AccountCode != null ? true : false,
                    accountCode = new[] { repriceRequest?.Parties?.Participants?.FirstOrDefault().AccountCode },
                    waiveChangeFee = repriceRequest.IsReshopDisruption,
                    waiveChangeFeeSpecified = repriceRequest.IsReshopDisruption,
                }
            };
            return reshopSearchReq;
        }

        private static List<searchInputsPaxType> GetSearchInputsPaxType(List<Customer> customerDetails)
        {
            List<searchInputsPaxType> lstSearchInputsPaxType = new();
            foreach (var customer in customerDetails.DistinctBy(x => x.Ptc))
            {
                lstSearchInputsPaxType.Add(new searchInputsPaxType
                {
                    paxTypeCode = [(customer.ItaPtc ?? customer.Ptc).ToUpper()]
                });
            }

            return lstSearchInputsPaxType;
        }
    }
}
