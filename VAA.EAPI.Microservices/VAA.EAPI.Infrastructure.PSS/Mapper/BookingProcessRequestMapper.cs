using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.PSS.Proxy;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class BookingProcessRequestMapper
    {
        public static PerformRevenueRequestType MapBookingRequest(List<SelectedOffer> selectedOffers,
            List<OriginDestinationAssociation> originDestinationAssociations, List<Customer> customer, List<Segment> segments,
            List<PriceClass> priceClasses)
        {
            var revenueStoreFareRequest = new RevenueStoreFareRequestType();
            var fareSolAmountPtc = new List<FareSolutionAmountPtcType>();
            var fareSolFarePtc = new List<FareSolutionFarePtcType>();
            var fareSolCalc = new List<FareSolutionFareCalcPtcType>();

            revenueStoreFareRequest.FareSolutionPtc = GetFareSolutionPtcs(customer, selectedOffers, fareSolAmountPtc, segments, fareSolFarePtc,
                fareSolCalc, priceClasses).ToArray();

            var request = new PerformRevenueRequestType()
            {
                RequestInfo = new RequestInfoType
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType
                    {
                        City = ConfigReader.Get(Constants.Environment.AgentCityCode),
                        PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                    },
                },
                SetupContextRequest = new SetupContextRequestType
                {
                    RequestInfo = new RequestInfoType
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                        HostContextParams = new HostContextParamsType
                        {
                            PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                        },
                    },
                    StampingInfo = new StampingInfoType
                    {
                        UserAuthorizationData = new UserAuthorizationDataType
                        {
                            AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                        }
                    },
                },
                CreateRequest = new CreateRequestType
                {
                    RequestInfo = new RequestInfoType
                    {
                        ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                        HostContextParams = new HostContextParamsType
                        {
                            City = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                        },
                    },
                    StampingInfo = new StampingInfoType
                    {
                        UserAuthorizationData = new UserAuthorizationDataType
                        {
                            AgentCityCode = ConfigReader.Get(Constants.Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Constants.Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Constants.Environment.SecurityId)
                        }
                    },
                    FlightInfo = GetFlightInfo(segments, customer.Count.ToString(), selectedOffers.First().OfferItems, priceClasses).ToArray(),
                    PnrNameDataType = GetPnrNameData(customer).ToArray(),
                    Phone = new PhoneType[] { new PhoneType() { CityCode = originDestinationAssociations.First().OriginDestination.OriginAirport, FreeformText = $"{customer.First(x => x.Ptc == "ADT").Contacts.Phone}**Home/{customer.First(x => x.Ptc == "ADT").Contacts.Countrycode}**" } }
                },
                AddRemarksRequest = GetAddRemarksRequests(customer).ToArray()
            };

            revenueStoreFareRequest.FareSolutionAmountPtc = fareSolAmountPtc.ToArray();
            revenueStoreFareRequest.FareSolutionFarePtc = fareSolFarePtc.ToArray();
            revenueStoreFareRequest.FareSolutionFareCalcPtc = fareSolCalc.ToArray();
            request.RevenueStoreFareRequest = revenueStoreFareRequest;

            return request;
        }

        public static List<FlightInfoType> GetFlightInfo(List<Segment> segmentDetails, string count, List<OfferItem> offerItems, List<PriceClass> priceClasses)
        {
            var flightInfo = new List<FlightInfoType>();
            var segNumber = 1;

            foreach (var segment in segmentDetails)
            {
                flightInfo.Add(new FlightInfoType()
                {
                    AirlineCode = Constants.Application.VSAirlineCode,
                    ArrivalPoint = segment.DestinationAirportCode,
                    DeparturePoint = segment.OriginAirportCode,
                    ClassOfService = segment.ClassOfService,
                    SegmentNumber = $"{segNumber++}".PadLeft(2, '0'),
                    FlightNumber = segment.MarketingFlightNumber,
                    ArrivalTime = DateTimeHelper.ConvertDateToString(segment.ScheduledArrivalTime.LocalDateTime, Constants.Application.DateTimeFormat, Constants.Application.DeltaTimeFormat),
                    DepartureTime = DateTimeHelper.ConvertDateToString(segment.ScheduledDepartureTime.LocalDateTime, Constants.Application.DateTimeFormat, Constants.Application.DeltaTimeFormat),
                    Date = DateTimeHelper.ConvertDateToString(segment.ScheduledDepartureTime.LocalDateTime, Constants.Application.DateTimeFormat, Constants.Application.DeltaDateFormat),
                    ItineraryStatus = "NN",
                    NumberInParty = count
                });
            }

            return flightInfo;
        }

        public static List<PnrNameDataType> GetPnrNameData(List<Customer> customerDetails)
        {
            var pnrNameData = new List<PnrNameDataType>();

            foreach (var cust in customerDetails)
            {
                pnrNameData.Add(new PnrNameDataType()
                {
                    FirstName = cust.FirstName,
                    LastName = cust.LastName,
                    RevenueStandbyFlag = "Y",
                    NonRevenueStandbyFlag = "N"
                });
            }

            return pnrNameData;
        }

        public static List<AddRemarksRequestType> GetAddRemarksRequests(List<Customer> customerDetails)
        {
            var cust = customerDetails.First(x => x.Ptc == "ADT");
            return new List<AddRemarksRequestType>()
            {
                new AddRemarksRequestType()
                {
                    AddOSIRemarkRequest = new AddOSIRemarkRequestType[]
                    {
                        new AddOSIRemarkRequestType()
                        {
                            RemarkInfo = new RemarkInfoType()
                            {
                                AirlineCode = Constants.Application.VSAirlineCode,
                                FreeformText = $"CTC /{cust.Contacts.Email}"
                            }
                        }
                    }
                },
                new AddRemarksRequestType()
                {
                    AddOSIRemarkRequest = new AddOSIRemarkRequestType[]
                    {
                        new AddOSIRemarkRequestType()
                        {
                            RemarkInfo = new RemarkInfoType()
                            {
                                AirlineCode = Constants.Application.VSAirlineCode,
                                FreeformText = $"CTC /{cust.Contacts.Phone}"
                            }
                        }
                    }
                }
            };
        }

        public static List<FareSolutionPtcType> GetFareSolutionPtcs(List<Customer> customerDetails, List<SelectedOffer> selectedOffers, List<FareSolutionAmountPtcType> fareSolutionAmountPtcs, List<Segment> segmentDetails, List<FareSolutionFarePtcType> fareSolutionFarePtcs, List<FareSolutionFareCalcPtcType> fareSolutionFareCalcPtcs, List<PriceClass> priceClasses)
        {
            var fareSolPtc = new List<FareSolutionPtcType>();
            var distinctPts = customerDetails.GroupBy(x => x.Ptc);

            foreach (var ptc in distinctPts)
            {
                fareSolPtc.Add(new FareSolutionPtcType()
                {
                    NumberOfPassengers = ptc.Count().ToString(),
                    PtcCode = ptc.Key,
                    ValidatingCarrier = Constants.Application.VSAirlineCode
                });

                AddFareSolutionAmountPtcs(ptc.First().CustId, ptc.First().Ptc, selectedOffers.First(), fareSolutionAmountPtcs, segmentDetails, fareSolutionFarePtcs, customerDetails, fareSolutionFareCalcPtcs, priceClasses);
            }
            return fareSolPtc;
        }

        public static void AddFareSolutionAmountPtcs(string custId, string ptc, SelectedOffer selectedOffers, List<FareSolutionAmountPtcType> fareSolutionAmountPtcs,
            List<Segment> segmentDetails, List<FareSolutionFarePtcType> fareSolutionFarePtcs, List<Customer> customerDetails,
            List<FareSolutionFareCalcPtcType> fareSolutionFareCalcPtcs, List<PriceClass> priceClasses)
        {
            var amountNumber = 1;
            var offerItem = selectedOffers.OfferItems.First(x => x.Services.First().ServiceBeneficiaries.First() == custId);

            foreach (var pricing in offerItem.OfferitemPricing.First().SalePrice)
            {
                if (pricing.PriceComponentType != PriceComponentTypes.TotalTaxEnum)
                {
                    fareSolutionAmountPtcs.Add(new FareSolutionAmountPtcType()
                    {
                        PassengerTypeCode = ptc,
                        FareSolutionAmount = new FareSolutionAmountType()
                        {
                            Amount = pricing.Amount.MonetaryAmount,
                            AmountNumber = $"{amountNumber++}".PadLeft(2, '0'),
                            CurrencyCode = pricing.Amount.BaseCurrency,
                            TypeOfTax = pricing.PriceComponentName,
                            TypeOfAmount = GetTypeOfAmount(pricing.PriceComponentType),
                            CityCodeForCharge = "***"
                        }
                    });
                }
            }

            AddFareSolutionFarePtcs(segmentDetails, ptc, fareSolutionFarePtcs, offerItem.OfferitemPricing.First().SalePrice, offerItem.Services, priceClasses);
            AddFareSolutionFareCalcs(customerDetails, offerItem.OfferitemPricing.First().FareCalculationText, fareSolutionFareCalcPtcs, ptc);
        }

        public static string? GetTypeOfAmount(PriceComponentTypes? priceComponentType)
        {
            return priceComponentType switch
            {
                PriceComponentTypes.BasePriceEnum => "T",
                PriceComponentTypes.BaseFareEnum => "B",
                PriceComponentTypes.TaxEnum => "X",
                _ => null
            };
        }

        public static void AddFareSolutionFareCalcs(List<Customer> customerDetails, string fareCalc, List<FareSolutionFareCalcPtcType> fareSolutionFareCalcs, string ptc)
        {
            fareSolutionFareCalcs.Add(new FareSolutionFareCalcPtcType()
            {
                PassengerTypeCode = ptc,
                FareSolutionFareCalc = new FareSolutionFareCalcType()
                {
                    FareCalc = fareCalc
                }
            });
        }

        private static void AddFareSolutionFarePtcs(List<Segment> segmentDetails, string ptc, List<FareSolutionFarePtcType> fareSolutionFarePtcs,
            IEnumerable<Price> salePrices, IEnumerable<OfferService> services, IEnumerable<PriceClass> priceClasses)
        {
            var segId = 1;

            foreach (var seg in segmentDetails)
            {
                var priceClassRefId = services.First(x => x.ProductSegmentAssociation.OfferSegmentRefIds.Contains(seg.SegmentId)).PriceClassRefIds;

                fareSolutionFarePtcs.Add(new FareSolutionFarePtcType()
                {
                    PassengerTypeCode = ptc,
                    FareSolutionFare = new FareSolutionFareType()
                    {
                        SegmentNumber = segId.ToString().PadLeft(2, '0'),
                        FareValue = salePrices.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount,
                        CurrencyCode = salePrices.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.BaseCurrency,
                        FareBasisCodeAndTicketDesignator = priceClasses.First(x => x.Id == priceClassRefId.FirstOrDefault()).FareBasisCode,
                        PriceableUnitNumber = segId.ToString().PadLeft(2, '0'),
                        ConnectionOrStopOver = "X"
                    }
                });

                segId++;
            }
        }
    }
}
