using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Constants;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.BspMarket;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.FareFamily;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using SeatAvailabilityRQ = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;

namespace VAA.EAPI.NDC.Offer.Validations
{
    public class RequestValidations
    {
        public static List<ErrorDetail> ValidateCabinCodes(ShoppingRequestCabinTypeType[] cabinTypes)
        {
            var errors = new List<ErrorDetail>();

            if (cabinTypes != null && cabinTypes.Length > 0)
            {
                var cabinCodes = new List<int>();
                var validateSpecialcharacter = true;

                foreach (var y in cabinTypes)
                {
                    if ((!RegexCompiled.Match(y.CabinTypeCode, "^[2-5]*$").Success) || string.IsNullOrWhiteSpace(y.CabinTypeCode))
                    {
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "12_3",
                            IsDynamicMessage = false
                        });
                        validateSpecialcharacter = false;
                    }
                }

                if (validateSpecialcharacter)
                {
                    foreach (var y in cabinTypes)
                    {
                        cabinCodes.Add(Convert.ToInt32(y.CabinTypeCode));

                        if ((cabinCodes.Min() < (int)(ITACabinType.UpperClass) || cabinCodes.Max() > (int)(ITACabinType.EconomyClassic)))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "12_3",
                            });
                        }
                    }
                }

                //Validate against Generic Config
                foreach (var c in cabinTypes)
                {
                    var found = Enum.IsDefined(typeof(CabinCodes), Convert.ToInt32(c.CabinTypeCode));

                    if (!found && errors.Count == 0)
                    {
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "12_3",
                            IsDynamicMessage = false
                        });
                    }
                }
            }
            return errors;
        }

        public static List<ErrorDetail> ValidatePaxList(PaxType[] customers)
        {
            var errors = new List<ErrorDetail>();

            if (customers == null || customers.Length == 0)
            {
                errors.Add(new ErrorDetail
                {
                    IsDynamicMessage = false,
                    ErrorCode = "143_1",
                });
            }
            else
            {
                var countAdt = customers.Count(x => x.PTC == Constants.Application.AdultPTCCode);
                var countChd = customers.Count(x => x.PTC == Constants.Application.ChildPTCCode);
                var countYoung = customers.Count(x => x.PTC == Constants.Application.YoungAdultPTCCode);
                var countInf = customers.Count(x => x.PTC == Constants.Application.InfantPTCCode);
                var countDistinctPaxId = customers.Select(x => x.PaxID).Distinct().Count();

                if (countAdt < countInf)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "324_1",
                    });
                }
                if ((countAdt + countChd + countYoung) > 9)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "68_1",
                    });
                }
                if (((countAdt + countChd + countInf + countYoung) != customers.Length) || countAdt == 0 || countDistinctPaxId < customers.Length)
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "143_2",
                    });
                }
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateOriginDestinations(OriginDestCriteriaType[] originDestinationList)
        {
            bool isValid = true;
            var errors = new List<ErrorDetail>();

            // Max of 6 O&Ds are supported
            var TotalNumberOfOriginsDestinations = Convert.ToInt32(Constants.Application.OriginsDestinationsCount);

            if (originDestinationList.Length > TotalNumberOfOriginsDestinations)
            {
                if (errors.Any(x => x.ErrorCode == "724_1"))
                {
                    errors.Add(new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "724_1",
                    });
                }
            }

            //Validate OnD should be 3 letter char
            foreach (var originDestination in originDestinationList)
            {
                var checkOnD = RegexCompiled.IsMatch(originDestination?.OriginDepCriteria?.IATA_LocationCode, @"^[A-Z]{3}$") && RegexCompiled.IsMatch(originDestination?.DestArrivalCriteria.IATA_LocationCode, @"^[A-Z]{3}$");
                if (!checkOnD)
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "130_1",
                        IsDynamicMessage = false
                    });
                var checkOnDMatch = (bool)originDestination?.OriginDepCriteria?.IATA_LocationCode.Equals(originDestination?.DestArrivalCriteria?.IATA_LocationCode);
                if (checkOnDMatch)
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "130_1",
                        IsDynamicMessage = false
                    });

                }
                string[] invalidDest = Constants.Application.InvalidRoute.Split(';');
                foreach (string OnD in invalidDest)
                {
                    if (((bool)originDestination?.DestArrivalCriteria?.IATA_LocationCode.Equals("LHR") && (bool)originDestination?.OriginDepCriteria?.IATA_LocationCode.Equals(OnD)) || ((bool)originDestination?.DestArrivalCriteria?.IATA_LocationCode.Equals(OnD) && (bool)originDestination?.OriginDepCriteria?.IATA_LocationCode.Equals("LHR")))
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "100_2",
                            IsDynamicMessage = false
                        });
                    }
                }
                var originDepCriteriaDate = originDestination?.OriginDepCriteria?.Date;
                if (originDepCriteriaDate.HasValue && (originDepCriteriaDate.Value.Date < DateTime.Now.Date || originDepCriteriaDate.Value.Date > DateTime.Now.AddYears(1)))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "ITA_376",
                        IsDynamicMessage = false
                    });

                }
                // Validate OnD tolerance window
                var departureDate = Convert.ToDateTime(originDestination?.OriginDepCriteria?.Time.Hour == 0 ? originDestination?.OriginDepCriteria?.Date : originDestination?.OriginDepCriteria?.Date.AddHours(double.Parse(originDestination?.OriginDepCriteria?.Time.Hour.ToString())));
                if (departureDate.Hour > 0)
                {
                    if (string.IsNullOrWhiteSpace(originDestination?.OriginDepCriteria.TimeAfterDuration) || string.IsNullOrWhiteSpace(originDestination?.OriginDepCriteria?.TimeBeforeDuration))
                    {
                        isValid = false;
                    }
                    else
                    {
                        string timeAfterDuration = RegexCompiled.Match(originDestination.OriginDepCriteria?.TimeAfterDuration, Constants.Application.RegexToValidateNumeric).Value;
                        string timeBeforeDuration = RegexCompiled.Match(originDestination.OriginDepCriteria?.TimeBeforeDuration, Constants.Application.RegexToValidateNumeric).Value;

                        if (string.IsNullOrWhiteSpace(timeAfterDuration) || string.IsNullOrWhiteSpace(timeBeforeDuration))
                        {
                            isValid = false;
                        }
                        else
                        {
                            var timeAfterDurationHour = int.Parse(timeAfterDuration);
                            var timeBeforeDurationHour = int.Parse(timeBeforeDuration);
                            if (timeAfterDurationHour == 0 || timeBeforeDurationHour == 0)
                            {
                                isValid = false;
                            }

                            var departureRequestedAfter = departureDate;
                            var departureRequestedBefore = departureDate;
                            departureRequestedAfter = departureRequestedAfter.AddHours(timeAfterDurationHour);
                            departureRequestedBefore = departureRequestedBefore.AddHours(-timeBeforeDurationHour);
                            if ((departureRequestedAfter.Date != departureDate.Date) || (departureDate.Date != departureRequestedBefore.Date))
                            {
                                isValid = false;
                            }
                        }
                    }
                }
                if (!isValid && !errors.Exists(x => x.ErrorCode == "103"))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "103",
                        IsDynamicMessage = false
                    });
                }
            }

            //New Validation added for calender search
            foreach (var originDestination in originDestinationList)
            {
                var calendarSearch = originDestination?.CalendarDateCriteria;
                if (calendarSearch != null)
                {
                    if (calendarSearch?.DaysAfterNumber == null || calendarSearch?.DaysBeforeNumber == null)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "102_3",
                            IsDynamicMessage = false
                        });
                    }
                    else if (calendarSearch?.DaysBeforeNumber != 3 || calendarSearch?.DaysAfterNumber != 3)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "102_4",
                            IsDynamicMessage = false
                        });
                    }
                }
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateCalendarSearchRQ(IATA_AirShoppingRQ airShoppingRQ)
        {
            var errors = new List<ErrorDetail>();
            var flightOriginDestinationCriteria = (FlightRequestOriginDestinationsCriteriaType)airShoppingRQ?.Request?.FlightRequest?.Item;
            bool directFlight = airShoppingRQ?.Request?.FlightRelatedCriteria?.FlightCriteria?.FirstOrDefault()?.FlightCharacteristicsCriteria?.FirstOrDefault()?.CharacteristicCode.ToString() == Constants.Application.Direct;
            bool calendarSearch = flightOriginDestinationCriteria?.OriginDestCriteria.FirstOrDefault()?.CalendarDateCriteria != null;
            bool toleranceWindow = flightOriginDestinationCriteria.OriginDestCriteria.Any(onD => onD.OriginDepCriteria?.Time.Hour > 0 || onD.OriginDepCriteria?.Time.Minute > 0 || onD.OriginDepCriteria?.Time.Second > 0);

            if (calendarSearch && (directFlight || toleranceWindow))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "915_1",
                    IsDynamicMessage = false
                });
            }
            return errors;
        }

        public static List<ErrorDetail> ValidateRequestAgentDetails(DistributionChainLinkType[] distributionChain)
        {
            var errors = new List<ErrorDetail>();
            foreach (var dC in distributionChain)
            {
                if (dC.ParticipatingOrg == null || string.IsNullOrEmpty(dC.ParticipatingOrg.OrgID))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "912_1",
                        IsDynamicMessage = false
                    });
                }
                else if ((dC.SalesAgent != null && string.IsNullOrEmpty(dC.SalesAgent.SalesAgentID)) || (dC.SalesBranch != null && string.IsNullOrEmpty(dC.SalesBranch.SalesBranchID)))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "912_1",
                        IsDynamicMessage = false
                    });
                }

                else if (string.IsNullOrEmpty(dC.Ordinal) || (dC.Ordinal.Equals("0")) || (dC.Ordinal.Any(Char.IsLetter)) || (dC.OrgRole == DistributionChainRoleCodeType.Seller && !dC.Ordinal.StartsWith('1')))
                {
                    errors.Add(new ErrorDetail
                    {
                        ErrorCode = "912_24",
                        IsDynamicMessage = false
                    });
                }
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateRequestFareType(AirShoppingRequestType airShoppingRequestType)
        {
            var errors = new List<ErrorDetail>();

            if (airShoppingRequestType.OfferCriteria == null || airShoppingRequestType.OfferCriteria.FareCriteria.Length == 0)
            {
                errors.Add(new ErrorDetail()
                {
                    ErrorCode = "143_5",
                    IsDynamicMessage = false
                });
            }
            return errors;
        }

        public static List<ErrorDetail> ValidateAgencyDetails(ValidationRS validationRS)
        {
            var errors = new List<ErrorDetail>();

            if (validationRS != null && validationRS.Error != null)
            {
                foreach (var error in validationRS.Error)
                {
                    if (error?.Code == "G009")
                    {
                        if (!errors.Any(error => error.ErrorCode == "911_22"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "911_22"
                            });
                        }
                    }
                    else if (error?.Code == "10144")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_2"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_2"
                            });
                        }
                    }
                    else if (error?.Code == "10146")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_3"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_3"
                            });
                        }
                    }
                    else if (error?.Code == "10147")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_4"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_4"
                            });
                        }
                    }
                    else if (error?.Code == "10145")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_5"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_5"
                            });
                        }
                    }
                    else if (error?.Code == "10140")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_6"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_6"
                            });
                        }
                    }
                    else if (error?.Code == "10152")
                    {
                        if (!errors.Any(error => error.ErrorCode == "368_8"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "368_8"
                            });
                        }
                    }
                    else if (error?.Code == "10153")
                    {
                        if (!errors.Any(error => error.ErrorCode == "911_44"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "911_44"
                            });
                        }
                    }
                    else
                    {
                        if (!errors.Any(error => error.ErrorCode == "911_22"))
                        {
                            errors.Add(new ErrorDetail
                            {
                                IsDynamicMessage = false,
                                ErrorCode = "911_22"
                            });
                        }
                    }
                }
            }
            return errors;
        }

        public static IEnumerable<NDCWarning> ValidateCurrencyDetails(IATA_AirShoppingRQ airShoppingRQ, OfferResponse offers, BspMarketCurrencyData currencyData, ValidationRS validationRS, CalendarSearchResponse calendarSearchResponse)
        {
            bool isValid = true;

            if (validationRS.ValidateResponse.AgencyDetails.RequestAgencyIdType == "IATA")
            {
                if ((airShoppingRQ.Request.ResponseParameters?.CurParameter?.FirstOrDefault()?.CurCode != currencyData.DualCurrencyCode && airShoppingRQ.Request.ResponseParameters?.CurParameter?.FirstOrDefault()?.CurCode != currencyData.LocalCurrencyCode))
                {
                    isValid = false;
                }
            }
            else
            {
                if (airShoppingRQ.Request.ResponseParameters?.CurParameter?.FirstOrDefault()?.CurCode != offers.Offers?.FirstOrDefault()?.SalePrice?.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency && airShoppingRQ.Request.ResponseParameters?.CurParameter?.FirstOrDefault()?.CurCode != calendarSearchResponse?.FlexibleDateCalendar?.DepartureDetails?.FirstOrDefault()?.FlexibleDatePriceCalendarCell?.FirstOrDefault()?.SalePrice?.FirstOrDefault(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum)?.Amount?.BaseCurrency)
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                return new NDCWarning[] {
                    new()
                    {
                        Code = "71_A",
                        IsDynamicMessage = false
                    }
                };
            }
            else
                return Enumerable.Empty<NDCWarning>();
        }

        public static IEnumerable<ErrorDetail> ValidateAgentID_OfferServiceList(AgencyValidateResponse validateResponse, OfferResponse offerResponse)
        {
            if (validateResponse == null || offerResponse == null)
                return Enumerable.Empty<ErrorDetail>();

            return offerResponse.Parties.Participants
             .Where(agent => agent.AgencyID != validateResponse.AgencyDetails.RequestAgencyId && agent.Name != validateResponse.AgencyDetails.RequestOrgName)
             .Select(participant =>
             new ErrorDetail()
             {
                 IsDynamicMessage = false,
                 ErrorCode = "368_2",
             });
        }

        private static string GetFormattedACCT(AgencyValidateResponse validationRS)
        {
            if (validationRS.KeyType == Constants.Application.KeyTypeDirect)
            {
                return string.Concat(string.Format(Constants.Application.ACCTFormat, validationRS.AgencyDetails.RequestAgencyIdType, validationRS.AgencyDetails.RequestAgencyId, validationRS.AgencyDetails.RequestOrgName, string.Empty, string.Empty).Take(Constants.Application.ACCTFormatMaxLength));
            }
            else
            {
                return string.Concat(string.Format(Constants.Application.ACCTFormat, validationRS.AgencyDetails.RequestAgencyIdType, validationRS.AgencyDetails.RequestAgencyId, validationRS.AgencyDetails.RequestOrgName, validationRS.AggregatorDetails.AgencyId, validationRS.AggregatorDetails.CompanyName).Take(Constants.Application.ACCTFormatMaxLength));
            }
        }


        public static List<ErrorDetail> ValidateSeatsInPNR()
        {
            var errors = new List<ErrorDetail>();

            errors.Add(new ErrorDetail()
            {
                IsDynamicMessage = false,
                ErrorCode = "197_1",
            });

            return errors;
        }

        public static List<ErrorDetail> ValidateAgentID(AgencyValidateResponse validateResponse, OrderDocument orderDocResponse)
        {
            var errors = new List<ErrorDetail>();
            string remarks = orderDocResponse?.OrderRemarks.First().Text;
            string acctRemark = GetFormattedACCT(validateResponse);

            if (orderDocResponse != null && !remarks.Contains(acctRemark))
            {
                errors.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_2",
                });
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateAgentIDForServiceList(AgencyValidateResponse validateResponse, OrderDocument orderDocResponse)
        {
            var errors = new List<ErrorDetail>();
            var remarks = orderDocResponse?.OrderRemarks.ToList();
            string acctRemark = GetFormattedACCT(validateResponse);

            if (orderDocResponse != null && !remarks.Any(x => x.Text.Is(acctRemark)))
            {
                errors.Add(new ErrorDetail()
                {
                    IsDynamicMessage = false,
                    ErrorCode = "368_2",
                });
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateTaxonomyCode(ServiceCriteriaType[] serviceCriterias)
        {
            var errors = new List<ErrorDetail>();
            var validTaxonomyCodes = Constants.Application.ServiceTaxonomyCodes;

            if (serviceCriterias != null)
            {
                foreach (var serviceCriteria in serviceCriterias)
                {
                    if (serviceCriteria.TaxonomyCode != null && serviceCriteria.TaxonomyCode.Length > 0)
                    {
                        foreach (var c in serviceCriteria.TaxonomyCode)
                        {
                            var found = validTaxonomyCodes.Contains(c);
                            if (!found && errors.Count == 0)
                            {
                                errors.Add(new ErrorDetail()
                                {
                                    ErrorCode = "12_4",
                                    IsDynamicMessage = false
                                });
                            }
                        }
                    }
                }
            }

            return errors;
        }

        public static List<ErrorDetail> ValidateFareTypeCode(FareCriteriaType[] farePreferences)
        {
            var errors = new List<ErrorDetail>();
            var validFareTypeCodes = Constants.Application.FareTypeCodes;
            var validFareSubTypeCode = Constants.Application.FareSubTypeCodes;

            if (farePreferences != null && farePreferences.Length > 0)
            {
                if (farePreferences.Length > 1)
                {
                    errors.Add(new ErrorDetail()
                    {
                        ErrorCode = "12_6",
                        IsDynamicMessage = false
                    });
                }

                foreach (var f in farePreferences)
                {
                    if (f.FareSubTypeCode != null && f.FareSubTypeCode.Length > 1 && errors.Count ==0)
                    {
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "914_15",
                            IsDynamicMessage = false
                        });
                    }

                    var found = validFareTypeCodes.Contains(f.FareTypeCode[0]);
                    if (!found && errors.Count == 0)
                    {
                        errors.Add(new ErrorDetail()
                        {
                            ErrorCode = "12_5",
                            IsDynamicMessage = false
                        });
                    }
                }

                if(farePreferences.Any(x => x.FareTypeCode.Contains(Constants.Application.FareTypeCode)))
                {
                    foreach (var f in farePreferences)
                    {
                        if(f.FareSubTypeCode != null && errors.Count == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "914_17",
                                IsDynamicMessage = false
                            });
                        }
                    }
                }

                if (farePreferences.Any(x => x.FareTypeCode.Contains(Constants.Application.CorporateFareTypeCode)))
                {
                    foreach (var f in farePreferences)
                    {
                        if (f.FareSubTypeCode == null && errors.Count == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "914_14",
                                IsDynamicMessage = false
                            });
                        }

                        if (f.FareSubTypeCode != null && f.FareSubTypeCode.Length > 0 && f.FareSubTypeCode[0].ToUpper() != Constants.Application.CorporateFareSubTypeCode && errors.Count == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "914_16",
                                IsDynamicMessage = false
                            });
                        }
                    }
                }

                if (farePreferences.Any(x => x.FareTypeCode.Contains(Constants.Application.PrivateFareTypeCode)))
                {
                    foreach (var f in farePreferences)
                    {
                        if (f.FareSubTypeCode == null && errors.Count == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "914_14",
                                IsDynamicMessage = false
                            });
                        }

                        var found = f.FareSubTypeCode != null && f.FareSubTypeCode.Length > 0 && validFareSubTypeCode.Any(x => x.Replace(" ", "").Is(f.FareSubTypeCode[0].Replace(" ", "")));
                        if (!found && errors.Count == 0)
                        {
                            errors.Add(new ErrorDetail()
                            {
                                ErrorCode = "914_16",
                                IsDynamicMessage = false
                            });
                        }
                    }
                }
            }
            return errors;
        }

        public static ResponseMessage NullSeatAvailabilityErrorResponseMessage(SeatAvailabilityRQ.IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            var responseMessage = new ResponseMessage();

            if (seatAvailabilityRQ == null)
            {
                responseMessage = new ResponseMessage()
                {
                    Errors = new[]
                    {
                        new ErrorDetails()
                        {
                            Code = Constants.Application.NDCNULLRQERRORCODE,
                            Message = Constants.Application.NDCNULLAIRSHOPPINGRQERRORMSG,
                            StatusText = Constants.Application.NDCNULLRQSTATUSCODE
                        }
                    }
                };

            }
            return responseMessage;
        }

        public static IEnumerable<ErrorDetail> ValidateAccountCodeDeatils(ValidationRS validationRS, IATA_AirShoppingRQ airShoppingRQ)
        {
            var errors = new List<ErrorDetail>();

            var accountCode = validationRS.ValidateResponse?.AgencyDetails?.AccountCode;
            var corporateId = airShoppingRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner?.Item as OrgType;
            var fareTypes = airShoppingRQ.Request?.OfferCriteria?.FareCriteria?.FirstOrDefault();

            if (fareTypes is null) return null;

            if (string.IsNullOrEmpty(accountCode) && fareTypes.FareTypeCode.Any(code => code == Constants.Application.PrivateFareTypeCode))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "75G_1" });

            if (errors.Count == 0 && fareTypes.FareTypeCode.Any(code => code == Constants.Application.CorporateFareTypeCode) && (corporateId?.OrgID != validationRS?.ValidateResponse?.CorporateDetails?.CorporateId || string.IsNullOrEmpty(validationRS?.ValidateResponse?.CorporateDetails?.CorporateAccountCode)))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "75G_1" });

            return errors;
        }

        public static List<ErrorDetail> ValidateAgentTicketingAuthority(ValidationRS validationRS)
        {
            var errors = new List<ErrorDetail>();

            var agencyPermissions = validationRS?.ValidateResponse?.AgencyPermissions;
            var agencyIdType = validationRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType;

            bool isIataAgent = agencyIdType != Constants.Application.TIDS;
            bool hasTicketingAuthority = agencyPermissions?.AirlineTicketingAuthorityInd == true;

            if (isIataAgent && !hasTicketingAuthority) { errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "368_8" }); }
            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidatePaxType(PaxType[] paxTypes, IEnumerable<ItaFareTypeCodeData> matchedPTCCode)
        {
            var errors = new List<ErrorDetail>();

            var matchedPtcSet = new HashSet<string>(matchedPTCCode.Select(x => x.NdcPaxType));
            var invalidPtcs = paxTypes.Select(p => p.PTC).Where(ptc => !matchedPtcSet.Contains(ptc));

            if (invalidPtcs.Any())
            {
                errors.Add(new ErrorDetail
                {
                     IsDynamicMessage = false,
                     ErrorCode = "143_2"
                });
            }
            return errors;
        }

    }
}