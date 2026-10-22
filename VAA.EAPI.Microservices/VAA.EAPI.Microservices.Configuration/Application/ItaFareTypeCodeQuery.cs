using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class ItaFareTypeCodeQuery : IRequest<ItaFareTypeCodeData>
    {
        public string FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }
       
    }
}
