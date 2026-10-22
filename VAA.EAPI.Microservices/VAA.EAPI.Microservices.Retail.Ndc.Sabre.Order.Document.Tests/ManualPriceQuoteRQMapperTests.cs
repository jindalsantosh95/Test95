using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Tests
{
    public class ManualPriceQuoteRQMapperTests
    {
        public ManualPriceQuoteRQMapperTests()
        {
            ConfigReader.Set("ConversationId", "ConversationId");
            ConfigReader.Set("FromPartyId", "FromPartyId");
            ConfigReader.Set("ToPartyId", "ToPartyId");
        }

        [Fact]
        public void ManualPriceQuoteRQMapperContentShouldPadBaggageAllowanceNumber_WhenCheckedBagsExist()
        {
            var request = BuildIssueTicketRequest(allowedBagCount: 1);

            var result = ManualPriceQuoteRQMapper.ManualPriceQuoteAddInfoRQRequest(
                token: "token",
                type: true,
                issueTicketRequest: request,
                ptcType: "ADT",
                index: 0,
                passengers: Array.Empty<EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB>());

            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("01", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().BaggageAllowance.Number);
        }

        [Fact]
        public void ManualPriceQuoteRQMapperContentShouldMapZeroBaggageAllowanceNumber_WhenNoCheckedBagsAllowed()
        {
            var request = BuildIssueTicketRequest(allowedBagCount: 0);

            var result = ManualPriceQuoteRQMapper.ManualPriceQuoteAddInfoRQRequest(
                token: "token",
                type: true,
                issueTicketRequest: request,
                ptcType: "ADT",
                index: 0,
                passengers: Array.Empty<EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation.PassengerPNRB>());

            Assert.NotNull(result);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ);
            Assert.NotNull(result.ManualPriceQuoteAddInfoRQ.ContentDetails);
            Assert.Equal("0", result.ManualPriceQuoteAddInfoRQ.ContentDetails.LineEntitlements.First().BaggageAllowance.Number);
        }

        private static IssueTicketRequest BuildIssueTicketRequest(int allowedBagCount)
        {
            const string customerId = "ADULT_1";
            const string ptc = "ADT";
            const string segmentId = "SEG1";
            const string baggageId = "BAG1";
            const string priceClassId = "PC1";
            const string fareBasisCode = "BY2QBDLA";

            return new IssueTicketRequest
            {
                Pos = new Pos { CountryCode = "GB" },
                parties = new Parties(),
                CustomerDetails =
                [
                    new Customer { CustId = customerId, Ptc = ptc }
                ],
                SegmentDetails =
                [
                    new Segment
                    {
                        SegmentId = segmentId,
                        FareBasis = fareBasisCode,
                        TicketNotValidAfter = "2026-12-31",
                        TicketNotValidBefore = "2026-12-31"
                    }
                ],
                PriceClass =
                [
                    new PriceClass
                    {
                        Id = priceClassId,
                        FareBasisCode = fareBasisCode
                    }
                ],
                BaggageAllowances =
                [
                    new BaggageAllowances
                    {
                        BaggageAllowanceId = baggageId,
                        BaggageAllowanceTypeCode = "Checked",
                        FreeBaggageAllowance = new FreeBaggageAllowance
                        {
                            AllowedBagCnt = allowedBagCount,
                            BagDescriptors = new(),
                            DimensionAllowance = new(),
                            WeightAllowance = new()
                        }
                    }
                ],
                SelectedOffers =
                [
                    new SelectedOffer
                    {
                        OfferItems =
                        [
                            new OfferItem
                            {
                                TicketDesigCode = "TD",
                                Services =
                                [
                                    new OfferService
                                    {
                                        ServiceBeneficiaries = [customerId],
                                        FlightSegmentsRefIds = [segmentId],
                                        PriceClassRefIds = [priceClassId],
                                        BaggageRefIds = [baggageId]
                                    }
                                ],
                                OfferitemPricing =
                                [
                                    new OfferitemPricing
                                    {
                                        FareCalculationText = "LON VS ATL NUC100.00END ROE1.0",
                                        SalePrice =
                                        [
                                            new Price
                                            {
                                                BaseCurrencyAmount = new BaseCurrencyAmount
                                                {
                                                    Amount = "100.00",
                                                    Currency = "GBP"
                                                },
                                                Amount = new CurrencyAmount
                                                {
                                                    MonetaryAmount = "100.00",
                                                    BaseCurrency = "GBP"
                                                },
                                                TaxBreakup =
                                                [
                                                    new Tax
                                                    {
                                                        Code = "TX",
                                                        Amount = new CurrencyAmount { MonetaryAmount = "10.00" }
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]
            };
        }
    }
}

