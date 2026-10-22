using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment
{
    public sealed class WingsAssessmentMemberBatch
    {
        public WingsAssessmentMemberBatch(IReadOnlyCollection<GoldMemberDetails> members, bool hasMoreUnprocessedRecords)
        {
            Members = members;
            HasMoreUnprocessedRecords = hasMoreUnprocessedRecords;
        }

        public IReadOnlyCollection<GoldMemberDetails> Members { get; }

        public bool HasMoreUnprocessedRecords { get; }
    }
}
