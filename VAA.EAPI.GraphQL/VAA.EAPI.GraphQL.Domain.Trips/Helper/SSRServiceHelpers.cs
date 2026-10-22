using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public static class SSRServiceHelpers
    {
        public static IEnumerable<Passenger> GetEligibleSegments(PassengerSSRResponse msResponse, string passengerId, IEnumerable<string> segmentIds, bool applyToAll, string ssrCategoryCode)
        {
            var segmentList = segmentIds.Select(int.Parse);

            return msResponse.Passengers.Where(p => p.PassengerResourceId == passengerId).Select(p => new Passenger()
            {
                PassengerResourceId = p.PassengerResourceId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                SsrSegments = p.SsrSegments.Where(ps => applyToAll ? ps.ServiceRequests.Any(sr => sr.ServiceCategory == ssrCategoryCode && sr.IsEligible) : segmentList.Contains(ps.FlightSegment.SegmentId)).Select(ps => new SsrSegment()
                {
                    FlightSegment = ps.FlightSegment,
                    ServiceRequests = ps.ServiceRequests.Where(x => x.ServiceCategory == ssrCategoryCode),
                })
            }).Where(p => p.SsrSegments.Any());
        }

        public static Dictionary<string, string> GetServiceAction(IEnumerable<Passenger> requestedSegmentDetails, string requestedServiceCode, IEnumerable<string> segmentIds, IEnumerable<Models.ReferenceData.SSRMealRefData> refData, string ssrCategoryCode, string noChoiceCode)
        {
            bool isServiceSelected = requestedSegmentDetails.SelectMany(r => r.SsrSegments).Where(ssrSegment => segmentIds.Select(int.Parse).Contains(ssrSegment.FlightSegment.SegmentId)).Any(ssrSegment => ssrSegment.ServiceRequests.Any(s => s.IsSelected));
            string selectedServiceCode = isServiceSelected ? requestedSegmentDetails.SelectMany(r => r.SsrSegments).Where(ssrSegment => segmentIds.Select(int.Parse).Contains(ssrSegment.FlightSegment.SegmentId)).SelectMany(ssrSegment => ssrSegment.ServiceRequests.Where(s => s.IsSelected).Select(s => s.SelectedChoice)).FirstOrDefault() : null;

            var dlServiceCode = requestedServiceCode != noChoiceCode ? refData.Where(r => r.Category == ssrCategoryCode).SelectMany(c => c.CategoryDetails).Where(c => c.VsCode == requestedServiceCode).Select(c => !string.IsNullOrEmpty(c.DlRemark) ? c.DlRemark : c.DlCode).FirstOrDefault() : selectedServiceCode;

            if (!isServiceSelected && requestedServiceCode != noChoiceCode)
            {
                return new Dictionary<string, string>
                {
                    { "actionType", SSRModifyActionType.ADD.ToString()},
                    { "ssrCode", dlServiceCode }
                };
            }
            else if (isServiceSelected && !string.IsNullOrEmpty(selectedServiceCode) && requestedServiceCode != noChoiceCode)
            {
                return new Dictionary<string, string>
                {
                    { "actionType", SSRModifyActionType.EDIT.ToString()},
                    { "ssrCode", dlServiceCode }
                };
            }
            else if (isServiceSelected && !string.IsNullOrEmpty(selectedServiceCode) && requestedServiceCode == noChoiceCode)
            {
                return new Dictionary<string, string>
                {
                    { "actionType", SSRModifyActionType.DELETE.ToString()},
                    { "ssrCode", dlServiceCode }
                };
            }

            return null;
        }
    }
}