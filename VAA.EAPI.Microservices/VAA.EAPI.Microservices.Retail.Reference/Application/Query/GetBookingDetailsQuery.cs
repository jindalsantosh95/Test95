using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetBookingDetailsQuery : IRequest<Response<IEnumerable<BookingDetails>>>
    {
        public FlownDetails flownDetails { get; set; }
    }    
}
