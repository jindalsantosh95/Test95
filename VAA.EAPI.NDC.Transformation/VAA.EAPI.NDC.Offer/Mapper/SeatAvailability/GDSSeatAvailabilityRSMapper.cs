using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using Const = VAA.EAPI.NDC.Offer.Domain.Model.VAA;

namespace VAA.EAPI.NDC.Offer.Mapper.SeatAvailability
{
    public class GDSSeatAvailabilityRSMapper
    {
        public static IATA_SeatAvailabilityRS MapSeatAvailaibilityResponse(SeatMSAvailabilityResponse seatAvailabilityResponse, OrderSearchResponse orderSearchResponse, List<PaxType> paxList, IEnumerable<EAPI.Domain.Entities.Common.NDCWarning> warningList, IHostEnvironment _hostenv)
        {
            var dataLists = new DataListsType();
            var datedMarketingSegmentList = new List<DatedMarketingSegmentType>();
            var datedOperatingSegmentList = new List<DatedOperatingSegmentType>();
            var paxSegmentList = new List<PaxSegmentType>();
            var seatProfileList = new List<SeatProfileType>();
            var serviceDefinitionList = new List<ServiceDefinitionType>();
            var disclosureTypeList = new List<DisclosureType>();
            var ALaCarteOfferItemList = new List<ALaCarteOfferItemType>();

            GetDataList(seatAvailabilityResponse, dataLists, datedMarketingSegmentList, datedOperatingSegmentList, serviceDefinitionList, disclosureTypeList, seatProfileList, paxSegmentList, orderSearchResponse, paxList, _hostenv);

            return new()
            {
                Items = new SeatAvailResponseType[1]
                {
                    new SeatAvailResponseType()
                    {
                        DataLists = dataLists,
                        SeatMap = GetSeatMapList(seatAvailabilityResponse,datedMarketingSegmentList,paxSegmentList, seatProfileList, serviceDefinitionList,ALaCarteOfferItemList, paxList, orderSearchResponse),
                        ALaCarteOffer = ALaCarteOfferItemList.Count()>0 ? GetALaCarteOffer(seatAvailabilityResponse,dataLists,ALaCarteOfferItemList) : null,
                        Warning = GetSeatWarning(warningList)
                     }
                }
            };
        }

        private static Domain.RequestResponseModels.SeatAvailabilityRS.WarningType[] GetSeatWarning(IEnumerable<NDCWarning> warningList)
        {
            return warningList.Where(w => w != null).Select(warning =>
                new Domain.RequestResponseModels.SeatAvailabilityRS.WarningType()
                {
                    Code = Convert.ToString(warning.Code),
                    DescText = warning.ShortText,
                    LangCode = Constants.Application.LangCode,
                    TypeCode = warning.Type,
                    TagText = warning.TagText
                }).ToArray();
        }

        private static SeatMapType[] GetSeatMapList(SeatMSAvailabilityResponse seatAvailabilityResponse, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, List<PaxType> paxList, OrderSearchResponse orderSearchResponse)
        {
            return seatAvailabilityResponse.FlightSegmentSeatMap?.Where(fssm => fssm.Catalogue != null && fssm.Catalogue.Any())
            .Select(fssm => new SeatMapType()
            {
                PaxSegmentRefID = GetSegmentIdV2(fssm.FlightSegment, datedMarketingSegmentList, paxSegmentList),
                CabinCompartment = GetCabinCompartment(fssm, datedMarketingSegmentList, paxSegmentList, seatProfileList, serviceDefinitionList, aLaCarteOfferItemList, paxList, orderSearchResponse)

            }).ToArray();
        }

        private static CabinCompartmentType[] GetCabinCompartment(FlightSegmentSeatMap flightSegmentSeatMap, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, List<PaxType> paxList, OrderSearchResponse orderSearchResponse)
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

                    cabinType.CabinTypeCode = GetCabinCode(c.ClassType);
                    cabinType.CabinTypeName = GetCabinName(c.ClassType);

                    if (flightSegmentSeatMap.FlightSegment.FareFamily.Contains(cabinType.CabinTypeName.Split(" ").FirstOrDefault()) || cabinType.CabinTypeName.Contains(flightSegmentSeatMap.FlightSegment?.FareFamily))
                    {
                        cabin.CabinType = cabinType;
                        cabin.DeckCode = c.SeatRows?.FirstOrDefault().UpperDeck == true ? DeckCodeType.Upper : DeckCodeType.Main;
                        cabin.DeckCodeSpecified = true;
                        cabin.FirstRowNumber = c.SeatRows.FirstOrDefault(x => x.RowNumber != null).RowNumber;
                        cabin.LastRowNumber = c.SeatRows.LastOrDefault(x => x.RowNumber != null).RowNumber;
                        brandID = GetBrandIDV2(flightSegmentSeatMap.FlightSegment.FareFamily);

                        foreach (var r in c.SeatRows)
                        {
                            if (r.RowNumber != null)
                            {
                                seatRowList.Add(new SeatRowType()
                                {
                                    RowNumber = r.RowNumber.ToString(),
                                    Seat = GetSeat(r, datedMarketingSegmentList, paxSegmentList, paxList, seatProfileList, serviceDefinitionList, aLaCarteOfferItemList, flightSegmentSeatMap, columnList, brandID, c, orderSearchResponse),
                                    RowCharacteristicCode = GetRowCharacteristics(r)
                                });
                            }
                        }

                        foreach (var clm in columnList)
                        {
                            var seatColumn = clm.Split('_');
                            seatColumnList.Add(new SeatColumnType()
                            {
                                ColumnID = seatColumn.First(),
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
                                else if (sr.SeatColumns.Any(column => column.Galley == true))
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
                                else if (sr.SeatColumns.Any(column => column.Closet == true))
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

        private static AvailableSeatType[] GetSeat(SeatRow r, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList, List<PaxType> paxList, List<SeatProfileType> seatProfileList, List<ServiceDefinitionType> serviceDefinitionList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, FlightSegmentSeatMap flightSegmentSeatMap, List<string> columnList, string brandID, SeatMap seatMap, OrderSearchResponse orderSearchResponse)
        {
            return r.SeatColumns.Select(clm =>
            {
                bool exitSeatAvailability = true;
                var columnID = string.Empty;
                if ((!string.IsNullOrEmpty(clm.OccupationCode)))
                {
                    if (clm.Exit && paxList.Any(p => p.PTC == Const.Constants.CHD || p.PTC == Const.Constants.INF))
                        exitSeatAvailability = false;

                    columnID = clm.SeatIdentifer.Substring(clm.SeatIdentifer.Length - 1) + "_" + (clm.Pos == "C" ? "MS" : clm.Pos);

                    if (columnList.Count > 0 && !columnList.Any(c => c == columnID))
                        columnList.Add(columnID);

                    else if (columnList.Count == 0)
                        columnList.Add(columnID);

                    if (brandID != null)
                    {
                        if (exitSeatAvailability && clm.Available && clm.BrandId == brandID && clm.OccupationCode == Const.Constants.OccupationCodeF) // have to check for occupational code Z
                        {       //Seat is free "F"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = clm.OccupationCode,
                                SeatCharacteristicCode = clm.CharacteristicCodes?.ToArray(),
                                OfferItemRefID = CreateALaCarteOfferItemList(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.Eligibility, orderSearchResponse),
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Available && clm.BrandId == brandID && clm.OccupationCode == Const.Constants.OccupationCodeZ)
                        {      //Seat is blocked but available is true
                            return new AvailableSeatType()
                            {
                                ColumnID = clm.SeatIdentifer[^1..],
                                RowNumber = r.RowNumber,
                                OccupationStatusCode = Const.Constants.OccupationCodeF,
                                SeatCharacteristicCode = clm.CharacteristicCodes != null ? clm.CharacteristicCodes.ToArray() : null,
                                OfferItemRefID = CreateALaCarteOfferItemList(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.Eligibility, orderSearchResponse),   //Comment
                                SeatProfileRefID = clm.Exit ? seatProfileList.FirstOrDefault()?.SeatProfileID : null
                            };
                        }
                        else if (exitSeatAvailability && clm.Blocked)
                        {      //Seat blocked for other reasons "Z"
                            return new AvailableSeatType()
                            {
                                ColumnID = clm?.SeatIdentifer?[(clm.SeatIdentifer.Length - 1)..],
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
                                OfferItemRefID = CreateALaCarteOfferItemList(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.Eligibility, orderSearchResponse),
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
                                OfferItemRefID = CreateALaCarteOfferItemList(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, clm.CatalogueRefIds, clm.Eligibility, orderSearchResponse),
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

        private static string[] CreateALaCarteOfferItemList(List<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<PaxType> paxList, IEnumerable<ServiceDefinitionType> serviceDefinitionList, FlightSegmentSeatMap flightSegmentSeatMap, IEnumerable<string> catalogueRefIds, IEnumerable<SeatEligibility> eligibility, OrderSearchResponse orderSearchResponse)
        {
            return catalogueRefIds.Select(catalogueRefId => GetALaCarteOfferItemListV2(datedMarketingSegmentList, paxSegmentList, aLaCarteOfferItemList, paxList, serviceDefinitionList, flightSegmentSeatMap, catalogueRefId, eligibility, orderSearchResponse)).ToArray();
        }

        private static string GetALaCarteOfferItemListV2(List<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList, List<ALaCarteOfferItemType> aLaCarteOfferItemList, IEnumerable<PaxType> paxList, IEnumerable<ServiceDefinitionType> serviceDefinitionList, FlightSegmentSeatMap flightSegmentSeatMap, string catalogueRefId, IEnumerable<SeatEligibility> eligibility, OrderSearchResponse orderSearchResponse)
        {
            foreach (var sc in flightSegmentSeatMap.Catalogue)
            {
                if (sc.CatalogueId == catalogueRefId && !aLaCarteOfferItemList.Any(a => a.OfferItemID == GetTrimmedCatalogueIDV2(catalogueRefId)))
                {
                    aLaCarteOfferItemList.Add(new ALaCarteOfferItemType()
                    {
                        OfferItemID = GetTrimmedCatalogueIDV2(sc.CatalogueId),
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
                            PaxRefID = GetEligiblePaxList(eligibility, orderSearchResponse, flightSegmentSeatMap.FlightSegment, catalogueRefId),
                            OfferFlightAssociations = new OfferFlightAssociationsType()
                            {
                                Item = new PaxSegmentRefListType()
                                {
                                    PaxSegmentRefID = new[] { GetSegmentIdV2(flightSegmentSeatMap.FlightSegment, datedMarketingSegmentList, paxSegmentList) }
                                },
                                ItemElementName = ItemChoiceType.PaxSegmentReferences
                            }
                        }
                    });
                }
            }

            return GetTrimmedCatalogueIDV2(catalogueRefId);
        }

        private static string[] GetEligiblePaxList(IEnumerable<SeatEligibility> eligibility, OrderSearchResponse orderSearchResponse, Segment flightSegment, string catalogueRefId)
        {
            var paxRefIDList = new List<string>();
            var paxList = new List<Customer>();
            var seatedPax = new List<Customer>();

            var seatOrderItems = orderSearchResponse.OrderItems.Where(oi => oi.Services.First().ProductOrder.ProductType.ToString() == "Seat" && oi.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == flightSegment.SegmentId);

            foreach (var s in orderSearchResponse.OrderItems.Where(oi => oi.Services.First().ProductOrder.ProductType.ToString() != "Seat"))
            {
                if (!paxList.Exists(p => p.CustId == s.Services.First().ServiceBeneficiaries.First()))
                {
                    var pax = orderSearchResponse.CustomerDetails.Where(cust => cust.CustId == s.Services.First().ServiceBeneficiaries.FirstOrDefault()).FirstOrDefault();
                    paxList.Add(pax);
                }
            }

            if (seatOrderItems.Count() > 0)
            {
                foreach (var oi in seatOrderItems)
                {
                    seatedPax.Add(orderSearchResponse.CustomerDetails.FirstOrDefault(cust => cust.CustId == oi.Services.First().ServiceBeneficiaries.First()));
                }

                foreach (var eligiblePax in eligibility)
                {
                    var found = seatedPax.Exists(p => p.LastNameNumber + "-" + p.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault());

                    if (!found)
                        paxRefIDList.Add(paxList.Where(pax => pax.LastNameNumber + "-" + pax.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault())?.FirstOrDefault()?.CustIdNdc);
                }
            }
            else
            {
                foreach (var eligiblePax in eligibility)
                {
                    if (eligiblePax.CatalogueRefId == catalogueRefId)
                        paxRefIDList.Add(paxList.Where(pax => pax.LastNameNumber + "-" + pax.FirstNameNumber == eligiblePax.CustomerId.FirstOrDefault())?.FirstOrDefault()?.CustIdNdc);
                }
            }

            return paxRefIDList.ToArray();
        }

        private static ALaCarteOfferType GetALaCarteOffer(SeatMSAvailabilityResponse seatAvailabilityResponse, DataListsType dataLists, List<ALaCarteOfferItemType> aLaCarteOfferItemList)
        {
            ALaCarteOfferType aLaCarteOffer = new();
            var catalogue = seatAvailabilityResponse.FlightSegmentSeatMap.Where(fs => fs.Catalogue != null).Select(c => c.Catalogue).FirstOrDefault();

            if (catalogue != null && catalogue.FirstOrDefault() != null)
            {
                aLaCarteOffer.DisclosureRefID = dataLists.DisclosureList.FirstOrDefault().DisclosureID;
                aLaCarteOffer.OfferID = seatAvailabilityResponse.SeatOfferId;
                aLaCarteOffer.OwnerCode = Constants.Application.OWNERCODE;
                aLaCarteOffer.OfferExpirationTimeLimitDateTime = Convert.ToDateTime(seatAvailabilityResponse.OfferExpirationTimeLimit);
                aLaCarteOffer.OfferExpirationTimeLimitDateTimeSpecified = true;
            }
            aLaCarteOffer.OfferItem = aLaCarteOfferItemList.ToArray();

            return aLaCarteOffer;
        }

        private static DataListsType GetDataList(SeatMSAvailabilityResponse seatAvailabilityResponse, DataListsType dataLists, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList, List<ServiceDefinitionType> serviceDefinitionList, List<DisclosureType> seatDisclosureTypeList, List<SeatProfileType> seatProfileList, List<PaxSegmentType> paxSegmentList, OrderSearchResponse orderSearchResponse, List<PaxType> paxList, IHostEnvironment hostEnv)
        {
            var segmentList = new List<Segment>();

            dataLists.PaxList = paxList.Select(pL =>
                                        new PaxType()
                                        {
                                            PTC = pL.PTC,
                                            PaxID = pL.PaxID
                                        }).ToArray();

            foreach (var segment in orderSearchResponse.SegmentDetails)
            {
                GetPaxSegmentId(segment, datedMarketingSegmentList, paxSegmentList);
                GetDatedMarketingSegmentList(segment, datedMarketingSegmentList, datedOperatingSegmentList);
            }

            var exitSeatServiceDescription = GetExitSeatServiceDescription(hostEnv);
            var nonExitSeatServiceDescription = GetNonExitSeatServiceDescription(hostEnv);

            if (seatAvailabilityResponse.FlightSegmentSeatMap != null)
            {
                foreach (var ss in seatAvailabilityResponse.FlightSegmentSeatMap)
                {
                    if (ss.Catalogue != null || ss.Catalogue.Count() > 0)
                    {
                        foreach (var sc in ss.Catalogue)
                        {
                            var brandID = GetBrandIDV2(ss.FlightSegment.FareFamily);
                            if (brandID != null && brandID == sc.BrandId)
                            {
                                bool allowExitSeat = true;
                                if (dataLists.PaxList.ToList().Exists(p => p.PTC == Constants.Application.CHD || p.PTC == Constants.Application.INF) && (sc.ProductId == Const.Constants.ExitSeatProductId || sc.ProductId == Const.Constants.ExitReduceLegroomSeatProductID))
                                    allowExitSeat = false;
                                if (allowExitSeat)
                                {
                                    if (!serviceDefinitionList.Exists(sd => sd.Name == sc.ProductName))
                                    {
                                        serviceDefinitionList.Add(new ServiceDefinitionType()
                                        {
                                            ServiceDefinitionID = IdCreator.CreateServiceDefinitionID(serviceDefinitionList.Count()),
                                            OwnerCode = Constants.Application.OwnerCode,
                                            Name = sc.ProductName,
                                            ServiceCode = sc.ProductId,
                                            RFIC = sc.ReasonForIssuanceCode,
                                            RFISC = sc.ReasonForIssuanceSubCode
                                        });
                                    }
                                }
                            }
                            if (sc.ProductId != Const.Constants.ExitSeatProductId && seatDisclosureTypeList.Count() == 0)
                            {
                                seatDisclosureTypeList.Add(new DisclosureType()
                                {
                                    Desc = nonExitSeatServiceDescription.ToArray(),
                                    DisclosureID = IdCreator.CreateSeatDiscloureTypeID(seatDisclosureTypeList.Count())
                                });
                                seatDisclosureTypeList.Distinct();
                            }
                            if ((sc.ProductId == Const.Constants.ExitSeatProductId || sc.ProductId == Const.Constants.ExitReduceLegroomSeatProductID) && seatProfileList.Count() == 0)
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

            dataLists.PaxList = GetSeatPaxListV2(dataLists.PaxList.ToList());
            dataLists.DatedMarketingSegmentList = datedMarketingSegmentList.ToArray();
            dataLists.DatedOperatingSegmentList = datedOperatingSegmentList.ToArray();
            dataLists.PaxSegmentList = paxSegmentList.ToArray();
            dataLists.SeatProfileList = seatProfileList.Count() > 0 ? seatProfileList.ToArray() : null;
            dataLists.ServiceDefinitionList = serviceDefinitionList.Count() > 0 ? serviceDefinitionList.ToArray() : null;
            dataLists.DisclosureList = seatDisclosureTypeList.Count() > 0 ? seatDisclosureTypeList.ToArray() : null;

            return dataLists;
        }

        private static PaxType[] GetSeatPaxListV2(List<PaxType> paxList)
        {
            if (paxList.Exists(x => x.PTC == Constants.Application.INF))
            {
                var adtPax = paxList.Where(x => x.PTC == Constants.Application.ADT).Select(x => x).ToList();
                var infPax = paxList.Where(x => x.PTC == Constants.Application.INF).Select(x => x).ToList();

                for (int i = 0; i < infPax.Count(); i++)
                {
                    adtPax[i].PaxRefID = infPax[i].PaxID;
                }
            }

            return paxList.ToArray();
        }

        private static string GetPaxSegmentId(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<PaxSegmentType> paxSegmentList)
        {
            if (datedMarketingSegmentList.Exists(x => x.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime)).ToUniversalTime())))
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

        private static PaxSegmentType CreateSeatPaxSegmentType(List<PaxSegmentType> paxSegmentList)
        {
            var paxSegmentType = new PaxSegmentType()
            {
                PaxSegmentID = IdCreator.CreatePaxSegmentID(paxSegmentList.Count()),
                DatedMarketingSegmentRefId = IdCreator.CreateDatedMarketingSegmentID(paxSegmentList.Count())
            };

            return paxSegmentType;
        }

        private static void GetDatedMarketingSegmentList(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList)
        {
            if (!datedMarketingSegmentList.Exists(x => x.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && x.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && DateTime.Equals(x.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime).ToUniversalTime()))))
            {
                datedMarketingSegmentList.Add(CreateDatedMarketingSegment(flightSegment, datedMarketingSegmentList, datedOperatingSegmentList));
            }
        }

        private static DatedMarketingSegmentType CreateDatedMarketingSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, List<DatedMarketingSegmentType> datedMarketingSegmentList, List<DatedOperatingSegmentType> datedOperatingSegmentList)
        {
            DatedMarketingSegmentType datedMarketingSegmentType = new();

            datedMarketingSegmentType.DatedMarketingSegmentId = IdCreator.CreateDatedMarketingSegmentID(datedMarketingSegmentList.Count());
            datedMarketingSegmentType.DatedOperatingSegmentRefId = GetDatedOperatingSegmentID(flightSegment, datedMarketingSegmentList.Count(), datedOperatingSegmentList);
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
            datedMarketingSegmentType.MarketingCarrierFlightNumberText = flightSegment.MarketingFlightNumber;

            return datedMarketingSegmentType;
        }

        private static string GetDatedOperatingSegmentID(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, int count, List<DatedOperatingSegmentType> datedOperatingSegmentList)
        {
            var datedOperatingSegment = CreateDatedOperatingSegment(flightSegment, count);
            datedOperatingSegmentList.Add(datedOperatingSegment);
            return datedOperatingSegment.DatedOperatingSegmentId;
        }

        private static DatedOperatingSegmentType CreateDatedOperatingSegment(VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment flightSegment, int count)
        {
            DatedOperatingSegmentType datedOperatingSegmentList = new();

            datedOperatingSegmentList.DatedOperatingSegmentId = IdCreator.CreateDatedOperatingSegmentID(count);
            datedOperatingSegmentList.CarrierDesigCode = flightSegment.OperatingCarrier == null ? flightSegment.MarketingCarrier : flightSegment.OperatingCarrier;
            datedOperatingSegmentList.OperatingCarrierFlightNumberText = flightSegment.OperatingFlightNumber == null ? flightSegment.MarketingFlightNumber : flightSegment.OperatingFlightNumber;
            datedOperatingSegmentList.Duration = (flightSegment.FlightDuration != null) ? GetDuration(TimeSpan.Parse(flightSegment.FlightDuration)) : string.Empty;

            return datedOperatingSegmentList;
        }

        private static string GetDuration(TimeSpan timeSpan)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", Constants.Application.PT, timeSpan.Days * 24 + timeSpan.Hours, Constants.Application.H, timeSpan.Minutes, Constants.Application.M, timeSpan.Seconds, Constants.Application.S);
        }
       

        private static List<DescType> GetNonExitSeatServiceDescription(IHostEnvironment hostEnv)
        {
            VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.NonExitServiceDiscriptionList.LoadXml(File.ReadAllText(hostEnv.ContentRootPath + "\\Resources\\NotExitSeatServiceDescription.xml"));
            XmlDocument doc = VAA.EAPI.NDC.Offer.Domain.Model.VAA.Constants.NonExitServiceDiscriptionList;
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


        private static string GetSegmentIdV2(Segment flightSegment, IEnumerable<DatedMarketingSegmentType> datedMarketingSegmentList, IEnumerable<PaxSegmentType> paxSegmentList)
        {
            var dms = datedMarketingSegmentList.First(ss => ss.Dep.IATA_LocationCode == flightSegment.OriginAirportCode && ss.Arrival.IATA_LocationCode == flightSegment.DestinationAirportCode && DateTime.Equals(ss.Dep.AircraftScheduledDateTime.ToUniversalTime(), (Convert.ToDateTime(flightSegment.ScheduledDepartureTime.LocalDateTime)).ToUniversalTime()));

            return paxSegmentList.First(x => x.DatedMarketingSegmentRefId == dms.DatedMarketingSegmentId).PaxSegmentID;
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
            }

            return serviceDefinitionRefID;
        }

        private static string GetTrimmedCatalogueIDV2(string catalogueId)
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

        private static string GetCabinCode(string classType)
        {
            string resultstring = "";
            switch (classType)
            {
                case "C": return resultstring = "2";

                case "J": return resultstring = "2";

                case "W": return resultstring = "4";

                case "Y": return resultstring = "5";

                default: return resultstring;
            }
        }
        private static string GetCabinName(string classType)
        {
            string resultstring = "";
            switch (classType)
            {
                case "C": return resultstring = "Upper Class";

                case "J": return resultstring = "Upper Class";

                case "W": return resultstring = "Premium Economy";

                case "Y": return resultstring = "Economy";

                default: return resultstring;
            }
        }
        private static string GetBrandIDV2(string fareFamily)
        {
            switch (fareFamily)
            {
                case "Economy Classic": return Constants.Application.EconomyBrandID;

                case "Economy Light": return Constants.Application.EconomyBrandID;

                case "Economy Delight": return Constants.Application.DelightBrandID;

                case "Premium": return Constants.Application.PremEconomyBrandID;

                case "Upper": return Constants.Application.UpperBrandID;

                case "Economy Classic Flex": return Constants.Application.EconomyBrandID;

                case "Economy Delight Flex": return Constants.Application.DelightBrandID;

                case "Premium Flex": return Constants.Application.PremEconomyBrandID;

                case "Upper Flex": return Constants.Application.UpperBrandID;

                default: return null;
            }
        }

        public static string[] GetRowCharacteristics(SeatRow row)
        {
            var rowcharacteristics = new List<string>();
            if (row.OverWing == true)
            {
                rowcharacteristics.Add("K");
            }
            else
            {
                rowcharacteristics.Add("X");
                if (row.ExitOnLeft == true)
                {
                    rowcharacteristics.Add("EL");
                    if (row.ExitOnRight == true)
                    {
                        rowcharacteristics.Add("ER");
                    }
                }
                else if (row.ExitOnRight == true)
                {
                    rowcharacteristics.Add("ER");
                }
            }

            return rowcharacteristics.ToArray();
        }

        public static string[] GetColumnIDForToilet(IEnumerable<SeatColumn> columnList, string seatConfig)
        {
            List<string> columnIdList = new();
            var seatConfigLength = seatConfig.Length;
            foreach (var col in columnList)
            {
                if (seatConfigLength > Convert.ToInt32(col.Index) && col.Toilet == true)
                {
                    var coulmnID = seatConfig.Substring(Convert.ToInt32(col.Index), 1);
                    columnIdList.Add(coulmnID);
                }

                else if (seatConfigLength == Convert.ToInt32(col.Index) && col.Toilet == true)
                {
                    var coulmnID = seatConfig.Substring(Convert.ToInt32(col.Index));
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
                if (seatConfigLength > Convert.ToInt32(col.Index) && col.Galley == true)
                {
                    var columnID = seatConfig.Substring(Convert.ToInt32(col.Index), 1);
                    columnIdList.Add(columnID);
                }

                else if (seatConfigLength == Convert.ToInt32(col.Index) && col.Galley == true)
                {
                    var coulmnID = seatConfig.Substring(Convert.ToInt32(col.Index));
                    columnIdList.Add(coulmnID);
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
                if (seatConfigLength > Convert.ToInt32(col.Index) && col.Closet == true)
                {
                    var coulmnID = seatConfig.Substring(Convert.ToInt32(col.Index), 1);
                    columnIdList.Add(coulmnID);
                }

                else if (seatConfigLength == Convert.ToInt32(col.Index) && col.Closet == true)
                {
                    var coulmnID = seatConfig.Substring(Convert.ToInt32(col.Index));
                    columnIdList.Add(coulmnID);
                }
            };

            return columnIdList.ToArray();
        }

        public static string GetRowNumber(IEnumerable<SeatRow> rowList, int rowNumber)
        {
            var firstRowNumber = string.Empty;
            var seatRowWithRowNumber = rowList.Select((item, index) => new { item, index }).Where(x => x.item.RowNumber != null).Select(x => x.index).ToArray();

            int firstSeatRowIndex = seatRowWithRowNumber.First();
            int lastSeatRowIndex = seatRowWithRowNumber.Last();

            if (rowNumber < firstSeatRowIndex)
            {
                firstRowNumber = rowList.ElementAt(firstSeatRowIndex).RowNumber.ToString();
            }
            else if (rowList.ElementAt(rowNumber - 1).RowNumber != null)
            {
                firstRowNumber = rowList.ElementAt(seatRowWithRowNumber.Where(x => x < rowNumber).Last()).RowNumber.ToString();
            }
            else if (rowList.ElementAt(rowNumber - 1).RowNumber == null && rowList.Count() > rowNumber + 1)
            {
                firstRowNumber = rowList.ElementAt(seatRowWithRowNumber.Where(x => x > rowNumber).First()).RowNumber.ToString();
            }
            else if (rowNumber > lastSeatRowIndex)
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
    }
}
