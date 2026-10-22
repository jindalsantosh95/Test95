using MediatR;
using System.Collections;
using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS
{
    public class BrandMSCommand 
    {
        public string Currency { get; set; }
        public string PseudoCityCode { get; set; }
        public string AccountCode { get; set; }
        public IEnumerable<BrandQueryRequestTrip> Itinerary { get; set; }
    }
}
