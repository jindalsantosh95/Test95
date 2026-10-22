namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.WingsGoldMemberAssessment
{
    public sealed class WingsAssessmentStagingResult
    {
        public WingsAssessmentStagingResult(bool isSuccessful, string statusMessage, int rowsInserted)
        {
            IsSuccessful = isSuccessful;
            StatusMessage = statusMessage;
            RowsInserted = rowsInserted;
        }

        public bool IsSuccessful { get; }

        public string StatusMessage { get; }

        public int RowsInserted { get; }
    }
}
