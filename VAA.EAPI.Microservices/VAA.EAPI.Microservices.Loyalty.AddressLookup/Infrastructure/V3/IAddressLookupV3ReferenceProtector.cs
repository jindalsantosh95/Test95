namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public interface IAddressLookupV3ReferenceProtector
    {
        string Protect(AddressLookupV3Reference reference);
        AddressLookupV3Reference Unprotect(string protectedReference);
    }
}
