using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper
{
    public static class GetLastName
    {
        public static string PopulateLastName(CreateCustomerEntities createCustomerEntities)
        {
            return createCustomerEntities?.TestMember == true ? Constants.Application.VirginTest : createCustomerEntities?.LastName;
        }
    }
}
