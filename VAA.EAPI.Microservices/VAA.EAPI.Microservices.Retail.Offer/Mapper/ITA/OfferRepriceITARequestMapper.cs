using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common;
using Environment = VAA.EAPI.Microservices.Retail.Offer.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferRepriceITARequestMapper
    {
        public static search RepriceSearchReqMapper(RepriceRequest repriceRequest, string fareFamilyName, List<Cabins> cabins, string shoppingResponseId, List<Customer> offerDocCustomerDetails)
        {
            search reshopSearchReq = new()
            {
                api = ConfigReader.Get(Environment.ItaServiceApi),
                key = ConfigReader.Get(Environment.ItaServiceKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                qpx = searchQpx.cloudprod,
                name = searchName.solutionSellup,
                version = ConfigReader.Get(Environment.ItaServiceVersion),
                session = shoppingResponseId.Split("|").FirstOrDefault(),
                summarizer = new[] { searchSummarizer.tripDetails },
                inputs = new searchInputs
                {
                    fareFamily = fareFamilyName,
                    solution = $"{shoppingResponseId.Split("|").LastOrDefault()}/{repriceRequest.PricedOffer.SelectedOffer.OfferRefID.Split("|").LastOrDefault()}",
                    paxType = GetSearchInputsPaxType(offerDocCustomerDetails).ToArray(),
                    fareFamilyToPrice = repriceRequest.RepriceQueries.SearchCriteria.FareFamilies != null && repriceRequest.RepriceQueries.SearchCriteria.FareFamilies.Count != 0 ? FareFamilyMapper.GetFareFamiliesToPrice(repriceRequest.RepriceQueries.SearchCriteria.FareFamilies).ToArray() : new List<searchInputsFareFamilyToPrice> { new() { name = fareFamilyName } }.ToArray(),
                    salesCity = repriceRequest.Pos?.CityCode,
                    currency = repriceRequest.Pos?.CurrencyCode,
                    accountCode = [repriceRequest.Parties?.Participants?.FirstOrDefault().AccountCode],
                    requirePrivateFares = repriceRequest.Parties.Participants.FirstOrDefault().AccountCode != null ? true : false
                }
            };
            return reshopSearchReq;
        }

        private static List<searchInputsPaxType> GetSearchInputsPaxType(List<Customer> customerDetails)
        {
            List<searchInputsPaxType> lstSearchInputsPaxType = new();
            customerDetails.ForEach(customerDetail =>
            {
                lstSearchInputsPaxType.Add(new searchInputsPaxType
                {
                    paxTypeCode = new[] { customerDetail.Ptc = customerDetail?.ItaPtc is null ? customerDetail.Ptc : customerDetail?.ItaPtc }.ToArray()
                });
            });

            return lstSearchInputsPaxType;
        }
    }
}
