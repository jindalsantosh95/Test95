using MediatR;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO.Request
{
    public class ATPCORequest : IRequest<Models.RouteHappy.Resp>
    {
        public Control Control { get; set; }
        public string Currency { get; set; }
        public Data Data { get; set; }
        public string PCC { get; set; }
        public string Acct_code { get; set; }

        public static explicit operator ATPCORequest(BrandQueryRequest source)
        {
            return new()
            {
                Control = new Control()
                {
                    includes = new string[] { "uta", "upa", "amenity" },
                    include_rq = true
                },
                Currency = source.Currency,
                Data = new Data()
                {
                    Itineraries = source.Itinerary.Select(i => (Itinerary)i)
                },
                PCC = source.PseudoCityCode,
                Acct_code = source.AccountCode
            };
        }
    }
}