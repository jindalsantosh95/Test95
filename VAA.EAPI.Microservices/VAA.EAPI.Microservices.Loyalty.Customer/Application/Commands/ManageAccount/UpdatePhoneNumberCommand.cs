using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdatePhoneNumberCommand : IRequest<Response<string>>
    {
        public string CustomerID { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string FcNumber { get; set; }
        public string CorrelationId { get; set; }   

        public static explicit operator UpdatePhoneNumberCommand(UpdatePhoneNumber updatePhoneNumber)
        {
            return new()
            {
                PhoneNumber = updatePhoneNumber.PhoneNumber,
                CountryCode = updatePhoneNumber.CountryCode,
                CustomerID = updatePhoneNumber.CustomerId,
                FcNumber = updatePhoneNumber.FcNumber
            };
        }
    }
}
