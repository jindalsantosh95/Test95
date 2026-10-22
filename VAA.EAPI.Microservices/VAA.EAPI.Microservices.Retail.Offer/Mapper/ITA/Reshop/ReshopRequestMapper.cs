using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop
{
    public class ReshopRequestMapper
    {
        public static search ReshopSearchReqMapper(ReshopRequest reshopRequest, bool isBundledSlnSummary)
        {
            List<searchInputsSlice> searchInputsSlices = GetSearchSlice(reshopRequest.OriginDestinationAssociations, reshopRequest.ReshopQueries, reshopRequest.SegmentDetails);
            int sliceIndex = searchInputsSlices.FindIndex(slice => slice.date != null);
            search reshopSearchReq = new()
            {
                api = ConfigReader.Get(Constants.Environment.ItaServiceApi),
                key = ConfigReader.Get(Constants.Environment.ItaServiceKey),
                dataset = searchDataset.production,
                datasetSpecified = true,
                name = searchName.sliceBySliceSearch,
                version = ConfigReader.Get(Constants.Environment.ItaServiceVersion),
                qpx = (searchQpx)Enum.Parse(typeof(searchQpx), ConfigReader.Get(Constants.Environment.ItaQpx)),
                qpxSpecified = true,
                summarizer = new[] { isBundledSlnSummary ? searchSummarizer.bundledSolutionSummary : searchSummarizer.sliceBySliceSummary },
                inputs = new searchInputs
                {
                    sliceIndexSpecified = true,
                    pnrLocator = reshopRequest.BookingReferences.RLOC ?? string.Empty,
                    paxType = GetSearchInputsPaxType(reshopRequest.CustomerDetails).ToArray(),
                    reissue = new searchInputsReissue
                    {
                        ticket = !reshopRequest.IsReshopHoldBooking ? GetReissueTicket(reshopRequest).ToArray() : GetReissueHoldBooking(reshopRequest).ToArray(),
                        validatingCarrier = reshopRequest.BookingReferences.AirlineDesignator
                    },
                    slice = searchInputsSlices.ToArray(),
                    sliceIndex = sliceIndex,
                    waiveChangeFee = reshopRequest.IsReshopDisruption,
                    waiveChangeFeeSpecified = reshopRequest.IsReshopDisruption,
                    carryOnAllowance = reshopRequest.ReshopQueries.FlightSpecific.SearchCriteria.CarryOnBaggageAllowance,
                    baggageAllowance = reshopRequest.ReshopQueries.FlightSpecific.SearchCriteria.CheckInBaggageAllowance,
                    fareFamilyToPrice = GetFareFamilies(reshopRequest).ToArray(),
                    accountCode = new[] { reshopRequest.Parties?.Participants?.FirstOrDefault().AccountCode },
                    salesCity = reshopRequest.Pos?.CityCode,
                    currency = reshopRequest.Pos?.CurrencyCode,
                    requirePrivateFares = reshopRequest.Parties?.Participants?.FirstOrDefault().AccountCode != null ? true : false

                }
            };
            return reshopSearchReq;
        }

        private static List<searchInputsReissueTicket> GetReissueHoldBooking(ReshopRequest reshopRequest)
        {
            return reshopRequest.ReshopOrder.Where(x => x.OrderProductType == Constants.Application.Flight).Select(reshopOrder => new searchInputsReissueTicket
            {
                awardPointsFareTotal = 0,
                cocFareTotal = GetFareTotal(reshopOrder.priceDetails),
                fareCalculation = reshopOrder.AccountableDocumentRefs?.FirstOrDefault()?.FareCalculation?.Replace(Constants.Application.FareCalcUnwantedChar, ""),
                issueDate = reshopOrder.AccountableDocumentRefs.First().IssueDate,
                saleFareTotal = GetSaleFareTotal(reshopOrder.priceDetails),
                ticketNumber = "",
                coupon = GetReissueTicketCoupons(reshopRequest, reshopOrder).ToArray(),
                pax = new[] { GetReissueTicketPax(reshopRequest, reshopOrder.Services) },
                pointOfSale = new searchInputsReissueTicketPointOfSale { soldByTravelAgency = false },
                tax = GetReissueTicketTaxes(reshopOrder.priceDetails).ToArray()
            }).ToList();
        }

        private static List<searchInputsPaxType> GetSearchInputsPaxType(List<Customer> customerDetails)
        {
            List<searchInputsPaxType> lstSearchInputsPaxType = [];

            foreach (var customer in customerDetails.DistinctBy(x => x.Ptc))
            {
                lstSearchInputsPaxType.Add(new searchInputsPaxType
                {
                    paxTypeCode = [(customer.ItaPtc ?? customer.Ptc).ToUpper()]
                });
            }
            return lstSearchInputsPaxType;
        }

        private static List<searchInputsReissueTicket> GetReissueTicket(ReshopRequest reshopRequest)
        {
            return reshopRequest.ReshopOrder.Where(x=>x.OrderProductType==Constants.Application.Flight).Select(reshopOrder => new searchInputsReissueTicket
            {
                awardPointsFareTotal = 0,
                cocFareTotal = GetFareTotal(reshopOrder.priceDetails),
                fareCalculation = reshopOrder.AccountableDocumentRefs.First().FareCalculation.Replace(Constants.Application.FareCalcUnwantedChar, ""),
                issueDate = reshopOrder.AccountableDocumentRefs.First().IssueDate,
                saleFareTotal = GetSaleFareTotal(reshopOrder.priceDetails),
                ticketNumber = reshopOrder.AccountableDocumentRefs.First().DocumentNumber,
                coupon = GetReissueTicketCoupons(reshopRequest, reshopOrder).ToArray(),
                pax = new[] { GetReissueTicketPax(reshopRequest, reshopOrder.Services) },
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

        private static IEnumerable<searchInputsReissueTicketTax> GetReissueTicketTaxes(List<PriceDetail> priceDetails)
        {
            var searchInputsReissueTicketTaxes = priceDetails.Where(x => x.PriceComponentType == PriceDetail.PriceComponentTypes.TotalTax && !string.IsNullOrEmpty(x.PriceComponentName)).Select(price => new searchInputsReissueTicketTax
            {
                code = price?.PriceComponentName,
                price = price?.Amount.BaseCurrency + price?.Amount.MonetaryAmount
            });

            return searchInputsReissueTicketTaxes;
        }

        private static IEnumerable<searchInputsReissueTicketCoupon> GetReissueTicketCoupons(ReshopRequest reshopRequest, ReshopOrder reshopOrder)
        {
            var isNotReshopHoldBooking = !reshopRequest.IsReshopHoldBooking;

            return (from segDtls in reshopRequest.SegmentDetails
                    select new searchInputsReissueTicketCoupon
                    {
                        bookingCode = segDtls.ClassOfService,
                        departure = segDtls.ScheduledDepartureTime.LocalDateTime,
                        designator = isNotReshopHoldBooking ? GetDesignator(reshopOrder, segDtls.SegmentId) : "",
                        destinationCode = segDtls.DestinationAirportCode,
                        fare = isNotReshopHoldBooking ? GetCouponFare(reshopOrder, segDtls.SegmentId) : GetCouponFareHoldBookingReshop(reshopOrder, segDtls.SegmentId),
                        flight = segDtls.MarketingCarrier + segDtls.MarketingFlightNumber.TrimStart('0'),
                        originCode = segDtls.OriginAirportCode,
                        wasReissuedInvoluntarily = false,
                        used = isNotReshopHoldBooking ? reshopRequest.ReshopOrder.Where(x => x.OrderProductType == Constants.Application.Flight).Any(item => item.AccountableDocumentRefs.Any(ad => ad.Coupons.Any(q => Constants.Application.Flownstatus.Contains(q.CouponStatus.ToUpper()) && q.segmentRefId == segDtls.SegmentId))) : false,
                        usedSpecified = true
                    });
        }

        private static string GetDesignator(ReshopOrder reshopOrder, string segNo)
        {
            return reshopOrder.AccountableDocumentRefs.SelectMany(a => a.Coupons).Where(c => c.segmentRefId == segNo).Select(c =>
            {
                var ticketDesignator = c.FareBasis?.Contains("/") == true ? c.FareBasis.Split('/')[1] : string.Empty;
                return ticketDesignator == "VV81" ? string.Empty : ticketDesignator;
            }).LastOrDefault() ?? string.Empty;
        }

        private static string GetCouponFare(ReshopOrder reshopOrder, string segNo)
        {
            return reshopOrder.AccountableDocumentRefs.SelectMany(a => a.Coupons).Where(c => c.segmentRefId == segNo).Select(c => string.IsNullOrWhiteSpace(c.FareBasis) ? string.Empty : c.FareBasis.Contains("/") ? c.FareBasis.Split('/')[0] : c.FareBasis) .LastOrDefault() ?? string.Empty;
        }

        private static string GetCouponFareHoldBookingReshop(ReshopOrder reshopOrder, string segNo)
        {
            return reshopOrder?.AccountableDocumentRefs.SelectMany(a => a.Coupons).Select(c => c.FareBasis).LastOrDefault() ?? string.Empty;
        }

        private static string GetReissueTicketPax(ReshopRequest reshopRequest, List<Service> services)
        {
            return services.SelectMany(service => reshopRequest.CustomerDetails.Where(custDtls => service.ServiceBeneficiaries.Contains(custDtls.CustId)).Select(cust => (cust.ItaPtc ?? cust.Ptc).ToUpper())).LastOrDefault() ?? string.Empty;
        }

        private static List<searchInputsSlice> GetSearchSlice(List<OriginDestinationAssociation> originDestinationAssociations, ReshopQueries ReshopQuery, List<Segment> segmentDetail)
        {
            List<searchInputsSlice> searchInputsSlices = new();
            List<searchInputsSliceSegment> searchInputSliceSegs = new();

            Dictionary<string, string> oDwithDates = new();
            if (ReshopQuery.FlightSpecific.FlightsRequest.Add != null && ReshopQuery.FlightSpecific.FlightsRequest.Add.Any())
            {
                foreach (var addQry in ReshopQuery.FlightSpecific.FlightsRequest.Add)
                {
                    if (!string.IsNullOrEmpty(addQry.OriginDestinationId) && !oDwithDates.ContainsKey(addQry.OriginDestinationId))
                        oDwithDates.Add(addQry.OriginDestinationId, originDestinationAssociations.FirstOrDefault(od => od.OdLabel == addQry.OriginDestinationId).OriginDestination.DepartureDate);
                }
            }
            if (ReshopQuery.FlightSpecific.FlightsRequest.Retain != null && ReshopQuery.FlightSpecific.FlightsRequest.Retain.Any())
            {
                foreach (var retainSeg in ReshopQuery.FlightSpecific.FlightsRequest.Retain)
                {
                    if (!string.IsNullOrEmpty(retainSeg.OriginDestinationId) && !oDwithDates.ContainsKey(retainSeg.OriginDestinationId))
                        oDwithDates.Add(retainSeg.OriginDestinationId, segmentDetail.FirstOrDefault(seg => seg.OriginDestinationRefId == retainSeg.OriginDestinationId).ScheduledDepartureTime.LocalDateTime.Split(Constants.Application.Timespan).FirstOrDefault());
                }
            }
            oDwithDates = oDwithDates.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (var slice in oDwithDates)
            {
                if (ReshopQuery.FlightSpecific.FlightsRequest.Add.Any(addOd => addOd.OriginDestinationId.Contains(slice.Key)))
                {
                    var addOd = originDestinationAssociations.FirstOrDefault(od => !string.IsNullOrEmpty(od.OdLabel) && od.OdLabel == slice.Key);
                    if (!searchInputsSlices.Any(s => s.origin == addOd.OriginDestination.OriginAirport && s.destination == addOd.OriginDestination.DestinationAirport && s.date == addOd.OriginDestination.DepartureDate))
                    {
                        searchInputsSlices.Add(new searchInputsSlice
                        {
                            destination = addOd.OriginDestination.DestinationAirport,
                            origin = addOd.OriginDestination.OriginAirport,
                            date = addOd.OriginDestination.DepartureDate,
                            time = addOd.OriginDestination?.DepartureTimeRange?.EarliestTime != null ? DateTime.Parse(addOd.OriginDestination?.DepartureTimeRange?.EarliestTime).ToString("htt").ToLower() : "anytime"
                        });
                    }
                }
                else
                {
                    IEnumerable<Segment> segLst = Array.Empty<Segment>();
                    if (ReshopQuery.FlightSpecific.FlightsRequest.Retain.Any(retainOd => retainOd != null && !string.IsNullOrEmpty(retainOd.OriginDestinationId) && retainOd.OriginDestinationId.Contains(slice.Key)))
                        segLst = segmentDetail.Where(seg => seg.OriginDestinationRefId == slice.Key);

                    if (segLst.Any())
                    {
                        searchInputSliceSegs = new();
                        foreach (var segDtls in segLst)
                        {
                            searchInputSliceSegs.Add(new searchInputsSliceSegment
                            {
                                bookingCode = segDtls.ClassOfService,
                                departure = segDtls.ScheduledDepartureTime.LocalDateTime,
                                destination = segDtls.DestinationAirportCode,
                                flight = segDtls.MarketingCarrier + segDtls.MarketingFlightNumber.TrimStart('0'),
                                origin = segDtls.OriginAirportCode
                            });
                        }
                        if (searchInputSliceSegs.Any())
                        {
                            searchInputsSlices.Add(new searchInputsSlice
                            {
                                segment = searchInputSliceSegs.ToArray()
                            });
                        }
                    }
                }
            }
            return searchInputsSlices;
        }

        private static IEnumerable<searchInputsFareFamilyToPrice> GetFareFamilies(ReshopRequest reshopRequest)
        {
            return reshopRequest.ReshopQueries?.FlightSpecific?.SearchCriteria?.FareFamilies?.Where(fareFamily => !string.IsNullOrWhiteSpace(fareFamily)).Select(fareFamily =>
            new searchInputsFareFamilyToPrice()
            {
                name = fareFamily,
            }) ?? Enumerable.Empty<searchInputsFareFamilyToPrice>();
        }
    }
}
