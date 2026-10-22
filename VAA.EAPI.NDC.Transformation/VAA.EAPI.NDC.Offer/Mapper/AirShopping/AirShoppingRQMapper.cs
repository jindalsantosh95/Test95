using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Application.Query.AirShopping;
using VAA.EAPI.NDC.Offer.Domain.ExternalEntities;
using VAA.EAPI.NDC.Offer.Domain.Model.OfferRQ;
using VAA.EAPI.NDC.Offer.Domain.Models.OfferRQ;
using VAA.EAPI.NDC.Offer.Domain.Models.VAA;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.Clid;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.AirShoppingMS;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public class AirShoppingRQMapper
    {
        public static AirShoppingMSQuery MapOfferRequest(AirShoppingQuery request, BspMarketCurrencyData currencyData, PccCurrencyData pccCurrencyData, IEnumerable<FareFamilies> fareFamiliesFromConfig, IEnumerable<ItaFareTypeCodeData> itaPaxTypeCode, AgencyValidateResponse agencyDetails, string fareTypeCode, string fareSubTypeCode, IEnumerable<TtlRulesData> ttlRules, CorporateData configMSRS)
        {
            var requestCurrency = request.AirShoppingRQ?.Request?.ResponseParameters?.CurParameter?[0]?.CurCode;
            var flightSearchRequest = GetFlightSearchRequest(request.AirShoppingRQ, fareFamiliesFromConfig);
            var departureDate = flightSearchRequest.SearchOriginDestinations?.FirstOrDefault()?.DepartureDate;
            var departureDateTime = !string.IsNullOrEmpty(departureDate) ? DateTime.ParseExact(departureDate, Constants.Application.DateFormat, CultureInfo.InvariantCulture) : (DateTime?)null;
            
            AirShoppingMSQuery offerRequest = new()
            {
                OrderId = Guid.NewGuid().ToString(),
                FareTypeRequest = new()
                {
                    FareTypeCode = fareTypeCode,
                    FareSubTypeCode = fareSubTypeCode,
                },
                Parties = GetParties(request.AirShoppingRQ, pccCurrencyData, agencyDetails, fareTypeCode, configMSRS),
                Pos = GetPos(currencyData, agencyDetails, requestCurrency),
                FlightsearchRequest = flightSearchRequest,
                CustomerDetails = GetCustomerDetails(request.AirShoppingRQ?.Request?.PaxList, request.AirShoppingRQ?.Request?.OfferCriteria?.FareCriteria, itaPaxTypeCode),
                ServiceProducts = GetFightServiceProductPreferences(request.AirShoppingRQ?.Request?.OfferCriteria)?.Where(sP => sP.IncludeInd),
                TtlInfo = departureDateTime.HasValue? CalculateTTL(departureDateTime.Value, fareTypeCode, fareSubTypeCode, ttlRules) : null,

            };
            return offerRequest;
        }

        public static IEnumerable<TtlInfo> CalculateTTL(DateTime departureDate, string fareTypeCode, string fareSubTypeCode, IEnumerable<TtlRulesData> ttlRules)
        {
            if (ttlRules == null) return Enumerable.Empty<TtlInfo>();

            DateTime bookingDate = DateTime.UtcNow;
            DateTime depDate = departureDate;
            var daysUntilDeparture = (depDate - bookingDate).Days;

            if (!int.TryParse(fareTypeCode, out int fareType))
            {
                return Enumerable.Empty<TtlInfo>();
            }

            var matches = ttlRules.Where(rule => rule.FareTypeCode == fareType && rule.FareSubTypeCode == fareSubTypeCode && (rule.MaxDepartureDate == 0? daysUntilDeparture >= rule.MinDepartureDate : daysUntilDeparture >= rule.MinDepartureDate && daysUntilDeparture <= rule.MaxDepartureDate)) .OrderByDescending(r => r.MinDepartureDate).ToList();
                     
            var results = new List<TtlInfo>();
            foreach (var match in matches)
            {
                DateTime? ttlAfterBooking = match.TTLAfterBooking.HasValue? bookingDate.AddDays(match.TTLAfterBooking.Value): (DateTime?)null;

                DateTime? ttlBeforeDeparture = match.TTLBeforeDeparture.HasValue? depDate.AddDays(-match.TTLBeforeDeparture.Value) : (DateTime?)null;

                DateTime? finalTtl = null;

                if (ttlAfterBooking.HasValue && ttlBeforeDeparture.HasValue)
                    finalTtl = ttlAfterBooking < ttlBeforeDeparture ? ttlAfterBooking : ttlBeforeDeparture;
                else
                    finalTtl = ttlAfterBooking ?? ttlBeforeDeparture;

                if (finalTtl.HasValue)
                {
                    results.Add(new TtlInfo
                    {
                        OperatingCarrier = match.OperatingCarrier,
                        TtlDate = finalTtl.Value.ToString(Constants.Application.ZuluDateTimeFormat)
                    });
                }
            }

            return results;
        }

        public static FlexiDateSearchMSQuery MapCalendarOfferRequest(AirShoppingQuery request, BspMarketCurrencyData currencyData, PccCurrencyData pccCurrencyData, IEnumerable<FareFamilies> fareFamiliesFromConfig, IEnumerable<ItaFareTypeCodeData> itaPaxTypeCode, AgencyValidateResponse agencyDetails, string requestCurrency, string fareTypeCode, CorporateData configMSRS)
        {
            return new()
            {
                OrderId = Guid.NewGuid().ToString(),
                Parties = GetParties(request?.AirShoppingRQ, pccCurrencyData, agencyDetails, fareTypeCode, configMSRS),
                Pos = GetPos(currencyData, agencyDetails, requestCurrency),
                FlightsearchRequest = GetFlightSearchRequest(request?.AirShoppingRQ, fareFamiliesFromConfig),
                CustomerDetails = GetCustomerDetails(request?.AirShoppingRQ?.Request?.PaxList, request.AirShoppingRQ?.Request?.OfferCriteria?.FareCriteria, itaPaxTypeCode)
            };
        }

        public static Parties GetParties(IATA_AirShoppingRQ airShoppingRQ, PccCurrencyData pccCurrencyData, AgencyValidateResponse agencyDetails, string fareTypeCode, CorporateData configMSRS)
        {
            var distributionChain = (airShoppingRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal)).Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier));

            if (distributionChain.Count() == 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.First()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.First()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.First().OrgRole.ToString(), true),
                            Ordinal = distributionChain.First()?.Ordinal,
                            PccCode = pccCurrencyData?.PCC != null ? pccCurrencyData.PCC : null,
                            AccountCode = (fareTypeCode == Constants.Application.PrivateFareTypeCode) ? agencyDetails?.AgencyDetails?.AccountCode : (fareTypeCode == Constants.Application.CorporateFareTypeCode) ? agencyDetails?.CorporateDetails?.CorporateAccountCode : null
                        }
                },
                    CorporateAccount = GetCorporateAccount(airShoppingRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner, configMSRS)
                };
            }
            else if (distributionChain.Count() > 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.Last().ParticipatingOrg?.OrgID,
                            Name = distributionChain.Last().ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.Last().OrgRole.ToString(), true),
                            Ordinal = distributionChain.Last().Ordinal,
                            PccCode = pccCurrencyData?.PCC != null ? pccCurrencyData.PCC : null,
                            AccountCode = (fareTypeCode == Constants.Application.PrivateFareTypeCode) ? agencyDetails?.AgencyDetails?.AccountCode : (fareTypeCode == Constants.Application.CorporateFareTypeCode) ? agencyDetails?.CorporateDetails?.CorporateAccountCode : null

                        },
                        new()
                        {
                            AgencyID = distributionChain.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.FirstOrDefault()?.OrgRole.ToString(), true),
                            Ordinal = distributionChain.FirstOrDefault()?.Ordinal,
                            PccCode = pccCurrencyData?.PCC != null ? pccCurrencyData.PCC : null,
                            AccountCode = (fareTypeCode == Constants.Application.PrivateFareTypeCode) ? agencyDetails?.AgencyDetails?.AccountCode : (fareTypeCode == Constants.Application.CorporateFareTypeCode) ? agencyDetails?.CorporateDetails?.CorporateAccountCode : null

                        }
                    },
                    CorporateAccount = GetCorporateAccount(airShoppingRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner, configMSRS)
                };
            }
            return null;
        }

        private static CorporateAccount GetCorporateAccount(ProgramOwnerType programOwner, CorporateData configMSRS)
        {
            return programOwner?.Item is not OrgType org ? null
                : new CorporateAccount
                {
                    AccountName = org.Name,
                    CorporateCode = org.OrgID,
                    TicketDesignator = configMSRS?.CbiTicketDesignator
                };
        }
        private static Pos GetPos(BspMarketCurrencyData currencyData, AgencyValidateResponse agencyDetails, string requestCurrency)
        {
            if (currencyData == null) return null;

            string local = currencyData.LocalCurrencyCode;
            string dual = currencyData.DualCurrencyCode;
            string currency;

            if (agencyDetails?.AgencyDetails?.PocCode == Constants.Application.PointOfCommencement)
            {
                currency = null;
            }
            else if (requestCurrency.IsNullOrEmpty())
            {
                currency = !local.IsNullOrEmpty() ? local : dual;
            }
            else if (requestCurrency == local)
            {
                currency = local;
            }
            else if (requestCurrency == dual)
            {
                currency = !dual.IsNullOrEmpty() ? dual : local;
            }
            else
            {
                currency = !local.IsNullOrEmpty() ? local : dual;
            }

            return new Pos
            {
                CountryCode = currencyData.CountryCode,
                CurrencyCode = currency
            };
        }
        private static IEnumerable<Customer> GetCustomerDetails(PaxType[] paxTypes, FareCriteriaType[] fareCriteria, IEnumerable<ItaFareTypeCodeData> itaPaxTypeCode)
        {
            List<Customer> ptcList = [];

            var isFareTypeCodeAvailable = itaPaxTypeCode.Any(x => x.FareTypeCode == fareCriteria?.FirstOrDefault()?.FareTypeCode?.FirstOrDefault());

            if (fareCriteria is not null && isFareTypeCodeAvailable)
            {
                var itaPtcTypeCode = from itaPtcCode in itaPaxTypeCode
                                     from paxList in paxTypes
                                     where itaPtcCode.NdcPaxType == paxList.PTC && itaPtcCode.FareTypeCode == fareCriteria.FirstOrDefault()?.FareTypeCode.FirstOrDefault() && (fareCriteria.FirstOrDefault()?.FareSubTypeCode?.FirstOrDefault() == null || itaPtcCode.FareSubTypeCode == fareCriteria.FirstOrDefault()?.FareSubTypeCode?.FirstOrDefault()?.Replace(" ", "").ToUpper())
                                     select new { itaPtcCode, paxList };

                ptcList = itaPtcTypeCode.Select(x => new Customer()
                {
                    Ptc = x.paxList.PTC == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : x.paxList.PTC,
                    ItaPtc = x.itaPtcCode.ItaPaxType,
                    CustId = x.paxList?.PaxID
                }).ToList();
            }
            else
            {
                ptcList = paxTypes.Select(pax => new Customer()
                {
                    Ptc = pax?.PTC == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : pax?.PTC,
                    CustId = pax?.PaxID
                }).ToList();
            }

            return ptcList;
        }

        private static IEnumerable<ServiceProduct> GetFightServiceProductPreferences(OfferCriteriaType offerCriteria)
        {
            List<ServiceProduct> serviceProducts = new();

            if (offerCriteria != null && offerCriteria.ServiceCriteria != null && offerCriteria.ServiceCriteria.Length > 0)
            {
                offerCriteria.ServiceCriteria.ToList().ForEach(ser =>
                {
                    if (ser.TaxonomyCode != null && ser.TaxonomyCode.Length > 0)
                    {
                        ser.TaxonomyCode.ToList().ForEach(tax =>
                        {
                            ServiceProduct serviceProduct = new()
                            {
                                IncludeInd = ser.IncludeInd,
                                ProductCode = tax
                            };
                            serviceProducts.Add(serviceProduct);
                        });
                    }
                });
            }

            return serviceProducts;
        }

        private static FlightSearchRequest GetFlightSearchRequest(IATA_AirShoppingRQ airShoppingRQ, IEnumerable<FareFamilies> fareFamiliesFromConfig)
        {
            var flightSearchRequest = new FlightSearchRequest();
            var flightOriginDestCriteria = (FlightRequestOriginDestinationsCriteriaType)airShoppingRQ?.Request?.FlightRequest?.Item;
            flightSearchRequest.SearchOriginDestinations = GetSearchOriginDestinations(flightOriginDestCriteria);
            flightSearchRequest.CabinTypes = GetCabinTypes(flightOriginDestCriteria?.OriginDestCriteria);
            flightSearchRequest.FareFamilies = PopulateFareFamily(flightSearchRequest.CabinTypes, airShoppingRQ, fareFamiliesFromConfig);
            flightSearchRequest.CalendarSearch = flightOriginDestCriteria?.OriginDestCriteria.FirstOrDefault()?.CalendarDateCriteria != null;
            flightSearchRequest.CarryOnBaggageAllowance = true;
            flightSearchRequest.CheckInBaggageAllowance = true;
            flightSearchRequest.NonStopOnly = airShoppingRQ?.Request?.FlightRelatedCriteria?.FlightCriteria?.FirstOrDefault()?.FlightCharacteristicsCriteria?.FirstOrDefault()?.CharacteristicCode.ToString() == Constants.Application.Direct;

            return flightSearchRequest;
        }

        private static IEnumerable<SearchOriginDestination> GetSearchOriginDestinations(FlightRequestOriginDestinationsCriteriaType flightOriginDestCriteria)
        {
            var searchOriginDestinations = new List<SearchOriginDestination>();

            foreach (var originDest in flightOriginDestCriteria?.OriginDestCriteria)
            {
                var searchOriginDestination = new SearchOriginDestination
                {
                    Origin = originDest?.OriginDepCriteria?.IATA_LocationCode,
                    Destination = originDest?.DestArrivalCriteria?.IATA_LocationCode,
                    DepartureDate = originDest?.OriginDepCriteria?.Date.ToString(Constants.Application.DateFormat),
                    DepartureTimeRange = originDest?.OriginDepCriteria?.Time.Hour == 0 ? null : GetDepartureTimeRange(originDest)
                };

                searchOriginDestinations.Add(searchOriginDestination);
            }

            return searchOriginDestinations;
        }

        private static DepartureTimeRange GetDepartureTimeRange(OriginDestCriteriaType originDestType)
        {
            var hoursBeforeDeparture = double.Parse(RegexCompiled.Match(originDestType.OriginDepCriteria?.TimeBeforeDuration, Constants.Application.RegexToValidateNumeric).Value);
            var hoursAfterDeparture = double.Parse(RegexCompiled.Match(originDestType.OriginDepCriteria?.TimeBeforeDuration, Constants.Application.RegexToValidateNumeric).Value);
            var departureTime = originDestType.OriginDepCriteria?.Date.AddHours(double.Parse(originDestType.OriginDepCriteria?.Time.Hour.ToString()));

            return new DepartureTimeRange
            {
                EarliestTime = Convert.ToDateTime(departureTime).AddHours(-hoursBeforeDeparture).ToString(Constants.Application.DateTimeFormat),
                LatestTime = Convert.ToDateTime(departureTime).AddHours(hoursAfterDeparture).ToString(Constants.Application.DateTimeFormat)
            };
        }

        private static IEnumerable<CabinType> GetCabinTypes(OriginDestCriteriaType[] originDestCriteriaTypes)
        {
            var cabinTypes = new List<CabinType>();
            if (originDestCriteriaTypes.First().CabinType != null)
            {
                foreach (var cabin in originDestCriteriaTypes?.First().CabinType)
                {
                    if (!cabinTypes.Exists(c => c.CabinCode == cabin.CabinTypeCode))
                    {
                        var cabinType = new CabinType
                        {
                            CabinCode = cabin.CabinTypeCode,
                            CabinName = (cabin.CabinTypeCode).ToString()
                        };
                        cabinTypes.Add(cabinType);
                    }
                }
            }

            return cabinTypes.Count > 0 ? cabinTypes : null;
        }

        public static IEnumerable<string> PopulateFareFamily(IEnumerable<CabinType> cabinTypes, IATA_AirShoppingRQ offerRequest, IEnumerable<FareFamilies> fareFamiliesFromConfig)
        {
            List<SearchInputsFareFamilyToPrice> FareFamilies = new List<SearchInputsFareFamilyToPrice>();

            List<FareFamilies> fareRQCombinations = new List<FareFamilies>();

            var fightServiceProductPreferences = GetFightServiceProductPreferences(offerRequest.Request.OfferCriteria);
            var farePreferences = GetFarePreferences(offerRequest.Request.OfferCriteria);

            if (cabinTypes != null)
            {
                foreach (var cabinType in cabinTypes)
                {
                    if (fightServiceProductPreferences != null && fightServiceProductPreferences.Count() > 0)
                    {
                        foreach (var pref in fightServiceProductPreferences)
                        {
                            FareFamilies fareRQ = new FareFamilies()
                            {
                                CabinCode = cabinType.CabinCode,
                                FareTypeCode = farePreferences != null && farePreferences.Any() ? farePreferences.FirstOrDefault() : "",
                                ServiceTaxonomy = new List<ServiceTaxonomy>()
                                {
                                    new ServiceTaxonomy()
                                    {
                                        IncludeInd = pref.IncludeInd,
                                        ServiceCode = pref.ProductCode
                                    }
                                }
                            };
                            fareRQCombinations.Add(fareRQ);
                        }
                    }
                    else
                    {
                        FareFamilies fareRQ = new FareFamilies()
                        {
                            CabinCode = cabinType.CabinCode,
                            FareTypeCode = farePreferences != null && farePreferences.Any() ? farePreferences.FirstOrDefault() : "",
                            ServiceTaxonomy = new List<ServiceTaxonomy>()
                            {
                                new ServiceTaxonomy()
                                {
                                    ServiceCode = ""
                                }
                            }
                        };
                        fareRQCombinations.Add(fareRQ);
                    }
                }
            }
            else
            {
                if (fightServiceProductPreferences != null && fightServiceProductPreferences.Any())
                {
                    foreach (var pref in fightServiceProductPreferences)
                    {
                        FareFamilies fareRQ = new FareFamilies()
                        {
                            CabinCode = "",
                            FareTypeCode = farePreferences != null && farePreferences.Any() ? farePreferences.FirstOrDefault() : "",
                            ServiceTaxonomy = new List<ServiceTaxonomy>()
                            {
                                new ServiceTaxonomy()
                                {
                                    IncludeInd = pref.IncludeInd,
                                    ServiceCode = pref.ProductCode
                                }
                            }
                        };
                        fareRQCombinations.Add(fareRQ);
                    }
                }
                else
                {
                    FareFamilies fareRQ = new FareFamilies()
                    {
                        CabinCode = "",
                        FareTypeCode = farePreferences != null && farePreferences.Any() ? farePreferences.FirstOrDefault() : "",
                        ServiceTaxonomy = new List<ServiceTaxonomy>()
                        {
                            new ServiceTaxonomy()
                            {
                                ServiceCode = ""
                            }
                        }
                    };
                    fareRQCombinations.Add(fareRQ);
                }
            }

            //compare the request combination with combinations from generic config

            IEnumerable<string> commonFarefamilies = Enumerable.Empty<string>();
            IEnumerable<string> prevFareFamilies = Enumerable.Empty<string>();
            IEnumerable<string> totalFarefamilies = Enumerable.Empty<string>();
            int count = 0;

            string prevCabinCode = null;

            foreach (var ffRQ in fareRQCombinations)
            {
                bool found = false;
                var privateFareTypeCode = new[] { "758", "70G", "70J" };
                var requestedServiceTaxonomies = fareRQCombinations
                    .Where(fareRequestCombination => fareRequestCombination.CabinCode == ffRQ.CabinCode)
                    .SelectMany(fareRequestCombination => fareRequestCombination.ServiceTaxonomy)
                    .ToList();

                foreach (var fareFamily in fareFamiliesFromConfig.OrderByDescending(fareFamily => fareFamily.ServiceTaxonomy.Count()))
                {
                    var fareTypeCodes = privateFareTypeCode.Contains(ffRQ.FareTypeCode) ? "" : ffRQ.FareTypeCode;
                    var completeTaxonomyMatch = fareFamily.ServiceTaxonomy.Count() > 1
                        && fareFamily.ServiceTaxonomy.Count() == requestedServiceTaxonomies.Count
                        && fareFamily.ServiceTaxonomy.All(serviceTaxonomy => requestedServiceTaxonomies.Any(requestedServiceTaxonomy =>
                            requestedServiceTaxonomy.IncludeInd == serviceTaxonomy.IncludeInd
                            && requestedServiceTaxonomy.ServiceCode == serviceTaxonomy.ServiceCode));
                    var singleTaxonomyMatch = requestedServiceTaxonomies.Count == 1
                        && fareFamily.ServiceTaxonomy.Count() == 1
                        && ffRQ.ServiceTaxonomy.FirstOrDefault()?.IncludeInd == fareFamily.ServiceTaxonomy.FirstOrDefault().IncludeInd
                        && ffRQ.ServiceTaxonomy.FirstOrDefault()?.ServiceCode == fareFamily.ServiceTaxonomy.FirstOrDefault().ServiceCode;

                    if (ffRQ.CabinCode == fareFamily.CabinCode
                       && fareTypeCodes == fareFamily.FareTypeCode
                        && (completeTaxonomyMatch || singleTaxonomyMatch))
                    {
                        ffRQ.FareFamily = fareFamily.FareFamily;
                        //only valid for first cycle or loop
                        if (count == 0 && prevCabinCode == null)
                        {
                            prevCabinCode = ffRQ.CabinCode;
                            prevFareFamilies = ffRQ.FareFamily;
                            commonFarefamilies = ffRQ.FareFamily;
                            count++;
                        }
                        //it will go in this else only if cabin code changes, so that it can take common from prev cabin code and add it to total list
                        else if (prevCabinCode != null && count > 0 && prevCabinCode != ffRQ.CabinCode)
                        {
                            totalFarefamilies = totalFarefamilies.Concat(commonFarefamilies);
                            prevCabinCode = ffRQ.CabinCode;
                            prevFareFamilies = ffRQ.FareFamily;
                            commonFarefamilies = ffRQ.FareFamily;
                        }
                        //it will go in this else only if prev cabin code and current cabin code matches, so that we can take common farefamilies
                        else
                        {
                            commonFarefamilies = !commonFarefamilies.Any() ? prevFareFamilies.Intersect(ffRQ.FareFamily) : commonFarefamilies.Intersect(ffRQ.FareFamily);
                            prevFareFamilies = ffRQ.FareFamily;
                            count++;
                        }
                        found = true;
                        if (found)
                            break;
                    }
                }
                if (!found)
                {
                    throw new System.Exception(Constants.Application.CombinationNotAllowed);
                }
            }

            totalFarefamilies = totalFarefamilies.Concat(commonFarefamilies);

            if (!totalFarefamilies.Any())
                throw new System.Exception(Constants.Application.CombinationNotAllowed);


            return totalFarefamilies;
        }

        private static IEnumerable<string> GetFarePreferences(OfferCriteriaType offerCriteria)
        {
            List<string> fareTypeCodes = new List<string>();

            if (offerCriteria != null && offerCriteria.FareCriteria != null && offerCriteria.FareCriteria.Count() > 0)
            {
                offerCriteria.FareCriteria.ToList().ForEach(ftc =>
                {
                    fareTypeCodes.AddRange(ftc.FareTypeCode);
                });
            }

            return fareTypeCodes;
        }
    }
}
