using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class UpdateMemberPhoneNumberMapper
    {
        public static UpdateMemberPhoneNumberRequest GetUpdateMemberPhoneNumber(UpdatePhoneNumber request, string FcNumber, string CustomerId)
        {
            return new UpdateMemberPhoneNumberRequest()
            {
                PhoneNumber = request.PhoneNumber,
                CountryCode = request.CountryCode,
                FcNumber = FcNumber,
                CustomerID = CustomerId

            };
        }
    }
}
