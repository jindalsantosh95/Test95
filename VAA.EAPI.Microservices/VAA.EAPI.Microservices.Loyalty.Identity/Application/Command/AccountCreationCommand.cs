
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class AccountCreationCommand : IRequest<bool>
    {
        public string Id { get; set; }
        public object Data { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string Specversion { get; set; }
        
        public static explicit operator AccountCreationCommand(CloudEvent cloudEventRequest)
        {
            return new()
            {
                Id = cloudEventRequest.Id,
                Data = cloudEventRequest.Data,
                Type = cloudEventRequest.Type,
                Subject = cloudEventRequest.Subject,
                Specversion = Constants.Application.EventSpecVersion
            };
        }
    }

   
}
