using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services
{
    public class ServiceResponseQueryRequest:ServiceResponse
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public string CorrelationId { get; set; }
        public string EventType { get; set; }
    }
}
