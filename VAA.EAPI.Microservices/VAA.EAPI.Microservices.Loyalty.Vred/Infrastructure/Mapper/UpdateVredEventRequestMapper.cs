using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper
{
    public class UpdateVredEventRequestMapper
    {
        public static UpdateVredCustomerDetailsCommand UpdateVredCommandReqMapper(CloudEvent manageAccountRequest)
        {
            return new UpdateVredCustomerDetailsCommand
            {
                Id = manageAccountRequest.Id,
                Data = manageAccountRequest.Data,
                Type = manageAccountRequest.Type,
                Subject = manageAccountRequest.Subject,
                Specversion = manageAccountRequest.Specversion
            };
        }
    }
}
