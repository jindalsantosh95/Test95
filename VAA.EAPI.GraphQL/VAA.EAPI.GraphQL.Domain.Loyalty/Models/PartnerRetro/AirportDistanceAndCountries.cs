using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class AirportDistanceAndCountries
    {
        public string DistanceInMiles { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
    }
}
