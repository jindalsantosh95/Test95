namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public enum DocumentType
    {
        Customer,
        CustomerProfile,
        FlyingClubMember,
        CustomerSearch,
        MembershipChange,
        PartnerMembership,
        Award,
        AddressHistory,
        ContactHistory
    }

    public enum ConsentFor
    {
        FlyingClubGuardianship,
        FlyingClubAuthorizedUserAccount
    }

    public enum ContactType
    {
        Email,
        Phone
    }

    public enum TierCode
    {
        Red,
        Silver,
        Gold,
        Glass,
        VAC
    }

}
