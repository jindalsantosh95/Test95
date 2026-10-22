using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;


namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS
{
    public class ValidationMSCommand : IRequest<ValidationRS>
    {
        public AgencyValidateRequest AgencyValidateRequest { get; set; }
        public string AgentSubscriptionKey { get; set; }
    }
}
