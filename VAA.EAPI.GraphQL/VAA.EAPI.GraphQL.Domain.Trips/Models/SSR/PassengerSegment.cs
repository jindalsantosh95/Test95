using HotChocolate;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;
using SSRMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlPassengerSegment")]
    public class PassengerSegment
    {
        public FlightSegment? FlightSegment { get; set; }
        public IEnumerable<PassengerServiceRequest>? ServiceRequests { get; set; }
        public MealSelection? MealSelection { get; set; }

        public static explicit operator PassengerSegment((SSRMS.SsrSegment ssrSegment, PassengerConversionType type, IEnumerable<SSRMealRefData> ssrMealDetailsRefData, IEnumerable<AirportRefData> airports) input)
        {
            var (ssrSegment, type, ssrMealDetailsRefData, airports) = input;
            if (ssrSegment == null)
                return null;

            IEnumerable<SSRMS.ServiceRequest> matchingRequests = Enumerable.Empty<SSRMS.ServiceRequest>();
            switch (type)
            {
                case PassengerConversionType.MEAL:
                    matchingRequests = ssrSegment.ServiceRequests
                        .Where(sr => sr.ServiceCategory == Application.SpecialMealCategory);
                    matchingRequests = AddStandardMealCategory(matchingRequests);
                    break;

                case PassengerConversionType.SPECIALASSISTANCE:
                    var allowedCategories = Application.SpecialAssistanceCategory.Split(',');
                    matchingRequests = ssrSegment.ServiceRequests
                        .Where(sr => allowedCategories.Contains(sr.ServiceCategory));
                    matchingRequests = matchingRequests.Append(AddNoAssist(matchingRequests, Application.WheelChairAssistCateogyCode));
                    matchingRequests = matchingRequests.Append(AddNoAssist(matchingRequests, Application.BlindAssistCategoryCode));
                    matchingRequests = matchingRequests.Append(AddNoAssist(matchingRequests, Application.DeafAssistCategoryCode));
                    break;

                default:
                    return null;
            }

            var segment = new PassengerSegment
            {
                FlightSegment = (FlightSegment)(ssrSegment.FlightSegment, airports),
                MealSelection = type == PassengerConversionType.MEAL && matchingRequests.Any()
                                ? (MealSelection)(matchingRequests.First(), ssrMealDetailsRefData)
                                : null,
                ServiceRequests = type == PassengerConversionType.SPECIALASSISTANCE
                                ? MapToPassengerServiceRequest(matchingRequests, ssrMealDetailsRefData)
                                : null
            };

            return segment;
        }

        private static IEnumerable<SSRMS.ServiceRequest> AddStandardMealCategory(IEnumerable<SSRMS.ServiceRequest> matchingRequests)
        {
            var choices = matchingRequests.FirstOrDefault()?.AvailableChoices.ToList();

            if (matchingRequests.FirstOrDefault()?.SelectedChoice.IsNullOrEmpty() == false && !choices.Any(s => s == matchingRequests.FirstOrDefault()?.SelectedChoice)) 
                choices.Add(matchingRequests.FirstOrDefault().SelectedChoice);

            choices.Add(Application.StandardMealCategoryChoice);

            if (matchingRequests.Any())
                matchingRequests.First().AvailableChoices = choices ?? [];

            return matchingRequests;
        }

        private static SSRMS.ServiceRequest AddNoAssist(IEnumerable<SSRMS.ServiceRequest> matchingRequests, string categoryCode)
        {
            return new SSRMS.ServiceRequest
            {
                ServiceTypeCode = Application.NoAssitAccessibleTravelCode,
                ServiceTypeName = Application.NoAssitAccessibleTravelTitle,
                IsEligible = matchingRequests.Any(mr => mr.IsEligible),
                IsSelected = !matchingRequests.Any(mr => mr.ServiceCategory == categoryCode && mr.IsSelected),
                ServiceCategory = categoryCode,
                SelectedChoice = Application.NoAssitAccessibleTravelCode
            };
        }


        private static IEnumerable<PassengerServiceRequest> MapToPassengerServiceRequest(IEnumerable<SSRMS.ServiceRequest> matchingRequests, IEnumerable<SSRMealRefData> ssrMealDetailsRefData)
        {
            if (matchingRequests == null)
                return Enumerable.Empty<PassengerServiceRequest>();

            return matchingRequests
                .GroupBy(mr => new { mr.ServiceCategory })
                .Select(group => new PassengerServiceRequest
                {
                    CategoryCode = group.Key.ServiceCategory,
                    CategoryTitle = ssrMealDetailsRefData.FirstOrDefault(sm => sm.Category == group.Key.ServiceCategory)?.CategoryTitle,
                    CategoryDetails = group.Select(sr => (CategoryDetail)(sr, ssrMealDetailsRefData.FirstOrDefault(sm => sm.Category == group.Key.ServiceCategory)))
                    .OrderBy(cd => cd.ServiceCode == Application.NoAssitAccessibleTravelCode ? 0 : 1)
                    .ThenBy(d => d.ServiceCode == Application.NoAssitAccessibleTravelCode ? null : d.ServiceCode),
                    IsSegmentSelectEligible = group.Where(gr => gr.ServiceTypeCode != Application.NoAssitAccessibleTravelCode).Any(gr => gr.IsEligible),
                    IsServiceRequested = group.Where(gr => gr.ServiceTypeCode != Application.NoAssitAccessibleTravelCode).Any(gr => gr.IsSelected),
                    IsServiceChangeAllowed = group.Where(gr => gr.ServiceTypeCode != Application.NoAssitAccessibleTravelCode).Any(gr => gr.CanModify)
                });
        }
    }
}