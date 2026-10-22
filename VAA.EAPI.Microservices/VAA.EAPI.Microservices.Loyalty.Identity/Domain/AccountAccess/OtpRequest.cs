using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess
{
    public class OtpRequest
    {
        public string ContinuationToken { get; set; }
        public string OtpCode {  get; set; }
        public string UrlEntraJourney { get; set; }
    }
}
