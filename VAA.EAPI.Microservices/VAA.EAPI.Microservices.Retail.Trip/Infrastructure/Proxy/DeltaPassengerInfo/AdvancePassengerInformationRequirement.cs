
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class AdvancePassengerInformationRequirement
    {
        public AdvancePassengerInformationCodeType RequirementType { get; set; }
        public bool Required { get; set; }

        public static explicit operator AdvancePassengerInformationRequirement(Domain.PassengerDetails.AdvancePassengerInformationRequirement source)
        {
            if (source is null) return null;

            return new AdvancePassengerInformationRequirement()
            {
                RequirementType = (AdvancePassengerInformationCodeType)source.RequirementType,
                Required = source.Required
            };
        }
    }
}