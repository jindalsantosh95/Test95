using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Helper;
using VAA.EAPI.NDC.Offer.Infrastructure.Mapper;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using Segment = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public class ServiceListRSMapper
    {
        public static Response<IATA_ServiceListRS> MapServiceListResponse(OfferResponse offerDocument, bool isOfferServiceRq, OrderDocument orderDocument, IGenericDataMapper genericDataMapper, IEnumerable<NDCWarning> warningList, IEnumerable<NDCWarning> warningofferList, FlightServiceResponse response)
        {
            var dataList = new DataListsType();
            IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList = Enumerable.Empty<DatedMarketingSegmentType>();
            IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList = Enumerable.Empty<DatedOperatingSegmentType>();
            IEnumerable<PaxSegmentType> paxSegmentList = Enumerable.Empty<PaxSegmentType>();
            IEnumerable<PaxSegmentType> paxSegmentRef = Enumerable.Empty<PaxSegmentType>();
            IEnumerable<PaxType> paxList = Enumerable.Empty<PaxType>();
            var alacarteOffer = new ALaCarteOfferType();
            IEnumerable<Domain.Model.Order.FlightSegment> segmentList = Enumerable.Empty<Domain.Model.Order.FlightSegment>();
            IEnumerable<Segment> offersegmentList = Enumerable.Empty<Segment>(); ;
            var orderSegment = new VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment();
            var offerSegment = new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment();
            var currencyCode = string.Empty;
            var currencyDecimalFormat = string.Empty;

            if (!isOfferServiceRq)
            {

                paxList = orderDocument.DataList.CustomerDetails.Select(customer => new PaxType()
                {
                    PaxID = customer.CustId,
                    PTC = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc
                });

                segmentList = orderDocument.DataList.SegmentDetails
                    .Select(segment => new FlightSegment()
                    {
                        MarketingCarrier = segment.MarketingCarrier,
                        MarketingFlightNumber = segment.MarketingFlightNumber,
                        OperatingCarrier = segment.OperatingCarrier,
                        OperatingFlightNumber = segment.OperatingFlightNumber,
                        OriginAirportCode = segment.OriginAirportCode,
                        DestinationAirportCode = segment.DestinationAirportCode,
                        ClassOfService = segment.ClassOfService,
                        ScheduledArrivalTime = new()
                        {
                            LocalDateTime = Convert.ToDateTime(segment.ScheduledArrivalTime?.LocalDateTime)
                        },
                        ScheduledDepartureTime = new()
                        {
                            LocalDateTime = Convert.ToDateTime(segment.ScheduledDepartureTime?.LocalDateTime)
                        },
                        SegmentNumber = segment.SegmentId,
                        SegmentID = segment.SegmentId,
                        FlightDuration = TimeSpan.Parse(segment.FlightDuration)
                    });

                currencyCode = orderDocument?.OrderItems?.FirstOrDefault()?.PriceDetails?.FirstOrDefault()?.Amount?.BaseCurrency;
                currencyDecimalFormat = DecimalHelper.GetDecimalFormat(orderDocument?.OrderItems?.FirstOrDefault()?.PriceDetails?.FirstOrDefault().Amount?.MonetaryAmount);
            }
            else
            {
                if (offerDocument?.DataList != null)
                {
                    paxList = offerDocument.DataList.CustomerDetails.Select(customer => new PaxType()
                    {
                        PaxID = customer.CustId,
                        PTC = customer.Ptc == Constants.Application.CNNPTCCode ? Constants.Application.ChildPTCCode : customer.Ptc
                    });
                }
                offersegmentList = offerDocument?.DataList?.FlightSegments;
                currencyCode = offerDocument?.Offers?.FirstOrDefault()?.AveragePassengerPrice?.Amount?.BaseCurrency;
                currencyDecimalFormat = DecimalHelper.GetDecimalFormat(offerDocument?.Offers?.FirstOrDefault()?.AveragePassengerPrice?.Amount?.MonetaryAmount);
            }
            if (segmentList != null)
            {
                foreach (var os in segmentList)
                {
                    GetPaxSegmentId(os, ref datedMarketingSegmentList, ref paxSegmentList, ref paxSegmentRef);
                    GetDatedMarketingSegmentList(os, offerSegment, ref datedMarketingSegmentList, ref datedOperatingSegmentList, genericDataMapper);
                }
            }
            if (offersegmentList != null)
            {
                foreach (var os in offersegmentList)
                {
                    GetPaxSegmentId(os, ref datedMarketingSegmentList, ref paxSegmentList, ref paxSegmentRef);
                    GetDatedMarketingSegmentList(orderSegment, os, ref datedMarketingSegmentList, ref datedOperatingSegmentList, genericDataMapper);
                }
            }
            GetALaCarteOffer(response, alacarteOffer, paxSegmentRef, paxList, currencyCode, currencyDecimalFormat);

            GetDataList(dataList, ref datedMarketingSegmentList, ref datedOperatingSegmentList, paxSegmentList, paxList, response);

            return new Response<IATA_ServiceListRS>()
            {
                Status = Status.OK,
                Value = new IATA_ServiceListRS()
                {
                    Items = new ServiceListResponseType[1]
                    {
                        new ServiceListResponseType()
                        {
                            ALaCarteOffer = alacarteOffer,
                            DataLists = dataList,
                            Warning = ((warningList != null && warningList?.Count() > 0)||(warningofferList != null && warningofferList?.Count() > 0)) ? GetWarningList(warningList,warningofferList) : null
                        }
                    }
                }
            };
        }

        private static void GetALaCarteOffer(FlightServiceResponse response, ALaCarteOfferType alacarteOffer, IEnumerable<PaxSegmentType> paxSegmentList, IEnumerable<PaxType> paxList, string currencyCode, string currencyDecimalFormat)
        {
            var serviceIdCount = 0;
            var mealCounter = 0;
            var disablilityCounter = 0;
            var paxSegmentRefID = paxSegmentList.Select(x => x.PaxSegmentID).ToArray();


            var paxArray = paxList as PaxType[] ?? paxList.ToArray();

            var eligiblePaxRefByOfferItemId = response.Offers
                .SelectMany(x => x.OfferItems)
                .ToDictionary(
                    x => x.OfferItemId,
                    x => GetEligiblePaxRefID(x, paxArray),
                    StringComparer.OrdinalIgnoreCase);

            response.Offers = response.Offers.Select(o =>
            {
                o.OfferItems = o.OfferItems
                    .Where(i => eligiblePaxRefByOfferItemId.TryGetValue(i.OfferItemId, out var paxRefIds) && paxRefIds.Any())
                    .ToArray();

                return o;
            }).ToArray();

            alacarteOffer.OfferID = response.Offers.FirstOrDefault()?.OfferId;
            alacarteOffer.OwnerCode = VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.OwnerCode;
            alacarteOffer.OfferItem = response.Offers.SelectMany(x => x.OfferItems).Select(y =>
                new ALaCarteOfferItemType()
                {
                    OfferItemID = y.OfferItemId,
                    Eligibility = new EligibilityType()
                    {
                        OfferFlightAssociations = new OfferFlightAssociationsType()
                        {
                            Item = new PaxSegmentRefListType()
                            {
                                PaxSegmentRefID = paxSegmentRefID
                            },
                            ItemElementName = ItemChoiceType.PaxSegmentReferences
                        },
                        PaxRefID = eligiblePaxRefByOfferItemId[y.OfferItemId]
                    },
                    Service = new ALaCarteServiceType()
                    {
                        ServiceDefinitionRefID = y.Classification == VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.Meals ? $"Meal{++mealCounter:00}" : $"Disable{++disablilityCounter:00}",
                        ServiceID = $"SV{++serviceIdCount}"
                    },
                    UnitPrice = new PriceType()
                    {
                        TotalAmount = new AmountType()
                        {
                            CurCode = currencyCode,
                            Value = string.IsNullOrEmpty(y.Price?.MonetaryAmount) ? Convert.ToDecimal(string.Format(currencyDecimalFormat, 0)) : Convert.ToDecimal(y.Price?.MonetaryAmount)
                        }
                    }
                }
            ).ToArray();
        }

        private static string[] GetEligiblePaxRefID(Infrastructure.Model.FlightServiceMS.OfferItem offerItem, IEnumerable<PaxType> paxList)
        {
            if (offerItem?.PTC == null)
                return Array.Empty<string>();

            return paxList
                .Where(x => offerItem.PTC.Contains(x.PTC))
                .Select(x => x.PaxID)
                .ToArray();
        }

        private static WarningType[] GetWarningList(IEnumerable<NDCWarning> warningorderList, IEnumerable<NDCWarning> warningofferList)
        {
            IEnumerable<NDCWarning> warningList = Enumerable.Empty<NDCWarning>();

            if (warningorderList != null) { warningList = warningorderList; }
            if (warningofferList != null) { warningList = warningofferList; }

            return warningList.Where(w => w != null)
               .Select(x => new WarningType
               {
                   Code = Convert.ToString(x.Code),
                   DescText = x.ShortText,
                   LangCode = Domain.Model.VAA.Constants.LangCode,
                   TypeCode = x.Type,
                   TagText = x.TagText
               }).ToArray();
        }

        private static DataListsType GetDataList(DataListsType dataList, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList, IEnumerable<PaxType> paxList, Infrastructure.Model.FlightServiceMS.FlightServiceResponse response)
        {
            dataList.PaxList = AssociateInfantV2(paxList).ToArray();
            dataList.DatedMarketingSegmentList = datedMarketingSegmentList.ToArray();
            dataList.DatedOperatingSegmentList = datedOperatingSegmentList.ToArray();
            dataList.PaxSegmentList = paxSegmentList.ToArray();
            dataList.ServiceDefinitionList = GetServiceDefinitionList(response);

            return dataList;
        }
        private static ServiceDefinitionType[] GetServiceDefinitionList(FlightServiceResponse response)
        {
            var mealCounter = 00;
            var disablilityCounter = 00;

            var serviceDefinitionList = response.Offers.SelectMany(x => x.OfferItems).Select(y => new ServiceDefinitionType()
            {
                Name = y.Description,
                OwnerCode = VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.OWNERCODE,
                ServiceCode = y.SSRCode,
                RFIC = y.ReasonForIssuanceCode,
                RFISC = y.ReasonForIssuanceSubCode,
                ServiceDefinitionID = y.Classification == VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.Meals ? $"Meal{++mealCounter:00}" : $"Disable{++disablilityCounter:00}"
            }).ToArray();

            return serviceDefinitionList;
        }

        private static string GetPaxSegmentId(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment orderflightSegment, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<PaxSegmentType> paxSegmentList, ref IEnumerable<PaxSegmentType> paxSegmentRef)
        {
            if (datedMarketingSegmentList.Any(x => x?.Dep?.IATA_LocationCode == orderflightSegment?.OriginAirportCode && x?.Arrival?.IATA_LocationCode == orderflightSegment?.DestinationAirportCode && DateTime.Equals(x?.Dep?.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(orderflightSegment?.ScheduledDepartureTime?.LocalDateTime)).ToUniversalTime())))
            {
                return paxSegmentList?.FirstOrDefault()?.PaxSegmentID;
            }
            else
            {
                var paxSegmentType = CreateSeatPaxSegmentType(paxSegmentList);

                if (orderflightSegment?.OperatingCarrier == null)  // only for VS Segments
                {
                    paxSegmentRef = paxSegmentRef.Append(paxSegmentType);
                }
                paxSegmentList = paxSegmentList.Append(paxSegmentType);

                return paxSegmentType.PaxSegmentID;
            }
        }
        private static string GetPaxSegmentId(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment offerflightSegment, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<PaxSegmentType> paxSegmentList, ref IEnumerable<PaxSegmentType> paxSegmentRef)
        {
            if (datedMarketingSegmentList.Any(x => x?.Dep?.IATA_LocationCode == offerflightSegment?.OriginAirportCode && x?.Arrival?.IATA_LocationCode == offerflightSegment?.DestinationAirportCode && DateTime.Equals(x?.Dep?.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(offerflightSegment?.ScheduledDepartureTime?.LocalDateTime)).ToUniversalTime())))
            {
                return paxSegmentList?.FirstOrDefault()?.PaxSegmentID;
            }
            else
            {
                var paxSegmentType = CreateSeatPaxSegmentType(paxSegmentList);

                if (offerflightSegment?.OperatingCarrier == null) // only for VS Segments
                {
                    paxSegmentRef = paxSegmentRef.Append(paxSegmentType);
                }
                paxSegmentList = paxSegmentList.Append(paxSegmentType);

                return paxSegmentType.PaxSegmentID;
            }
        }


        private static PaxSegmentType CreateSeatPaxSegmentType(IEnumerable<PaxSegmentType> paxSegmentList)
        {
            return new PaxSegmentType()
            {
                PaxSegmentID = IdCreator.CreatePaxSegmentID(paxSegmentList.Count()),
                DatedMarketingSegmentRefId = IdCreator.CreateDatedMarketingSegmentID(paxSegmentList.Count())
            };
        }

        private static void GetDatedMarketingSegmentList(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment orderflightSegment, VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment offerflightSegment, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, IGenericDataMapper genericDataMapper)
        {
            if (orderflightSegment.SegmentID != null && !datedMarketingSegmentList.Any(x => x.Dep.IATA_LocationCode == orderflightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == orderflightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(orderflightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime()))))
            {
                datedMarketingSegmentList = datedMarketingSegmentList.Append(CreateDatedMarketingSegment(orderflightSegment, offerflightSegment, ref datedMarketingSegmentList, ref datedOperatingSegmentList, genericDataMapper));
            }
            else if (offerflightSegment.SegmentId != null && !datedMarketingSegmentList.Any(x => x.Dep.IATA_LocationCode == offerflightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == offerflightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(offerflightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime()))))
            {
                datedMarketingSegmentList = datedMarketingSegmentList.Append(CreateofferDatedMarketingSegment(orderflightSegment, offerflightSegment, ref datedMarketingSegmentList, ref datedOperatingSegmentList, genericDataMapper));
            }
        }

        private static DatedMarketingSegmentType CreateDatedMarketingSegment(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment flightSegment, VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment offerflightSegment, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, IGenericDataMapper genericDataMapper)
        {
            return new DatedMarketingSegmentType()
            {
                DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count()),
                DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(flightSegment, offerflightSegment, datedMarketingSegmentList.Count(), ref datedOperatingSegmentList, genericDataMapper),
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierDesigCode = flightSegment.MarketingCarrier,
                CarrierName = GetCarrierName(flightSegment.MarketingCarrier, genericDataMapper),
                MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber
            };
        }

        private static DatedMarketingSegmentType CreateofferDatedMarketingSegment(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment orderflightSegment, VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, ref IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, IGenericDataMapper genericDataMapper)
        {
            return new DatedMarketingSegmentType()
            {
                DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count()),
                DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(orderflightSegment, flightSegment, datedMarketingSegmentList.Count(), ref datedOperatingSegmentList, genericDataMapper),
                Dep = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Arrival = new TransportPointType()
                {
                    IATA_LocationCode = flightSegment.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierDesigCode = flightSegment.MarketingCarrier,
                CarrierName = GetCarrierName(flightSegment.MarketingCarrier, genericDataMapper),
                MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber
            };
        }

        private static string GetDatedOperatingSegmentID(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment flightSegment, VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment offerflightSegment, int count, ref IEnumerable<DatedOperatingSegmentType> datedOperatingSegmentList, IGenericDataMapper genericDataMapper)
        {
            DatedOperatingSegmentType datedOperatingSegment;
            datedOperatingSegment = flightSegment.SegmentID != null ? CreateDatedOperatingSegment(flightSegment, count, genericDataMapper) : CreateofferDatedOperatingSegment(offerflightSegment, count, genericDataMapper);
            datedOperatingSegmentList = datedOperatingSegmentList.Append(datedOperatingSegment);

            return datedOperatingSegment.DatedOperatingSegmentId;
        }

        private static DatedOperatingSegmentType CreateDatedOperatingSegment(VAA.EAPI.NDC.Offer.Domain.Model.Order.FlightSegment flightSegment, int count, IGenericDataMapper genericDataMapper)
        {
            return new DatedOperatingSegmentType()
            {
                DatedOperatingSegmentId = IdCreator.CreateDatedOperatingSegmentID(count),
                CarrierDesigCode = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                CarrierName = flightSegment.OperatingCarrier == null ? GetCarrierName(flightSegment.MarketingCarrier, genericDataMapper) : GetCarrierName(flightSegment.OperatingCarrier, genericDataMapper),
                OperatingCarrierFlightNumberText = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                Duration = GetDuration(flightSegment.FlightDuration)
            };
        }

        private static DatedOperatingSegmentType CreateofferDatedOperatingSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, int count, IGenericDataMapper genericDataMapper)
        {
            return new DatedOperatingSegmentType()
            {
                DatedOperatingSegmentId = IdCreator.CreateDatedOperatingSegmentID(count),
                CarrierDesigCode = flightSegment.OperatingCarrier ?? flightSegment.MarketingCarrier,
                CarrierName = flightSegment.OperatingCarrier == null ? GetCarrierName(flightSegment.MarketingCarrier, genericDataMapper) : GetCarrierName(flightSegment.OperatingCarrier, genericDataMapper),
                OperatingCarrierFlightNumberText = flightSegment.OperatingFlightNumber ?? flightSegment.MarketingFlightNumber,
                Duration = GetDuration(TimeSpan.Parse(flightSegment.FlightDuration))
            };
        }

        private static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Domain.Model.VAA.Constants.PT, timeSpan.Days * 24 + timeSpan.Hours, Domain.Model.VAA.Constants.H, timeSpan.Minutes, Domain.Model.VAA.Constants.M, timeSpan.Seconds, Domain.Model.VAA.Constants.S);
        }

        private static string GetCarrierName(string marketingCarrier, IGenericDataMapper genericDataMapper)
        {
            var name = genericDataMapper.GetOperatingCarrierNameByCode(marketingCarrier);

            return name;
        }

        private static IEnumerable<PaxType> AssociateInfantV2(IEnumerable<PaxType> paxList)
        {
            if (paxList.Any(x => x.PTC == Constants.Application.INF))
            {
                var adtPax = paxList.Where(x => x.PTC == Constants.Application.ADT).ToList();
                var infPax = paxList.Where(x => x.PTC == Constants.Application.INF).ToList();

                for (int i = 0; i < infPax.Count; i++)
                {
                    adtPax[i].PaxRefID = infPax[i].PaxID;
                }
            }
            return paxList;
        }

    }
}
