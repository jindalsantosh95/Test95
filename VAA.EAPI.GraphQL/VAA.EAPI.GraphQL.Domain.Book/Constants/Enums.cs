namespace VAA.EAPI.GraphQL.Domain.Book.Constants
{
    public enum FQTValidationStatus
    {
        Unchecked,
        Valid,
        InvalidAccount,
        NameMismatch
    }

    public enum FQTTier
    {
        FF,
        FO,
        GM
    }

    public enum AllianzCustomerType
    {
        PRIMARY,
        OTHER,
        CHILD,
        INFANT
    }

    public enum CoBrandStatusEnum
    {
        NotApplied,
        Initiated,
        Approved,
        Rejected,
        Pending,
        ApprovedInsufficientCredit,
        Error
    }
}