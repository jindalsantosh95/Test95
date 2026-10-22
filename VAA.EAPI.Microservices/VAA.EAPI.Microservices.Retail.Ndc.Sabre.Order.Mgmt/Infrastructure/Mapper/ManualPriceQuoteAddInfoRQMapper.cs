using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class ManualPriceQuoteAddInfoRQMapper
    {
        public static ManualPriceQuoteAddInfoRQRequest ManualPriceQuoteAddInfoRQRequest(string token, bool type, OrderChangeRequestData request, string ptcType, string index, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                MessageHeader = new()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },

                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },

                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.ManualPriceQuoteAction,
                },
                ManualPriceQuoteAddInfoRQ = ManualPriceQuoteReqMapper(request, type, ptcType, index, passengers),
                Security = new() { BinarySecurityToken = token }
            };
        }
        public static ManualPriceQuoteAddInfoRQ ManualPriceQuoteReqMapper(OrderChangeRequestData request, bool type, string ptcType, string index, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            return new()
            {
                CreationDetails = !type ? GetRQCreationDetails(request.DataList.SegmentDetails, request.DataList.CustomerDetails, ptcType, passengers) : null,
                ContentDetails = type ? GetRQContentDetails(request, index, ptcType) : null,
                Version = "2.0.0"
            };
        }

        private static ManualPriceQuoteAddInfoRQContentDetails GetRQContentDetails(OrderChangeRequestData request, string index, string ptc)
        {
            var custIds = request.DataList.CustomerDetails.Where(x => x.Ptc == ptc).Select(x => x.CustId);
            var offerItem = request.SelectedOffers.First().OfferItems.First(x => x.Services.Any(s => s.ServiceBeneficiaries.Any(sb => custIds.Contains(sb))));
            var offerItemPricing = offerItem.OfferitemPricing.First();

            Price[] price = offerItemPricing.SalePrice.Where(x => x.BaseCurrencyAmount != null).ToArray();

            var taxBreakup = offerItemPricing.SalePrice.Select(x => x.EquivalentTaxBreakup ?? x.TaxBreakup).FirstOrDefault(t => t != null);

            ManualPriceQuoteAddInfoRQContentDetailsFareLadderTax[] taxes = new ManualPriceQuoteAddInfoRQContentDetailsFareLadderTax[taxBreakup.Count()];

            for (int i = 0; i < taxBreakup.Count(); i++)
            {
                var tax = taxBreakup.ElementAt(i);

                taxes[i] = new ManualPriceQuoteAddInfoRQContentDetailsFareLadderTax
                {
                    Amount = tax.Amount.EquivalentMonetaryAmount ?? tax.Amount.MonetaryAmount,
                    TaxCode = tax.Code
                };
            }
            bool isXFInvolved = request.DataList.PriceClass.Any(x => x.FareBasisCode.Contains("XF"));

            if (isXFInvolved)
            {
                var splittedFareCalc = offerItemPricing.FareCalculationText.Split(' ');

                for (int i = 0; i < splittedFareCalc.Length; i++)
                {
                    var matchedFb = request.DataList.PriceClass.FirstOrDefault(fb => splittedFareCalc[i].Contains(fb.FareBasisCode));
                    if (matchedFb != null)
                    {
                        var indexCount = splittedFareCalc[i].IndexOf(matchedFb.FareBasisCode);
                        splittedFareCalc[i] = splittedFareCalc[i].Substring(0, indexCount);
                    }
                }
                offerItemPricing.FareCalculationText = string.Join(' ', splittedFareCalc);
            }

            var fare = price.First();
            return new ManualPriceQuoteAddInfoRQContentDetails()
            {
                Record = index,
                FareCalc = new ManualPriceQuoteAddInfoRQContentDetailsFareCalc
                {
                    Text = offerItemPricing.FareCalculationText.Replace("XF ", "XF")
                },
                FareLadder = new ManualPriceQuoteAddInfoRQContentDetailsFareLadder
                {
                    BaseFare = new ManualPriceQuoteAddInfoRQContentDetailsFareLadderBaseFare
                    {
                        Amount = fare.BaseCurrencyAmount.EquivalentAmount ?? fare.BaseCurrencyAmount.Amount,
                        CurrencyCode = fare.BaseCurrencyAmount.EquivalentCurrency ?? fare.BaseCurrencyAmount.Currency
                    },
                    EquivFare = (request.Pos?.CountryCode != "US" && fare.BaseCurrencyAmount?.Currency != fare.Amount.BaseCurrency) ? new ManualPriceQuoteAddInfoRQContentDetailsFareLadderEquivFare
                    {
                        Amount = fare.Amount.MonetaryAmount,
                        CurrencyCode = fare.Amount.BaseCurrency,
                    } : null,
                    Taxes = taxes,
                },
                LineEntitlements = GetLine(offerItem, request,offerItem.TicketDesigCode)
            };
        }

        private static ManualPriceQuoteAddInfoRQContentDetailsLine1[] GetLine(EAPI.Domain.Virgin.Models.Offer.OfferItem offerItem, OrderChangeRequestData request , string ticketDesigCode)
        {
            return [.. request.DataList.PriceClass
                .SelectMany(pc =>
                {
                    if (offerItem.Services.Any(x => x.PriceClassRefIds.Contains(pc.Id)))
                    {
                        var services = offerItem.Services.Where(x => x.PriceClassRefIds.Contains(pc.Id));
                        var segs = services.SelectMany(x => x.FlightSegmentsRefIds);
                        (Dictionary<string, string> segments, bool hasARANKSegments) aRANKsegments = CheckForARANKSegments(request.DataList.SegmentDetails);

                        return segs.Select(s =>
                        {
                            var flightSegment = request.DataList.SegmentDetails.FirstOrDefault(f => f.SegmentId == s);
                            var connectionIndicator = flightSegment?.ConnectionOrStopOver == "X" ? "X" : null;
                            var baggageIds = offerItem.Services?.FirstOrDefault(seg => seg.FlightSegmentsRefIds?.Contains(s) == true)?.BaggageRefIds;
                            var checkedBagCount = request.DataList.BaggageAllowances?
                                .FirstOrDefault(a => a.BaggageAllowanceTypeCode == "Checked" && (baggageIds?.Contains(a.BaggageAllowanceId) == true))
                                ?.FreeBaggageAllowance?.AllowedBagCnt ?? 0;

                            return new ManualPriceQuoteAddInfoRQContentDetailsLine1
                            {
                                BaggageAllowance = new()
                                {
                                    Number = checkedBagCount == 0 ? "0" : checkedBagCount.ToString().PadLeft(2, '0'),
                                },
                                FareBasis = new() { Code = pc.FareBasisCode, TicketDesignator = !string.IsNullOrWhiteSpace(request?.Parties?.CorporateAccount?.TicketDesignator) ? request.Parties.CorporateAccount.TicketDesignator : (pc.TicketDesigCode ?? ticketDesigCode) },
                                ItineraryOptions = new()
                                {
                                    SegmentSelect =
                                    [
                                        new ManualPriceQuoteAddInfoRQContentDetailsLineItineraryOptionsSegmentSelect
                                        {
                                            Number = aRANKsegments.hasARANKSegments ? aRANKsegments.segments.FirstOrDefault(x => x.Key == s.Trim('F','S', 'E', 'G')).Value : aRANKsegments.segments.FirstOrDefault(x => x.Key == s.Trim('F','S', 'E', 'G')).Key,
                                            ConnectionInd = connectionIndicator,
                                        }                                  
                                    ]
                                },
                                ValidityDates = new()
                                {
                                    NotValidAfter = new() { Date = flightSegment.TicketNotValidAfter },
                                    NotValidBefore = new() { Date = flightSegment.TicketNotValidBefore }
                                }
                            };
                        });
                    }

                    return [];
                })];
        }

        private static ManualPriceQuoteAddInfoRQCreationDetails GetRQCreationDetails(IEnumerable<Segment> segmentDetails, IEnumerable<Customer> customerDetails, string ptcType, EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB[] passengers)
        {
            (Dictionary<string, string> segments, bool hasARANKSegments) aRANKsegments = CheckForARANKSegments(segmentDetails);

            ptcType = ptcType == "CHD" ? "CNN" : ptcType == "GBE" ? "YTH" : ptcType;

            var nameIds = passengers.Where(x => x.passengerType == ptcType).Select(x => x.nameId);

            return new ManualPriceQuoteAddInfoRQCreationDetails()
            {
                ItineraryOptions = aRANKsegments.hasARANKSegments ? aRANKsegments.segments.Select(seg =>
                                    new ManualPriceQuoteAddInfoRQCreationDetailsSegmentSelect()
                                    {
                                        Number = seg.Key
                                    }).ToArray() : aRANKsegments.segments.Select(seg => new ManualPriceQuoteAddInfoRQCreationDetailsSegmentSelect()
                                    {
                                        Number = seg.Key
                                    }).ToArray(),
                PassengerType = [ptcType],
                NameSelect = nameIds.Select(x => new ManualPriceQuoteAddInfoRQCreationDetailsNameSelect()
                {
                    NameNumber = x
                }).ToArray()
            };
        }

        private static (Dictionary<string, string>, bool) CheckForARANKSegments(IEnumerable<Segment> segmentDetails)
        {
            bool hasARANKSegments = false;
            int aRANKSegmentCounter = 0;

            Dictionary<string, string> aRANKSegmentList = new();

            for (int i = 0; i < segmentDetails.Count(); i++)
            {
                var currentSegment = segmentDetails.ElementAt(i).SegmentId.Trim('F', 'S', 'E', 'G');

                string aRANKSegment = currentSegment;

                if (i > 0 && segmentDetails.ElementAt(i - 1).DestinationAirportCode != segmentDetails.ElementAt(i).OriginAirportCode)
                {
                    hasARANKSegments = true;
                    aRANKSegmentCounter++;
                    aRANKSegment = (int.Parse(aRANKSegment) + aRANKSegmentCounter).ToString();
                }

                if (hasARANKSegments && currentSegment == aRANKSegment)
                {
                    aRANKSegment = (int.Parse(aRANKSegment) + aRANKSegmentCounter).ToString();
                }


                aRANKSegmentList.Add(currentSegment, aRANKSegment);
            }

            return (aRANKSegmentList, hasARANKSegments);
        }
    }
}