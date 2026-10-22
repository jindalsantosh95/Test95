using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    public class MealSelection
    {
        public bool IsSelected { get; set; }
        public string? SelectedChoice { get; set; }
        public IEnumerable<MealChoice>? AvailableChoices { get; set; }
        public bool IsEligible { get; set; }
        public bool IsChangeAllowed { get; set; }

        public static explicit operator MealSelection((Infrastructure.Models.SSRMS.ServiceRequest serviceRequest, IEnumerable<SSRMealRefData> ssrMealDetailsRefData) input)
        {
            var (serviceRequest, ssrMealDetailsRefData) = input;
            if (serviceRequest == null)
                return null;

            return new MealSelection
            {
                IsSelected = serviceRequest.SelectedChoice.IsNullOrEmpty() ? true : serviceRequest.IsSelected,
                SelectedChoice = SetSelectedMealCode(serviceRequest.SelectedChoice),
                AvailableChoices = SortedMealChoices(serviceRequest, ssrMealDetailsRefData),
                IsEligible = serviceRequest.IsEligible,
                IsChangeAllowed = serviceRequest.CanModify
            };
        }

        private static string SetSelectedMealCode(string selectedChoice)
        {
            if (selectedChoice.IsNullOrEmpty()) return Application.StandardMealCategoryChoice;
            if (selectedChoice == "LFML" || selectedChoice == "LSML") return "DBML" ;
            return selectedChoice;
        }

        private static IEnumerable<MealChoice> SortedMealChoices(ServiceRequest serviceRequest, IEnumerable<SSRMealRefData> ssrMealDetailsRefData)
        {
            var availbleChoices = serviceRequest.AvailableChoices?.Select(code => new MealChoice { Code = GetMealVsCode(code, ssrMealDetailsRefData), Description = GetMealDescription(code, ssrMealDetailsRefData) }) ?? new List<MealChoice>();

            return availbleChoices?
                .OrderBy(d => d.Code == Application.StandardMealCategoryChoice ? 0 : 1)
                .ThenBy(d => d.Code == Application.StandardMealCategoryChoice ? null : d.Description);
        }

        private static string GetMealVsCode(string dlCode, IEnumerable<SSRMealRefData> mealDetails)
        {
            return mealDetails.FirstOrDefault(md => md.Category == Application.SpecialMealCategory)?.CategoryDetails?.FirstOrDefault(d => d?.DlCode == dlCode)?.VsCode ?? dlCode;
        }

        private static string GetMealDescription(string mealCode, IEnumerable<SSRMealRefData> mealDetails)
        {
            if (mealCode == Application.StandardMealCategoryChoice) return Application.StandardMealCategoryTitle;

            return mealDetails.FirstOrDefault(md => md.Category == Application.SpecialMealCategory)?.CategoryDetails?.FirstOrDefault(d => d?.VsCode == mealCode)?.Title ?? string.Empty;
        }
    }
}
