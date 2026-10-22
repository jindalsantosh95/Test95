using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdatePhoneNumberEventCommand: IRequest<Response<string>>
    {
        public string CustomerID { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string FcNumber { get; set; }

        public static explicit operator UpdatePhoneNumberEventCommand(UpdatePhoneNumber updatePhoneNumber)
        {
            return new()
            {
                PhoneNumber = updatePhoneNumber.PhoneNumber,
                CountryCode = updatePhoneNumber.CountryCode,
                CustomerID = updatePhoneNumber.CustomerId,
                FcNumber = updatePhoneNumber.FcNumber,

            };
        }

    }
}
