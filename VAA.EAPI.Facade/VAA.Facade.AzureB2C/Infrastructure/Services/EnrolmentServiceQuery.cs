using System.Collections.Generic;
using MediatR;
using VAA.Facade.AzureB2C.Infrastructure.Proxy;
using VAA.Facade.AzureB2C.Domain.Model;

namespace VAA.Facade.AzureB2C.Infrastructure.Services
{
    public class EnrolmentServiceQuery : IRequest<KeyValuePair<bool, EnrolmentServiceResponse>>
    {
        public EnrolmentRequest EnrolmentServiceRequest { get; set; }

        public EnrolmentServiceQuery(EnrolmentRequest request)
        {
            EnrolmentServiceRequest = request;
        }
    }
}