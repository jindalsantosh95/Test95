using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.ITA.ReshopForCancellation
{
    public class ReshopForCancellationRequestMapper
    {
        public static search ITAAwrReqMapper(ReshopRequest reshopRequest)
        {
            search reshopSearchReq = new()
            {
                api = ConfigReader.Get(Constants.Environment.ItaReshopApi),
                key = ConfigReader.Get(Constants.Environment.ItaReshopApiKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                name = searchName.refund,
                version = ConfigReader.Get(Constants.Environment.ItaReshopApiVersion),
                qpx = (searchQpx)Enum.Parse(typeof(searchQpx), ConfigReader.Get(Constants.Environment.ItaQpx)),
                qpxSpecified = true,
                summarizer = new[] { searchSummarizer.refund },
                inputs = new searchInputs
                {
                    lowestAvailable = true,
                    refundableOnly = false,
                    sort = Constants.Application.SortReshopApi,
                    waiveChangeFee = false,
                    unrestrictedOnly = false,
                    useAlternateWaiverRefund = false,
                    waiverAgentCode = reshopRequest.BookingReferences?.ExternalRecordLocator ?? string.Empty,
                    dynamicWaiversInterlineProcessing = false,
                    prohibitRiskFreeReissue = false,
                    pnrLocator = reshopRequest.BookingReferences?.RLOC ?? string.Empty,
                    paxType = GetSearchInputsPaxType(reshopRequest.CustomerDetails).ToArray(),
                    reissue = new searchInputsReissue
                    {
                        ticket = GetReissueTicket(reshopRequest).ToArray(),
                        validatingCarrier = reshopRequest.BookingReferences?.AirlineDesignator
                    },
                    salesCity = reshopRequest.Pos?.CityCode,
                    currency = reshopRequest.Pos?.CurrencyCode,
                }
            };
            return reshopSearchReq;
        }

        public static search ITARepriceReqMapper(ReshopRequest reshopRequest)
        {
            List<searchInputsSlice> searchInputsSlices = GetSearchSlice(reshopRequest.SegmentDetails);
            int sliceIndex = searchInputsSlices.FindIndex(slice => slice.date != null);
            search reshopSearchReq = new()
            {
                api = ConfigReader.Get(Constants.Environment.ItaReshopApi),
                key = ConfigReader.Get(Constants.Environment.ItaReshopApiKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                name = searchName.solutionSellup,
                version = ConfigReader.Get(Constants.Environment.ItaReshopApiVersion),
                qpx = (searchQpx)Enum.Parse(typeof(searchQpx), ConfigReader.Get(Constants.Environment.ItaQpx)),
                qpxSpecified = true,
                summarizer = new[] { searchSummarizer.fareChangeRestrictionInfo, searchSummarizer.fareRefundInfo },
                inputs = new searchInputs
                {
                    sliceIndexSpecified = true,
                    lowestAvailable = true,
                    refundableOnly = false,
                    sort = Constants.Application.SortReshopApi,
                    waiveChangeFee = false,
                    unrestrictedOnly = false,
                    pnrLocator = reshopRequest.BookingReferences?.RLOC ?? string.Empty,
                    paxType = GetSearchInputsPaxType(reshopRequest.CustomerDetails).ToArray(),
                    reissue = new searchInputsReissue
                    {
                        ticket = GetReissueTicket(reshopRequest).ToArray(),
                        validatingCarrier = reshopRequest.BookingReferences?.AirlineDesignator
                    },
                    slice = searchInputsSlices.ToArray(),
                    sliceIndex = sliceIndex,
                    salesCity = reshopRequest.Pos?.CityCode,
                    currency = reshopRequest.Pos?.CurrencyCode,
                }
            };
            return reshopSearchReq;
        }

        private static List<searchInputsSlice> GetSearchSlice(List<Segment> segmentDetail)
        {
            List<searchInputsSlice> searchInputsSlices = new();
            List<searchInputsSliceSegment> searchInputSliceSegs = new();

            searchInputSliceSegs.AddRange(segmentDetail.Select(segDtls => new searchInputsSliceSegment
            {
                bookingCode = segDtls.ClassOfService,
                departure = segDtls.ScheduledDepartureTime.LocalDateTime,
                destination = segDtls.DestinationAirportCode,
                flight = segDtls.MarketingCarrier + segDtls.MarketingFlightNumber.TrimStart('0'),
                origin = segDtls.OriginAirportCode
            }
            ));

            if (searchInputSliceSegs.Any())
            {
                searchInputsSlices.Add(new searchInputsSlice
                {
                    segment = searchInputSliceSegs.ToArray()
                });
            }

            return searchInputsSlices;
        }



        private static List<searchInputsPaxType> GetSearchInputsPaxType(List<Customer> customerDetails)
        {
            List<searchInputsPaxType> lstSearchInputsPaxType = new();
            foreach (var customerDetail in customerDetails)
            {
                lstSearchInputsPaxType.Add(new searchInputsPaxType
                {
                    paxTypeCode = new[] { customerDetail.Ptc.ToUpper() }.ToArray()
                });
            };

            return lstSearchInputsPaxType;
        }

        private static List<searchInputsReissueTicket> GetReissueTicket(ReshopRequest reshopRequest)
        {
            return reshopRequest.ReshopOrder.Select(reshopOrder => new searchInputsReissueTicket
            {
                awardPointsFareTotal = 0,
                cocFareTotal = GetFareTotal(reshopOrder.priceDetails),
                fareCalculation = reshopOrder.AccountableDocumentRefs.First().FareCalculation,
                issueDate = reshopOrder.AccountableDocumentRefs.First().IssueDate,
                saleFareTotal = GetSaleFareTotal(reshopOrder.priceDetails),
                ticketNumber = reshopOrder.AccountableDocumentRefs.First().DocumentNumber,
                coupon = GetReissueTicketCoupons(reshopRequest, reshopOrder).ToArray(),
                pax = new [] { GetReissueTicketPax(reshopRequest, reshopOrder.Services) },
                pointOfSale = new searchInputsReissueTicketPointOfSale { soldByTravelAgency = false },
                tax = GetReissueTicketTaxes(reshopOrder.priceDetails).ToArray()
            }).ToList();
        }

        private static string GetSaleFareTotal(List<PriceDetail> priceDetails)
        {
            if (priceDetails != null)
            {
                var custPrice = priceDetails.First(price => price.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare);
                return custPrice.Amount.BaseCurrency + custPrice.Amount.MonetaryAmount;
            }
            return string.Empty;
        }

        private static string GetFareTotal(List<PriceDetail> priceDetails)
        {
            if (priceDetails != null)
            {
                var custPrice = priceDetails.First(price => price.PriceComponentType == PriceDetail.PriceComponentTypes.BaseFare);
                return custPrice?.BaseCurrencyAmount?.Currency + custPrice?.BaseCurrencyAmount?.Amount;
            }
            return string.Empty;
        }

        private static List<searchInputsReissueTicketTax> GetReissueTicketTaxes(List<PriceDetail> priceDetails)
        {
            List<searchInputsReissueTicketTax> searchInputsReissueTicketTaxes = new();
            foreach (var priceDtl in priceDetails)
            {
                if (priceDtl?.PriceComponentType == PriceDetail.PriceComponentTypes.Tax && !string.IsNullOrEmpty(priceDtl.PriceComponentName))
                {
                    searchInputsReissueTicketTax taxes = new()
                    {
                        code = priceDtl?.PriceComponentName,
                        price = priceDtl?.Amount.BaseCurrency + priceDtl?.Amount.MonetaryAmount
                    };
                    searchInputsReissueTicketTaxes.Add(taxes);
                }
            };
            return searchInputsReissueTicketTaxes;
        }

        private static List<searchInputsReissueTicketCoupon> GetReissueTicketCoupons(ReshopRequest reshopRequest, ReshopOrder reshopOrder)
        {
            return (from segDtls in reshopRequest.SegmentDetails
                    select new searchInputsReissueTicketCoupon
                    {
                        bookingCode = segDtls.ClassOfService,
                        departure = segDtls.ScheduledDepartureTime.LocalDateTime,
                        designator = GetDesignator(reshopOrder, segDtls.SegmentNumber),
                        destinationCode = segDtls.DestinationAirportCode,
                        fare = GetCouponFare(reshopOrder, segDtls.SegmentNumber),
                        flight = segDtls.MarketingCarrier + segDtls.MarketingFlightNumber.TrimStart('0'),
                        originCode = segDtls.OriginAirportCode,
                        wasReissuedInvoluntarily = false,
                        used = reshopRequest.ReshopOrder.Any(item => item.AccountableDocumentRefs.Any(ad => ad.Coupons.Any(q => Constants.Application.Flownstatus.Contains(q.CouponStatus.ToUpper()) && q.segmentRefId == segDtls.SegmentId))),
                        usedSpecified = true
                    }).ToList();
        }

        private static string GetDesignator(ReshopOrder reshopOrder, string segNo)
        {
            var coupon = reshopOrder.AccountableDocumentRefs.SelectMany(a => a.Coupons).FirstOrDefault(c => c.segmentRefId == segNo);

            var desig = coupon?.FareBasis?.Split("/");
            return desig != null && desig.Length >= 2 ? desig.Last() : string.Empty;
        }

        private static string GetCouponFare(ReshopOrder reshopOrder, string segNo)
        {
            var coupon = reshopOrder.AccountableDocumentRefs.SelectMany(a => a.Coupons).FirstOrDefault(c => c.segmentRefId == segNo);

            return coupon?.FareBasis?.Split("/")?[0] ?? string.Empty;
        }

        private static string GetReissueTicketPax(ReshopRequest reshopRequest, List<Service> services)
        {
            string ptc = string.Empty;
            foreach (var service in services)
            {
                foreach (var custDtls in reshopRequest.CustomerDetails)
                {
                    if (service.ServiceBeneficiaries.Contains(custDtls.CustId))
                    {
                        ptc = custDtls.Ptc.ToUpper();
                    }
                };
            };
            return ptc;
        }
    }
}
