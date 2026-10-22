using DeltaPaxInfo = VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.PassengerDetails
{
    public class AdvancePassengerInformationRequirement
    {
        public AdvancePassengerInformationCodeType RequirementType { get; set; }
        public bool Required { get; set; }

        public static explicit operator AdvancePassengerInformationRequirement(DeltaPaxInfo.AdvancePassengerInformationRequirement source)
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