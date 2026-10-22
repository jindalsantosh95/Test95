using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Helper
{
    public static class InRuleHelper
    {
        public static IEnumerable<EntityState> GetEntityStates(EarnAirlineAccrualCalculatorRequest earnAirlineAccrualCalculatorRequest)
        {
            return earnAirlineAccrualCalculatorRequest.FlightSegments.Select(segment => new EntityState
            {
                CorrelationId = Guid.NewGuid().ToString(),
                AirlineCodeMarketing = segment.MarketingCarrier,
                AirlineCodeOperating = segment.OperatingCarrier,
                FlightNumberMarketing = segment.MarketingFlightNumber,
                FlightNumberOperating = segment.OperatingFlightNumber,
                FlightDate = segment.ScheduledDepartureTime.ToString(),
                FareBasisCode = segment.FareBasisCode.IsNullOrWhiteSpace() ? string.Empty : segment.FareBasisCode,               
                FareBrand = GetFareBrand(segment.FareBasisCode),
                Customers = (earnAirlineAccrualCalculatorRequest?.CustomerDetails != null && earnAirlineAccrualCalculatorRequest.CustomerDetails.Any())
                                ? earnAirlineAccrualCalculatorRequest.CustomerDetails.Select(cd => CreateCustomer(cd, segment)).ToList()
                                : new List<TierRuleSetEntityRequest> { CreateCustomer(null, segment) }
            });
        }

        public static RuleEngineRequest GetRuleEngineRequest(EntityState entityState)
        {
            return new RuleEngineRequest
            {
                RuleApplicationName = Constants.Application.LoyaltyProgramRule,
                RuleEntityName = Constants.Application.BasicPointAwardRule,
                RequestEntity = entityState
            };
        }

        public static TierRuleSetEntityRequest CreateCustomer(CustomerDetail customerDetail, FlightSegment segment)
        {
            var customerRequest = new TierRuleSetEntityRequest
            {
                CustomerId = customerDetail?.MemberNumber ?? Guid.NewGuid().ToString(),
                Origin = segment.OriginAirportCode,
                Destination = segment.DestinationAirportCode,
                Distance = segment.SegmentDistance.ToString(),
                OperatingBookedClass = segment.OperatingBookingClass,
                MarketingBookedClass = DetermineCreditedClass(segment.FareBasisCode, segment.FlownClass, segment.MarketingBookingClass),
                AirlineCodeMarketing = segment.MarketingCarrier,
                AirlineCodeOperating = segment.OperatingCarrier,
                CabinClass = segment.CabinClass,
                SalesLocation = segment.SalesLocation.IsNullOrWhiteSpace() ? string.Empty : segment.SalesLocation,
                AgentIataNumber = segment.AgentIataNumber.IsNullOrWhiteSpace() ? string.Empty : segment.AgentIataNumber,
                FlownClass = segment.FlownClass.IsNullOrWhiteSpace() ? string.Empty : segment.FlownClass,
                CustomerContext = new CustomerContext
                {
                    CurrentStatus = customerDetail?.MemberStatus ?? "Active",
                    CampaignOrPromotionCode = customerDetail?.CampaignOrPromotionCode ?? string.Empty,
                    CurrentTier = customerDetail?.TierCode ?? "Red",
                    CurrentTierStartDate = customerDetail?.CurrentTierStartDate ?? string.Empty,
                    MemberType = customerDetail?.MemberType ?? "Individual"
                }
            };

            if (!segment.OriginCountryCode.IsNullOrWhiteSpace())
                customerRequest.OriginCountryCode = segment.OriginCountryCode;

            if (!segment.DestinationCountryCode.IsNullOrWhiteSpace())
                customerRequest.DestinationCountryCode = segment.DestinationCountryCode;

            return customerRequest;
        }

        private enum CabinType
        {
            Unknown,
            Economy,
            Premium,
            Upper
        }
        public static string DetermineCreditedClass(string fareBasisCode, string flownClass, string bookingClass)
        {
            if (fareBasisCode.IsNullOrWhiteSpace() || flownClass.IsNullOrWhiteSpace() || bookingClass.IsNullOrWhiteSpace())
            {
                return bookingClass ?? string.Empty;
            }

            var economyClasses = GetConfiguredClasses(Constants.EnvironmentSettings.EconomyClasses);
            var premiumClasses = GetConfiguredClasses(Constants.EnvironmentSettings.PremEconomyClasses);
            var upperClasses = GetConfiguredClasses(Constants.EnvironmentSettings.UpperClasses);

            CabinType fareBasisCabin = GetCabinType(fareBasisCode, economyClasses, premiumClasses, upperClasses);
            CabinType bookingCabin = GetCabinType(bookingClass, economyClasses, premiumClasses, upperClasses);
            CabinType flownCabin = GetCabinType(flownClass, economyClasses, premiumClasses, upperClasses);

            return (fareBasisCabin, bookingCabin, flownCabin) switch
            {
                (CabinType.Premium, CabinType.Economy, CabinType.Premium) => flownClass,
                (CabinType.Upper, CabinType.Economy, CabinType.Upper) => flownClass,
                (CabinType.Upper, CabinType.Premium, CabinType.Upper) => flownClass,
                _ => bookingClass
            };
        }


        private static HashSet<char> GetConfiguredClasses(string configurationKey)
        {
            string configuredValue = ConfigReader.Try(configurationKey);
            if (!configuredValue.IsNullOrWhiteSpace())
            {
                return new HashSet<char>(configuredValue.ToUpperInvariant().ToCharArray());
            }

            return new HashSet<char>(); // Fallback to an empty set if the configuration is missing or empty
        }

        private static CabinType GetCabinType(string classCode, HashSet<char> economyClasses, HashSet<char> premiumClasses, HashSet<char> upperClasses)
        {
            if (classCode.IsNullOrWhiteSpace())
            {
                return CabinType.Unknown;
            }

            char classIndicator = char.ToUpperInvariant(classCode[0]);

            if (economyClasses.Contains(classIndicator))
            {
                return CabinType.Economy;
            }

            if (premiumClasses.Contains(classIndicator))
            {
                return CabinType.Premium;
            }

            if (upperClasses.Contains(classIndicator))
            {
                return CabinType.Upper;
            }

            return CabinType.Unknown;
        }

        public static string GetFareBrand(string fareBasisCode)
        {
            if (!fareBasisCode.IsNullOrWhiteSpace() && fareBasisCode.Length >= 7)
            {
                return fareBasisCode[6].ToString(); // Return the 7th character (index 6)
            }
            return null; // Return null if less than 7 characters
        }
    }
}
