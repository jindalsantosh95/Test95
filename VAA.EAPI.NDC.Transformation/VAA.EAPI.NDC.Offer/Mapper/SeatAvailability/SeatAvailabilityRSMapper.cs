using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;
using Const = VAA.EAPI.NDC.Offer.Domain.Model.VAA;

namespace VAA.EAPI.NDC.Offer.Mapper.SeatAvailability
{
    public static class SeatAvailabilityRSMapper
    {
        public static IATA_SeatAvailabilityRS MapSeatAvailabilityResponse(SeatRS seatAvailabilityResponse, IEnumerable<Customer> paxList, OrderDocument orderDocument, OfferResponse offerDocument, IHostEnvironment _hostenv)
        {
            var dataLists = new DataListsType();
            var datedMarketingSegmentList = new List<DatedMarketingSegmentType>();
            var datedOperatingSegmentList = new List<DatedOperatingSegmentType>();
            var datedOperatingLegList = new List<DatedOperatingLegType>();
            var paxSegmentList = new List<PaxSegmentType>();
            var seatProfileList = new List<SeatProfileType>();
            var serviceDefinitionList = new List<ServiceDefinitionType>();
            var disclosureTypeList = new List<DisclosureType>();
            var alaCarteOfferItemList = new List<ALaCarteOfferItemType>();

            GetDataList(seatAvailabilityResponse, dataLists, paxList, datedMarketingSegmentList, datedOperatingSegmentList, serviceDefinitionList, disclosureTypeList, seatProfileList, paxSegmentList, orderDocument, offerDocument, datedOperatingLegList, _hostenv);

            return new()
            {
                Items = new SeatAvailResponseType[1]
                {
                    new SeatAvailResponseType()
                    {
                     DataLists = dataLists,
                     SeatMap = GetSeatMapList(seatAvailabilityResponse,datedMarketingSegmentList,paxSegmentList, seatProfileList, serviceDefinitionList,alaCarteOfferItemList,paxList, orderDocument,offerDocument),
                     ALaCarteOffer = alaCarteOfferItemList.Any() ? GetALaCarteOffer(seatAvailabilityResponse,dataLists,alaCarteOfferItemList) : null,
                    }
                }
            };
        }

        private static ALaCarteOfferType GetALaCarteOffer(SeatRS seatAvailabilityResponse, DataListsType dataLists, List<ALaCarteOfferItemType> aLaCarteOfferItemList)
        {
            ALaCarteOfferType aLaCarteOffer = new();

            if (seatAvailabilityResponse.SeatAvailabilityResponse.FlightSegmentSeatMap.Any(x => x.Catalogue != null))
            {
                aLaCarteOffer.DisclosureRefID = dataLists.DisclosureList[0].DisclosureID;
                aLaCarteOffer.OfferID = seatAvailabilityResponse.SeatAvailabilityResponse.SeatOfferId;
                aLaCarteOffer.OwnerCode = Const.Constants.OWNERCODE;
                aLaCarteOffer.OfferExpirationTimeLimitDateTime = Convert.ToDateTime(Convert.ToDateTime(seatAvailabilityResponse.SeatAvailabilityResponse.OfferExpirationTimeLimit).ToString(Constants.Application.DatTimeFormatHMS)).ToUniversalTime();
                aLaCarteOffer.OfferExpirationTimeLimitDateTimeSpecified = true;
            }

            aLaCarteOffer.OfferItem = aLaCarteOfferItemList.ToArray();
            return aLaCarteOffer;
        }

        private static SeatMapType[] GetSeatMapList(SeatRS seatAvailabilityResponse, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<Customer> paxList, OrderDocument orderDocument, OfferResponse offerDocument)
        {
            return seatAvailabilityResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.Where(fsm => fsm.Catalogue.Any())
            .Select(fssm => new SeatMapType()
            {
                PaxSegmentRefID = GetSegmentId(fssm.FlightSegment, datedMarketingSegmentList, paxSegmentList),
                CabinCompartment = GetCabinCompartment(fssm, datedMarketingSegmentList, paxSegmentList, seatProfileList, serviceDefinitionList, aLaCarteOfferItemList, paxList, orderDocument, offerDocument)
            }).ToArray();
        }

        private static string GetSegmentId(Segment flightSegment, IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList)
        {
            var dms = datedMarketingSegmentList.First(ss => ss.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && ss.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && ss?.Dep?.AircraftScheduledDateTime.Date == DateTime.Parse(flightSegment?.ScheduledDepartureTime?.LocalDateTime).Date);

            return paxSegmentList.First(x => x.DatedMarketingSegmentRefId == dms.DatedMarketingSegmentId).PaxSegmentID;
        }

        private static CabinCompartmentType[] GetCabinCompartment(FlightSegmentSeatMap flightSegmentSeatMap, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<Customer> paxList, OrderDocument orderDocument, OfferResponse offerDocument)
        {
            return flightSegmentSeatMap.Seatmap.Select(s =>
            {
                List<SeatMap> newSeatMap = new();
                SeatMap upperSeatMap = new();
                SeatMap mainSeatMap = new();

                if (s.SeatRows.Any(x => x.UpperDeck))
                {
                    upperSeatMap.ClassType = s.ClassType;
                    upperSeatMap.Equipment = s.Equipment;
                    upperSeatMap.Location = s.Location;
                    upperSeatMap.SeatRows = s.SeatRows.Where(x => x.UpperDeck).Select(r => r);
                    upperSeatMap.SeatConfiguration = s.SeatConfiguration;
                    newSeatMap.Add(upperSeatMap);

                    mainSeatMap.ClassType = s.ClassType;
                    mainSeatMap.Equipment = s.Equipment;
                    mainSeatMap.Location = s.Location;
                    mainSeatMap.SeatRows = s.SeatRows.Where(x => !x.UpperDeck).Select(r => r);
                    mainSeatMap.SeatConfiguration = s.SeatConfiguration;
                    newSeatMap.Add(mainSeatMap);
                }
                else
                    newSeatMap.Add(s);

                foreach (var c in newSeatMap)
                {
                    var cabin = new CabinCompartmentType();
                    var cabinComponentList = new List<CabinComponentType>();
                    var columnList = new List<string>();
                    var seatRowList = new List<SeatRowType>();
                    var cabinType = new CabinTypeType();
                    var brandID = string.Empty;
                    var seatColumnList = new List<SeatColumnType>();

                    cabinType.CabinTypeCode = string.IsNullOrEmpty(flightSegmentSeatMap.FlightSegment.OperatingCarrier) ? GetCabinCode(c.ClassType) : GetDLCabinCode(c.ClassType);
                    cabinType.CabinTypeName = string.IsNullOrEmpty(flightSegmentSeatMap.FlightSegment.OperatingCarrier) ? GetCabinName(c.ClassType) : GetDLCabinName(c.ClassType);

                    if (flightSegmentSeatMap.FlightSegment.FareFamily != null && (flightSegmentSeatMap.FlightSegment.FareFamily.Contains(cabinType.CabinTypeName.Split(" ").FirstOrDefault()) || cabinType.CabinTypeName.Contains(flightSegmentSeatMap.FlightSegment?.FareFamily)))
                    {
                        cabin.CabinType = cabinType;
                        cabin.DeckCode = c.SeatRows.First().UpperDeck ? DeckCodeType.Upper : DeckCodeType.Main;
                        cabin.DeckCodeSpecified = true;
                        cabin.FirstRowNumber = c.SeatRows.First(x => x.RowNumber != null).RowNumber;
                        cabin.LastRowNumber = c.SeatRows.Last(x => x.RowNumber != null).RowNumber;

                        brandID = string.IsNullOrEmpty(flightSegmentSeatMap.FlightSegment.OperatingCarrier) ? orderDocument.OrderItems != null && offerDocument.Offers == null ? GetBrandID(flightSegmentSeatMap.FlightSegment.FareFamily) : GetBrandIDPrime(flightSegmentSeatMap.FlightSegment.FareFamily) : GetDLBrandID(flightSegmentSeatMap.FlightSegment.FareFamily);

                        foreach (var r in c.SeatRows)
                        {
                            if (r.RowNumber != null)
                            {
                                seatRowList.Add(new SeatRowType()
                                {
                                    RowNumber = r.RowNumber.ToString(),
                                    Seat = GetSeat(r, datedMarketingSegmentList, paxSegmentList, paxList, seatProfileList, serviceDefinitionList, aLaCarteOfferItemList, flightSegmentSeatMap, columnList, brandID, c, orderDocument, offerDocument),
                                    RowCharacteristicCode = GetRowCharacteristics(r)
                                });
                            }
                        }

                        foreach (var clm in columnList)
                        {
                            var seatColumn = clm.Split('_');
                            seatColumnList.Add(new SeatColumnType()
                            {
                                ColumnID = seatColumn[0],
                                SeatCharacteristicCode = new[] { seatColumn.Last() }
                            });
                        }

                        cabin.SeatRow = seatRowList.ToArray();
                        cabin.SeatColumn = seatColumnList.ToArray();
                        int rowNum = 0;

                        foreach (var sr in c.SeatRows)
                        {
                            if (sr.RowNumber == null)
                            {
                                if (sr.SeatColumns.Any(column => column.Toilet))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Lavatory,
                                        ColumnID = GetColumnIDForToilet(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = GetRowNumber(c.SeatRows, rowNum),
                                    };
                                    var positionCode = GetPositionCode(c.SeatRows, rowNum, Convert.ToInt32(cabinComponent.FirstRowNumber));
                                    cabinComponent.PositionCode = positionCode;
                                    cabinComponent.PositionCodeSpecified = true;
                                    cabinComponentList.Add(cabinComponent);
                                }

                                if (sr.SeatColumns.Any(column => column.Galley))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Galley,
                                        ColumnID = GetColumnIDForGalley(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = GetRowNumber(c.SeatRows, rowNum),
                                    };
                                    cabinComponent.PositionCode = GetPositionCode(c.SeatRows, rowNum, Convert.ToInt32(cabinComponent.FirstRowNumber));
                                    cabinComponent.PositionCodeSpecified = true;
                                    cabinComponentList.Add(cabinComponent);
                                }

                                if (sr.SeatColumns.Any(column => column.Closet))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Closet,
                                        ColumnID = GetColumnIDForCloset(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = GetRowNumber(c.SeatRows, rowNum),

                                    };
                                    cabinComponent.PositionCode = GetPositionCode(c.SeatRows, rowNum, Convert.ToInt32(cabinComponent.FirstRowNumber));
                                    cabinComponent.PositionCodeSpecified = true;
                                    cabinComponentList.Add(cabinComponent);
                                    cabinComponentList.Add(cabinComponent);
                                }

                            }
                            else
                            {
                                if (sr.SeatColumns.Any(column => column.Toilet))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Lavatory,
                                        ColumnID = GetColumnIDForToilet(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = c.SeatRows.First().RowNumber.ToString(),
                                        LastRowNumber = c.SeatRows.Last().RowNumber.ToString(),
                                    };
                                    cabinComponentList.Add(cabinComponent);
                                }
                                else if (sr.SeatColumns.Any(column => column.Galley))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Galley,
                                        ColumnID = GetColumnIDForGalley(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = c.SeatRows.First().RowNumber.ToString(),
                                        LastRowNumber = c.SeatRows.Last().RowNumber.ToString(),
                                    };
                                    cabinComponentList.Add(cabinComponent);
                                }
                                else if (sr.SeatColumns.Any(column => column.Closet))
                                {
                                    var cabinComponent = new CabinComponentType()
                                    {
                                        CabinComponentTypeCode = Const.Constants.Closet,
                                        ColumnID = GetColumnIDForCloset(sr.SeatColumns, c.SeatConfiguration),
                                        FirstRowNumber = c.SeatRows.First().RowNumber.ToString(),
                                        LastRowNumber = c.SeatRows.Last().RowNumber.ToString(),
                                    };
                                    cabinComponentList.Add(cabinComponent);
                                }

                            }

                            rowNum++;
                        }

                        cabin.CabinComponent = cabinComponentList.ToArray();

                        return cabin;
                    }
                }
                return null;
            }).ToArray();
        }

        private static AvailableSeatType[] GetSeat(SeatRow r, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, IEnumerable<Customer> paxList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, Infrastructure.Model.SeatMS.FlightSegmentSeatMap flightSegmentSeatMap, List<string> columnList, string brandID, SeatMap seatMap, OrderDocument orderDocument, OfferResponse offerDocument)
        {
            return r.SeatColumns.Select(clm =>
            {
                bool exitSeatAvailability = true;
                var columnID = string.Empty;
                if ((!string.IsNullOrEmpty(clm.OccupationCode)))
                {
                    if (clm.Exit && paxList.Any(p => p.Ptc == Const.Constants.CHD || p.Ptc == Const.Constants.INF))
                        exitSeatAvailability = false;

                    columnID = clm.SeatIdentifer.Substring(clm.SeatIdentifer.Length - 1) + "_" + (clm.Pos == "C" ? "MS" : clm.Pos);

                    if (columnList.Any() && !columnList.Any(c => c == columnID))
                        columnList.Add(columnID);

                    else if (columnList.Count == 0)
                        columnList.Add(columnID);

                    if (brandID != null)
                    {
                        if (exitSeatAvailability && clm.Available && HasApplicableCatalogue(clm, flightSegmentSeatMap.Catalogue) && clm.OccupationCode == Const.Constants.OccupationCodeF)
                        {       //Seat is free "F"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray(),
                                OfferItemRefID = CreateALaCarteOfferItemListUsingRefID(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.BrandId, clm.Eligibility, orderDocument, offerDocument),
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Available && HasApplicableCatalogue(clm, flightSegmentSeatMap.Catalogue) && clm.OccupationCode == Const.Constants.OccupationCodeZ)
                        {      //Seat is blocked but available is true
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = Const.Constants.OccupationCodeF,
                                SeatCharacteristicCode = clm.CharacteristicCodes != null ? clm.CharacteristicCodes.ToArray() : null,
                                OfferItemRefID = CreateALaCarteOfferItemListUsingRefID(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.BrandId, clm.Eligibility, orderDocument, offerDocument),
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Blocked)
                        {      //Seat blocked for other reasons "Z"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer?[(clm.SeatIdentifer.Length - 1)..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray()
                            };
                        }
                        else
                        {     //Seat is occupied due to exitSeatAvailability "O"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = Const.Constants.OccupationCodeO,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray()
                            };
                        }
                    }
                    else
                    {
                        if (exitSeatAvailability && clm.Available && clm.OccupationCode == Const.Constants.OccupationCodeF)
                        {       //Seat is free "F"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray(),
                                OfferItemRefID = CreateALaCarteOfferItemListUsingRefID(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.BrandId, clm.Eligibility, orderDocument, offerDocument),
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Available && clm.OccupationCode == Const.Constants.OccupationCodeZ)
                        {      //Seat is blocked but available is true
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes != null ? clm.CharacteristicCodes.ToArray() : null,
                                OfferItemRefID = CreateALaCarteOfferItemListUsingRefID(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.BrandId, clm.Eligibility, orderDocument, offerDocument),
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Blocked)
                        {      //Seat blocked for other reasons "Z"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray()
                            };
                        }
                        else
                        {     //Seat is occupied due to exitSeatAvailability "O"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = Const.Constants.OccupationCodeO,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray()
                            };
                        }
                    }
                }
                else if (string.IsNullOrEmpty(clm.OccupationCode) && clm.Missing)
                {
                    return new AvailableSeatType()
                    {
                        ColumnID = seatMap.SeatConfiguration[Convert.ToInt32(clm.Index)].ToString(),
                        RowNumber = r.RowNumber,
                        OccupationStatusCode = Const.Constants.OccupationCodeQ,
                        SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray()
                    };
                }
                else
                    return null;
            }).ToArray();
        }

        private static string[] CreateALaCarteOfferItemListUsingRefID(List<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<Customer> paxList, IEnumerable<ServiceDefinitionType> serviceDefinitionList, FlightSegmentSeatMap flightSegmentSeatMap, IEnumerable<string> catalogueRefIds, string seatBrandId, IEnumerable<SeatEligibility> eligibility, OrderDocument orderDocument, OfferResponse offerDocument)
        {
            if (catalogueRefIds == null || flightSegmentSeatMap?.Catalogue == null)
                return Array.Empty<string>();

            return catalogueRefIds
                .Where(catalogueRefId => flightSegmentSeatMap.Catalogue.Any(catalogue => catalogue.CatalogueId == catalogueRefId && BrandIdsMatch(seatBrandId, catalogue.BrandId)))
                .Select(catalogueRefId => GetALaCarteOfferItemListUsingCatalogue(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, catalogueRefId, eligibility, orderDocument, offerDocument))
                .ToArray();
        }

        private static bool HasApplicableCatalogue(SeatColumn seatColumn, IEnumerable<VAA.EAPI.Domain.Virgin.Models.Products.Seat.FlightSegmentSeatMapCatalogue> catalogues)
        {
            return seatColumn.CatalogueRefIds?.Any(catalogueRefId =>
                catalogues?.Any(catalogue => catalogue.CatalogueId == catalogueRefId && BrandIdsMatch(seatColumn.BrandId, catalogue.BrandId)) == true) == true;
        }

        private static bool BrandIdsMatch(string seatBrandId, string catalogueBrandId)
        {
            if (catalogueBrandId is null || catalogueBrandId.Length == 0) return true;
            if (seatBrandId is null || seatBrandId.Length == 0) return false;

            var seatBrandIds = seatBrandId.Split(new[] { ',', ';', '|', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var catalogueBrandIds = catalogueBrandId.Split(new[] { ',', ';', '|', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return seatBrandIds.Any(seatBrand => catalogueBrandIds.Any(catalogueBrand => string.Equals(seatBrand, catalogueBrand, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool CatalogueAppliesToAnySeat(EAPI.Domain.Virgin.Models.Products.Seat.FlightSegmentSeatMapCatalogue catalogue, IEnumerable<SeatMap> seatMaps)
        {
            return seatMaps.SelectMany(seatMap => seatMap.SeatRows)
                .SelectMany(seatRow => seatRow.SeatColumns)
                .Any(seatColumn => seatColumn.CatalogueRefIds?.Contains(catalogue.CatalogueId) == true && BrandIdsMatch(seatColumn.BrandId, catalogue.BrandId));
        }

        private static string GetALaCarteOfferItemListUsingCatalogue(List<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<Customer> paxList, IEnumerable<ServiceDefinitionType> serviceDefinitionList, FlightSegmentSeatMap flightSegmentSeatMap, string catalogueRefId, IEnumerable<SeatEligibility> eligibility, OrderDocument orderDocument, OfferResponse offerDocument)
        {
            bool exitSeatAvailability = true;

            foreach (var sc in flightSegmentSeatMap.Catalogue)
            {
                if (sc.ProductName.Contains("Exit") && paxList.Any(p => p.Ptc == Const.Constants.CHD || p.Ptc == Const.Constants.INF || p.Ptc == Const.Constants.CNN))
                    exitSeatAvailability = false;

                if (exitSeatAvailability && sc.CatalogueId == catalogueRefId && !aLaCarteOfferItemList.Any(a => a.OfferItemID == GetTrimmedCatalogueID(catalogueRefId)))
                {
                    aLaCarteOfferItemList.Add(new ALaCarteOfferItemType()
                    {
                        OfferItemID = GetTrimmedCatalogueID(sc.CatalogueId),
                        UnitPrice = new PriceType()
                        {
                            TotalAmount = new AmountType()
                            {
                                CurCode = sc.SeatPrice?.BaseCurrency,
                                Value = Convert.ToDecimal(sc.SeatPrice?.MonetaryAmount)
                            }
                        },
                        Service = new ALaCarteServiceType()
                        {
                            ServiceID = IdCreator.CreateServiceID(aLaCarteOfferItemList.Count() + 1),
                            ServiceDefinitionRefID = GetServiceDefinitionRefID(sc, serviceDefinitionList)
                        },
                        Eligibility = new EligibilityType()
                        {
                            PaxRefID = offerDocument.DataList != null ? offerDocument.DataList?.CustomerDetails.Where(p => p.Ptc != Constants.Application.InfantPTCCode).Select(pax => pax.CustId).ToArray() : GetEligiblePaxList(eligibility, orderDocument, flightSegmentSeatMap.FlightSegment, catalogueRefId),
                            OfferFlightAssociations = new OfferFlightAssociationsType()
                            {
                                Item = new PaxSegmentRefListType()
                                {
                                    PaxSegmentRefID = new[] { GetSegmentId(flightSegmentSeatMap.FlightSegment, datedMarketingSegmentList, paxSegmentList) }
                                },
                                ItemElementName = ItemChoiceType.PaxSegmentReferences
                            }
                        }
                    });
                }
            };

            return GetTrimmedCatalogueID(catalogueRefId);
        }

        private static string[] GetEligiblePaxList(IEnumerable<SeatEligibility> eligibility, OrderDocument orderDocument, Segment flightSegment, string catalogueRefId)
        {
            var paxRefIDList = new List<string>();
            var paxList = new List<Customer>();
            var seatedPax = new List<Customer>();
            IEnumerable<OrderItems> seatOrderItems = null;

            var seg = flightSegment.SegmentId.Split("0").Last();
            paxList.AddRange(orderDocument.DataList.CustomerDetails);

            if (orderDocument.OrderItems.Any(x => x.Services.Any(y => y.Product != null && y.Product.ProductType != ProductTypes.SeatEnum && y.CurrentSeatAssignment != null)))
            {
                return [.. paxList.Select(p => p.CustId)];
            }
            else
            {
                seatOrderItems = orderDocument.OrderItems.Where(oi => oi.Services.FirstOrDefault()?.Product?.ProductType == ProductTypes.SeatEnum && oi.Services.FirstOrDefault()?.SegmentRefIds?.FirstOrDefault()?.Contains(seg) == true);

                if (seatOrderItems.Any())
                {
                    foreach (var oi in seatOrderItems)
                    {
                        seatedPax.Add(orderDocument.DataList.CustomerDetails.FirstOrDefault(cust => cust.CustId == oi.Services.First().ServiceBeneficiaries.First() && oi.Services.First().SegmentRefIds.FirstOrDefault().Contains(seg)));
                    }
                    foreach (var eligiblePax in eligibility)
                    {
                        var found = seatedPax.Exists(p => p.LastNameNumber + "-" + p.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault());
                        if (!found)
                            paxRefIDList.Add(paxList.Where(pax => pax.LastNameNumber + "-" + pax.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault())?.FirstOrDefault()?.CustId);
                    }
                }
                else
                {
                    foreach (var eligiblePax in eligibility)
                    {
                        if (eligiblePax.CatalogueRefId == catalogueRefId)
                            paxRefIDList.Add(paxList.Where(pax => pax.LastNameNumber + "-" + pax.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault())?.FirstOrDefault()?.CustId);
                    }
                }
            }
            return [.. paxRefIDList];
        }

        private static string GetCabinCode(string classType)
        {
            return classType switch
            {
                "C" => "2",
                "J" => "2",
                "W" => "4",
                "Y" => "5",
                _ => string.Empty
            };
        }

        private static string GetCabinName(string classType)
        {
            return classType switch
            {
                "C" => "Upper Class",
                "J" => "Upper Class",
                "W" => "Premium Economy",
                "Y" => "Economy",
                _ => string.Empty
            };
        }

        private static string GetDLCabinCode(string classType)
        {
            return classType switch
            {
                "C" => "2",
                "D" => "2",
                "J" => "2",
                "F" => "4",
                "P" => "4",
                "A" => "4",
                "G" => "5",
                "S" => "5",
                "W" => "5",
                "Y" => "5",
                "E" => "5",
                _ => string.Empty
            };
        }

        private static string GetDLCabinName(string classType)
        {
            return classType switch
            {
                "C" => "Upper Class",
                "D" => "Upper Class",
                "J" => "Upper Class",
                "F" => "Premium",
                "P" => "Premium",
                "A" => "Premium",
                "W" => "Economy",
                "G" => "Economy",
                "Y" => "Economy",
                "S" => "Economy",
                "E" => "Economy",
                _ => string.Empty
            };
        }

        private static string GetPaxSegmentId(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList)
        {
            if (datedMarketingSegmentList.Any(x => x.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime)).ToUniversalTime())))
            {
                var paxId = paxSegmentList.Select(x => x.PaxSegmentID).FirstOrDefault();
                return paxId;
            }
            else
            {
                var paxSegmentType = CreateSeatPaxSegmentType(paxSegmentList);
                paxSegmentList.Add(paxSegmentType);
                return paxSegmentType.PaxSegmentID;
            }
        }

        private static DataListsType GetDataList(SeatRS seatAvailabilityResponse, DataListsType dataLists, IEnumerable<Customer> paxList, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<ServiceDefinitionType> serviceDefinitionList, List<DisclosureType> seatDisclosureTypeList, List<SeatProfileType> seatProfileList, List<PaxSegmentType> paxSegmentList, OrderDocument orderDocument, OfferResponse offerDocument, List<DatedOperatingLegType> datedOperatingLegList, IHostEnvironment hostEnv)
        {

            dataLists.PaxList = paxList.Select(pL => new PaxType()
            {
                PTC = pL?.Ptc == Const.Constants.CNNPTCCode ? Const.Constants.ChildPTCCode : pL?.Ptc,
                PaxID = pL?.CustId,

            }).ToArray();


            var segmentList = (orderDocument?.OrderItems == null && offerDocument.Offers.Any()) ? GetFlightSegments(offerDocument) : GetFlightSegmentsOrderDoc(orderDocument);

            foreach (var segment in segmentList)
            {
                {
                    GetPaxSegmentId(segment, datedMarketingSegmentList, paxSegmentList);
                    GetDatedMarketingSegmentList(segment, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList);
                }
            }

            var exitSeatServiceDescription = GetExitSeatServiceDescription(hostEnv);
            var nonExitSeatServiceDescription = GetNonExitSeatServiceDescription(hostEnv);
            var brandID = string.Empty;
            if (seatAvailabilityResponse.SeatAvailabilityResponse != null)
            {
                foreach (var fsm in seatAvailabilityResponse.SeatAvailabilityResponse.FlightSegmentSeatMap)
                {
                    if (fsm.Catalogue.Any())
                    {
                        if (orderDocument.OrderItems != null && offerDocument.Offers == null)
                        {
                            fsm.FlightSegment.FareFamily = (from pc in orderDocument?.DataList.PriceClass
                                                            where pc.Id == orderDocument.OrderItems.FirstOrDefault().Services.FirstOrDefault().PriceClassRefIds.FirstOrDefault()
                                                            select pc.ProductName).First();
                            brandID = string.IsNullOrEmpty(fsm.FlightSegment.OperatingCarrier) ? GetBrandID(fsm.FlightSegment.FareFamily) : GetDLBrandID(fsm.FlightSegment.FareFamily);
                        }
                        else
                        {
                            fsm.FlightSegment.FareFamily = (from o in offerDocument.Offers
                                                            from oi in o.OfferItems
                                                            from s in oi.Services
                                                            from osegId in s.ProductSegmentAssociation.OfferSegmentRefIds
                                                            join oseg in offerDocument.DataList.OfferSegments
                                                            on osegId equals oseg.OfferSegmentId
                                                            join fseg in offerDocument.DataList.FlightSegments
                                                            on oseg.FlightSegmentRefId equals fseg.SegmentId
                                                            join pc in offerDocument.DataList.PriceClass
                                                            on s.PriceClassRefIds.First() equals pc.Id
                                                            where fsm.FlightSegment.DestinationAirportCode == fseg.DestinationAirportCode
                                                            && fsm.FlightSegment.OperatingFlightNumber == fseg.OperatingFlightNumber
                                                            && fsm.FlightSegment.MarketingCarrier == fseg.MarketingCarrier
                                                            select pc).First().ProductName;

                            brandID = string.IsNullOrEmpty(fsm.FlightSegment.OperatingCarrier) ? GetBrandIDPrime(fsm.FlightSegment.FareFamily) : GetDLBrandID(fsm.FlightSegment.FareFamily);
                        }


                        foreach (var fsc in fsm.Catalogue)
                        {
                            if (CatalogueAppliesToAnySeat(fsc, fsm.Seatmap))
                            {
                                bool allowExitSeat = true;

                                if (dataLists.PaxList.Any(p => p.PTC == Const.Constants.CHD || p.PTC == Const.Constants.INF) && (fsc.ProductId == Const.Constants.ExitSeatProductId || fsc.ProductId == Const.Constants.ExitReduceLegroomSeatProductID)) allowExitSeat = false;

                                if (allowExitSeat && !serviceDefinitionList.Any(sd => sd.Name == fsc.ProductName))
                                {
                                    serviceDefinitionList.Add(new ServiceDefinitionType()
                                    {
                                        ServiceDefinitionID = IdCreator.CreateServiceDefinitionID(serviceDefinitionList.Count()),
                                        OwnerCode = Const.Constants.OwnerCode,
                                        Name = fsc.ProductName,
                                        ServiceCode = fsc?.ProductId,
                                        RFIC = fsc.ReasonForIssuanceCode,
                                        RFISC = fsc.ReasonForIssuanceSubCode
                                    });
                                }

                            }

                            if (fsc.ProductId != Const.Constants.ExitSeatProductId && seatDisclosureTypeList.Count() == 0)
                            {
                                seatDisclosureTypeList.Add(new DisclosureType()
                                {
                                    Desc = nonExitSeatServiceDescription.ToArray(),
                                    DisclosureID = IdCreator.CreateSeatDiscloureTypeID(seatDisclosureTypeList.Count())
                                });
                                seatDisclosureTypeList.Distinct();
                            }
                            if ((fsc.ProductId == Const.Constants.ExitSeatProductId || fsc.ProductId == Const.Constants.ExitReduceLegroomSeatProductID) && seatProfileList.Count() == 0)
                            {
                                seatProfileList.Add(new SeatProfileType()
                                {
                                    SeatProfileID = IdCreator.CreateSeatProfileTypeID(seatProfileList.Count()),
                                    MarketingInfo = exitSeatServiceDescription.ToArray()
                                });
                                seatProfileList.Distinct();
                            }
                        }
                    }

                }
            }
            dataLists.PaxList = GetSeatPaxList(dataLists.PaxList.ToList());
            dataLists.DatedMarketingSegmentList = datedMarketingSegmentList.ToArray();
            dataLists.DatedOperatingSegmentList = datedOperatingSegmentList.ToArray();
            dataLists.PaxSegmentList = paxSegmentList.ToArray();
            dataLists.SeatProfileList = seatProfileList.Any() ? seatProfileList.ToArray() : null;
            dataLists.ServiceDefinitionList = serviceDefinitionList.Any() ? serviceDefinitionList.ToArray() : null;
            dataLists.DisclosureList = seatDisclosureTypeList.Any() ? seatDisclosureTypeList.ToArray() : null;
            return dataLists;
        }

        private static IEnumerable<VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment> GetFlightSegmentsOrderDoc(OrderDocument orderDocument)
        {
            return orderDocument?.DataList.SegmentDetails.Where(x => (x.MarketingCarrier == Constants.Application.OwnerCode && x.OperatingCarrier is null) || (x.OperatingCarrier == "DL"));
        }

        private static PaxType[] GetSeatPaxList(List<PaxType> paxList)
        {
            if (paxList.Any(x => x.PTC == Const.Constants.INF))
            {
                var adtPax = paxList.Where(x => x.PTC == Const.Constants.ADT).Select(x => x).ToList();
                var infPax = paxList.Where(x => x.PTC == Const.Constants.INF).Select(x => x).ToList();

                for (int i = 0; i < infPax.Count; i++)
                {
                    adtPax[i].PaxRefID = infPax[i].PaxID;
                }
            }

            return paxList.ToArray();
        }

        private static string GetBrandID(string fareFamily)
        {
            return fareFamily switch
            {
                "Economy Classic" => Const.Constants.EconomyBrandID,
                "Economy Classic Flex" => Const.Constants.EconomyBrandID,
                "Economy Light" => Const.Constants.EconomyBrandID,
                "Economy Delight" => Const.Constants.DelightBrandID,
                "Economy Delight Flex" => Const.Constants.DelightBrandID,
                "Premium" => Const.Constants.PremEconomyBrandID,
                "Premium Flex" => Const.Constants.PremEconomyBrandID,
                "Upper Class" => Const.Constants.UpperBrandID,
                "Upper Class Flex" => Const.Constants.UpperBrandID,
                _ => string.Empty
            };
        }

        private static string GetBrandIDPrime(string fareFamily)
        {
            return fareFamily switch
            {
                "Economy Classic" => Const.Constants.EconomyBrandID,
                "Economy Classic Flex" => Const.Constants.EconomyBrandID,
                "Economy Light" => Const.Constants.EconomyBrandID,
                "Economy Delight" => Const.Constants.DelightBrandID,
                "Economy Delight Flex" => Const.Constants.DelightBrandID,
                "Premium" => Const.Constants.PremEconomyBrandID,
                "Premium Flex" => Const.Constants.PremEconomyBrandID,
                "Upper Class" => Const.Constants.UpperBrandID,
                "Upper Class Flex" => Const.Constants.UpperBrandID,
                _ => string.Empty
            };
        }
        private static string GetDLBrandID(string fareFamily)
        {
            return fareFamily switch
            {
                "Economy Classic" => Const.Constants.EconomyDLBrandID,
                "Economy Classic Flex" => Const.Constants.EconomyDLBrandID,
                "Economy Light" => Const.Constants.EconomyDLBrandID,
                "Economy Delight" => Const.Constants.EconomyDelightDLBrandID,
                "Economy Delight Flex" => Const.Constants.EconomyDelightDLBrandID,
                "Premium" => Const.Constants.PremiumDLBrandID,
                "Premium Flex" => Const.Constants.PremiumDLBrandID,
                "Upper Class" => Const.Constants.UpperDLBrandID,
                "Upper Class Flex" => Const.Constants.UpperDLBrandID,
                _ => string.Empty
            };
        }

        private static List<DescType> GetExitSeatServiceDescription(IHostEnvironment hostEnv)
        {
            Const.Constants.ExitServiceDiscriptionList.LoadXml(File.ReadAllText(hostEnv.ContentRootPath + "\\Resources\\ExitSeatsServiceDescription.xml")); ;
            var nodeDescription = Const.Constants.ExitServiceDiscriptionList.DocumentElement.SelectNodes("/ServiceDefinition/Description");
            var descriptionList = new List<DescType>();

            if (nodeDescription != null)
            {
                for (int i = 0; i < nodeDescription.Count; i++)
                {
                    var description = new DescType();
                    description.DescText = nodeDescription[i].ChildNodes[1]?.InnerText;
                    description.URL = nodeDescription[i].ChildNodes[2]?.InnerText;
                    descriptionList.Add(description);
                }
            }

            return descriptionList;
        }

        private static List<DescType> GetNonExitSeatServiceDescription(IHostEnvironment hostEnv)
        {
            Const.Constants.NonExitServiceDiscriptionList.LoadXml(File.ReadAllText(hostEnv.ContentRootPath + "\\Resources\\NotExitSeatServiceDescription.xml"));
            XmlDocument doc = Const.Constants.NonExitServiceDiscriptionList;

            var nodeDescription = doc.DocumentElement.SelectNodes("/ServiceDefinition/Description");
            var descriptionList = new List<DescType>();

            if (nodeDescription != null)
            {
                for (int i = 0; i < nodeDescription.Count; i++)
                {
                    var description = new DescType();
                    description.DescText = nodeDescription[i].ChildNodes[1]?.InnerText;
                    description.URL = nodeDescription[i].ChildNodes[2]?.InnerText;
                    descriptionList.Add(description);
                }
            }

            return descriptionList;
        }

        private static void GetDatedMarketingSegmentList(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            if (!datedMarketingSegmentList.Any(x => x.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime()))))
            {
                datedMarketingSegmentList.Add(CreateDatedMarketingSegment(flightSegment, datedMarketingSegmentList, datedOperatingSegmentList, datedOperatingLegList));
            }
        }

        private static DatedMarketingSegmentType CreateDatedMarketingSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            DatedMarketingSegmentType datedMarketingSegmentType = new();

            datedMarketingSegmentType.DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count());
            datedMarketingSegmentType.DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(flightSegment, datedMarketingSegmentList.Count(), datedOperatingSegmentList, datedOperatingLegList);
            datedMarketingSegmentType.Dep = new TransportPointType()
            {
                IATA_LocationCode = flightSegment.OriginAirportCode,
                AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime),
                AircraftScheduledDateTimeSpecified = true
            };
            datedMarketingSegmentType.Arrival = new TransportPointType()
            {
                IATA_LocationCode = flightSegment.DestinationAirportCode,
                AircraftScheduledDateTime = Convert.ToDateTime(flightSegment.ScheduledArrivalTime.LocalDateTime),
                AircraftScheduledDateTimeSpecified = true
            };
            datedMarketingSegmentType.CarrierDesigCode = flightSegment.MarketingCarrier;
            datedMarketingSegmentType.CarrierName = flightSegment.MarketingCarrier;
            datedMarketingSegmentType.MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber;

            return datedMarketingSegmentType;
        }

        private static string GetDatedOperatingSegmentID(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, int count, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedOperatingSegment = CreateDatedOperatingSegment(flightSegment, count, datedOperatingLegList);
            datedOperatingSegmentList.Add(datedOperatingSegment);
            return datedOperatingSegment.DatedOperatingSegmentId;
        }

        private static DatedOperatingSegmentType CreateDatedOperatingSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, int count, List<DatedOperatingLegType> datedOperatingLegList)
        {
            DatedOperatingSegmentType datedOperatingSegmentList = new();

            datedOperatingSegmentList.DatedOperatingSegmentId = IdCreator.CreateDatedOperatingSegmentID(count);
            datedOperatingSegmentList.CarrierDesigCode = flightSegment.OperatingCarrier == null ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier;
            datedOperatingSegmentList.CarrierName = flightSegment.OperatingCarrier == null ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier;
            datedOperatingSegmentList.OperatingCarrierFlightNumberText = flightSegment.OperatingFlightNumber == null ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber;
            datedOperatingSegmentList.Duration = GetDuration(TimeSpan.Parse(flightSegment.FlightDuration));

            return datedOperatingSegmentList;
        }

        private static string PostSaleSeatCreateDatedOperatingLegType(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Legs leg, List<DatedOperatingLegType> datedOperatingLegList)
        {
            var datedOperatingLeg = new DatedOperatingLegType()
            {
                Arrival = new TransportPointType
                {
                    IATA_LocationCode = leg?.DestinationAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg?.ScheduledArrivalTime?.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                Dep = new TransportPointType
                {
                    IATA_LocationCode = leg?.OriginAirportCode,
                    AircraftScheduledDateTime = Convert.ToDateTime(leg?.ScheduledDepartureTime?.LocalDateTime),
                    AircraftScheduledDateTimeSpecified = true
                },
                CarrierAircraftType = string.IsNullOrEmpty(leg?.CarrierAircraftTypeCode) ? null : new CarrierAircraftTypeType()
                {
                    CarrierAircraftTypeCode = leg.CarrierAircraftTypeCode,
                    CarrierAircraftTypeName = leg.CarrierAircraftTypeName,
                },
                DatedOperatingLegID = IdCreator.CreateDatedOperatingLegID(datedOperatingLegList.Count),
                OnGroundDuration = (leg?.LegDuration != null && leg.LegDuration != default) ? leg.LegDuration : null

            };

            datedOperatingLegList.Add(datedOperatingLeg);

            return datedOperatingLeg.DatedOperatingLegID;

        }

        public static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Const.Constants.PT, timeSpan.Days * 24 + timeSpan.Hours, Const.Constants.H, timeSpan.Minutes, Const.Constants.M, timeSpan.Seconds, Const.Constants.S);
        }

        private static PaxSegmentType CreateSeatPaxSegmentType(List<PaxSegmentType> paxSegmentList)
        {
            var paxSegmentType = new PaxSegmentType()
            {
                PaxSegmentID = IdCreator.CreatePaxSegmentID(paxSegmentList.Count),
                DatedMarketingSegmentRefId = IdCreator.CreateDatedMarketingSegmentID(paxSegmentList.Count)
            };

            return paxSegmentType;
        }

        public static IEnumerable<VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment> GetFlightSegments(OfferResponse offerDoc)
        {
            return offerDoc.DataList.FlightSegments.Select((seg, segNumber) => new VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment
            {
                MarketingCarrier = seg?.MarketingCarrier,
                MarketingFlightNumber = seg?.MarketingFlightNumber,
                OperatingCarrier = seg?.OperatingCarrier,
                OperatingFlightNumber = seg?.OperatingFlightNumber,
                OriginAirportCode = seg?.OriginAirportCode,
                DestinationAirportCode = seg?.DestinationAirportCode,
                SegmentNumber = segNumber.ToString("00"),
                ClassOfService = offerDoc.DataList.OfferSegments.First(oS => oS.FlightSegmentRefId == seg.SegmentId).ClassOfService,
                FareFamily = seg?.FareFamily,
                Legs = seg.Legs,
                FlightDuration = seg?.FlightDuration,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = seg.ScheduledArrivalTime.LocalDateTime,
                },
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = seg.ScheduledDepartureTime.LocalDateTime,
                }
            });
        }

        public static string[] GetColumnIDForToilet(IEnumerable<SeatColumn> columnList, string seatConfig)
        {
            List<string> columnIdList = new();
            var seatConfigLength = seatConfig.Length;
            foreach (var col in columnList)
            {
                if (seatConfigLength > col.Index && col.Toilet)
                {
                    var coulmnID = seatConfig.Substring(col.Index, 1);
                    columnIdList.Add(coulmnID);
                }

                else if (seatConfigLength == col.Index && col.Toilet)
                {
                    var coulmnID = seatConfig.Substring(col.Index);
                    columnIdList.Add(coulmnID);
                }
            };

            return columnIdList.ToArray();
        }

        public static string[] GetColumnIDForGalley(IEnumerable<SeatColumn> columnList, string seatConfig)
        {
            List<string> columnIdList = new();
            var seatConfigLength = seatConfig.Length;
            foreach (var col in columnList)
            {
                if (seatConfigLength > col.Index && col.Galley)
                {
                    var columnID = seatConfig.Substring(col.Index, 1);
                    columnIdList.Add(columnID);
                }

                else if (seatConfigLength == col.Index && col.Galley)
                {
                    var columnID = seatConfig.Substring(col.Index);
                    columnIdList.Add(columnID);
                }
            };

            return columnIdList.ToArray();
        }

        public static string[] GetColumnIDForCloset(IEnumerable<SeatColumn> columnList, string seatConfig)
        {
            List<string> columnIdList = new();
            var seatConfigLength = seatConfig.Length;
            foreach (var col in columnList)
            {
                if (seatConfigLength > col.Index && col.Closet)
                {
                    var columnID = seatConfig.Substring(col.Index, 1);
                    columnIdList.Add(columnID);
                }

                else if (seatConfigLength == col.Index && col.Closet)
                {
                    var columnID = seatConfig.Substring(col.Index);
                    columnIdList.Add(columnID);
                }
            };

            return columnIdList.ToArray();
        }

        public static string GetRowNumber(IEnumerable<SeatRow> rowList, int rowNumber)
        {
            var firstRowNumber = string.Empty;
            var seatRowWithRowNumber = rowList.Select((item, index) => new { item, index }).Where(x => x.item.RowNumber != null).Select(x => x.index).ToArray();

            int firstSeatRowIndex = seatRowWithRowNumber[0];
            int lastSeatRowIndex = seatRowWithRowNumber.Last();

            if (rowNumber < firstSeatRowIndex)
            {
                firstRowNumber = rowList.ElementAt(firstSeatRowIndex).RowNumber.ToString();
            }
            else if (rowList.ElementAt(rowNumber - 1).RowNumber != null)
            {
                firstRowNumber = rowList.ElementAt(seatRowWithRowNumber.Where(x => x < rowNumber).Last()).RowNumber.ToString();
            }
            else if ((rowList.ElementAt(rowNumber - 1).RowNumber == null && rowList.Count() > rowNumber + 1) || rowNumber > lastSeatRowIndex)
            {
                firstRowNumber = rowList.ElementAt(lastSeatRowIndex).RowNumber.ToString();
            }

            return firstRowNumber;
        }

        public static ComponentPositionCodeType GetPositionCode(IEnumerable<SeatRow> rowList, int rowNumber, int firstRowNum)
        {
            var positionCode = new ComponentPositionCodeType();
            var indexOfFirstRowNum = rowList.Select((item, index) => new { item, index }).Where(x => x.item.RowNumber == firstRowNum.ToString()).Select(x => x.index).First();

            if (rowNumber > indexOfFirstRowNum)
            {
                positionCode = ComponentPositionCodeType.After;
            }
            else if (rowNumber < indexOfFirstRowNum)
            {
                positionCode = ComponentPositionCodeType.Before;
            }

            return positionCode;
        }

        private static string GetTrimmedCatalogueID(string catalogueId)
        {
            var seatOfferIDMaxLength = ConfigReader.Get<int>(Const.Constants.seatOfferIDMaxLength);
            if (catalogueId.Length > seatOfferIDMaxLength)
            {
                int start = catalogueId.Length - seatOfferIDMaxLength;
                catalogueId = catalogueId.Substring(start, seatOfferIDMaxLength);

                if (catalogueId.Substring(0, 1) == "-")
                    catalogueId = catalogueId.Substring(1);
            }

            return catalogueId;
        }

        public static string[] GetRowCharacteristics(SeatRow row)
        {
            var rowCharacteristics = new List<string>();
            if (row.OverWing)
            {
                rowCharacteristics.Add("K");
            }
            else
            {
                rowCharacteristics.Add("X");
                if (row.ExitOnLeft)
                {
                    rowCharacteristics.Add("EL");
                    if (row.ExitOnRight)
                    {
                        rowCharacteristics.Add("ER");
                    }
                }
                else if (row.ExitOnRight)
                {
                    rowCharacteristics.Add("ER");
                }
            }

            return rowCharacteristics.ToArray();
        }

        private static string GetServiceDefinitionRefID(EAPI.Domain.Virgin.Models.Products.Seat.FlightSegmentSeatMapCatalogue catalogue, IEnumerable<ServiceDefinitionType> serviceDefinitionList)
        {
            var serviceDefinitionRefID = string.Empty;
            foreach (var serviceDef in serviceDefinitionList)
            {
                if ((catalogue.ProductName == serviceDef.Name) && (catalogue.ReasonForIssuanceCode == serviceDef.RFIC || catalogue.ReasonForIssuanceSubCode == serviceDef.RFISC))
                {
                    serviceDefinitionRefID = serviceDef.ServiceDefinitionID;
                }
            };

            return serviceDefinitionRefID;
        }

    }
}