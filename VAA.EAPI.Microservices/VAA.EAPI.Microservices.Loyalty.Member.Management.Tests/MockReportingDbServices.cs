using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public sealed class MockReportingDbServices : IReportingDBServices
    {
        public WingsAssessmentStagingResult StagingResult { get; set; } = new(true, "Success", 0);
        public WingsAssessmentMemberBatch MemberBatch { get; set; } = new(Array.Empty<GoldMemberDetails>(), false);
        public Exception StagingException { get; set; }
        public int StageGoldMembersCallCount { get; private set; }
        public int GetBatchCallCount { get; private set; }

        public Task<WingsAssessmentStagingResult> StageGoldMemberDetailsForWingsAssessment()
        {
            StageGoldMembersCallCount++;
            return StagingException == null
                ? Task.FromResult(StagingResult)
                : Task.FromException<WingsAssessmentStagingResult>(StagingException);
        }

        public Task<WingsAssessmentMemberBatch> GetWingsMemberAssessmentBatch(int memberCount)
        {
            GetBatchCallCount++;
            return Task.FromResult(MemberBatch);
        }

        public Task<IEnumerable<MemberContactData>> GetMembersBirthdayEmailDetails() => throw new NotSupportedException();
        public Task<IEnumerable<BirthdayBonusMemberDetails>> GetMembersDetailsForBirthdayBonus(DateTime birthdayDateForRun) => throw new NotSupportedException();
        public Task<IEnumerable<PromotionEnrolledMembers>> GetEnrolledMemberDetails(string promotionId) => throw new NotSupportedException();
        public Task<string> DeleteTargetedMembership(string customerId, string documentId) => throw new NotSupportedException();
    }
}
