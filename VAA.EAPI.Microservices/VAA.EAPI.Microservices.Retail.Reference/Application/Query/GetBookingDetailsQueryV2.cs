using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetBookingDetailsQueryV2 : IRequest<Response<PreValidationPartnerData>>
    {
        public FlownDetails flownDetails { get; set; }
    }    
}
