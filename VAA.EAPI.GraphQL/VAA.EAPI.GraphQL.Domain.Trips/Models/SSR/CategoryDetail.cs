
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    public class CategoryDetail
    {
        public string? ServiceCode { get; set; }
        public string? ServiceName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEligible { get; set; }
        public IEnumerable<string>? Features { get; set; } = [];
        public bool IsChangeAllowed { get; set; }

        public static explicit operator CategoryDetail((Infrastructure.Models.SSRMS.ServiceRequest serviceRequest, SSRMealRefData ssrMealDetailsRefData) input)
        {
            var (serviceRequest, ssrMealDetailsRefData) = input;
            if (serviceRequest == null) return null;

            if (serviceRequest.ServiceTypeCode == Application.NoAssitAccessibleTravelCode)
                return new()
                {
                    ServiceCode = serviceRequest.ServiceTypeCode,
                    ServiceName = Application.NoAssitAccessibleTravelTitle,
                    IsSelected = serviceRequest.IsSelected,
                    IsEligible = serviceRequest.IsEligible,
                    IsChangeAllowed = serviceRequest.CanModify,
                    Features = []
                };

            var selectedCategory = ssrMealDetailsRefData.CategoryDetails?.FirstOrDefault(cd => cd?.DlCode == serviceRequest.ServiceTypeCode);

            return new CategoryDetail
            {
                ServiceCode = selectedCategory?.VsCode ?? serviceRequest.ServiceTypeCode,
                ServiceName = selectedCategory?.Title,
                IsSelected = serviceRequest.IsSelected,
                IsEligible = serviceRequest.IsEligible,
                IsChangeAllowed = serviceRequest.CanModify,
                Features = selectedCategory?.Features
            };
        }
    }
}