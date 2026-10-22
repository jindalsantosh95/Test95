using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using Application = VAA.EAPI.GraphQL.Common.Constants.Application;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public static class StampingRequestMapper
    {
        public static CustomerDTO CusotmerDetailsMapper(CreateCustomerAccountRequest request, string fcNumber)
        {
            if (request.CustomerDetails == null) { return null; }

            return new CustomerDTO
            {
                Customer = new CustomerCustMS
                {
                    FirstName = request?.CustomerDetails?.FirstName,
                    MiddleName = request?.CustomerDetails?.MiddleName,
                    LastName = request?.CustomerDetails.LastName,
                    ContactDetails = new Contact
                    {
                        EmailDetails =
                        [
                            new() {
                                emailId = request?.CustomerDetails?.ContactDetails?.EmailDetails?.FirstOrDefault(x => x.PreferredFlag == Application.PrimaryIndicator)?.EmailId,
                                PreferredFlag = Application.PrimaryIndicator,
                            }
                        ]
                    },
                    LoyaltyNumber = fcNumber
                }
            };
        }
    }
}
