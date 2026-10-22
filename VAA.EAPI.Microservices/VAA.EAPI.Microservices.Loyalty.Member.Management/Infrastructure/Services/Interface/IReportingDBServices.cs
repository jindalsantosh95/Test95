using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface IReportingDBServices
    {
        Task<IEnumerable<MemberContactData>> GetMembersBirthdayEmailDetails();

        Task<IEnumerable<BirthdayBonusMemberDetails>> GetMembersDetailsForBirthdayBonus(DateTime birthdayDateForRun);
        Task<WingsAssessmentStagingResult> StageGoldMemberDetailsForWingsAssessment();
        Task<WingsAssessmentMemberBatch> GetWingsMemberAssessmentBatch(int memberCount);
        Task<IEnumerable<PromotionEnrolledMembers>> GetEnrolledMemberDetails(string promotionId);
        Task<string> DeleteTargetedMembership(string customerId, string documentId);
    }
}
