using MediatR;
using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ReferenceMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ReferenceMS
{
    public class AirportDetailsQuery : IRequest<IEnumerable<AirportDetail>>
    {
        public string AirportCodes { get; set; }

        public AirportDetailsQuery(string airportCodes)
        {
            AirportCodes = airportCodes;
        }
    }
}
