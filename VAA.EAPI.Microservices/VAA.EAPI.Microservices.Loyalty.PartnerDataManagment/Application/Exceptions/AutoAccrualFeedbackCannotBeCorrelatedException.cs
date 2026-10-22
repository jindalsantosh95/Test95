namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Exceptions
{
    public sealed class AutoAccrualFeedbackCannotBeCorrelatedException(string uniqueIdentificationNumber)
        : Exception("No matching eligible flown record was found for the feedback unique identification number.")
    {
        public string UniqueIdentificationNumber { get; } = uniqueIdentificationNumber;
    }
}
