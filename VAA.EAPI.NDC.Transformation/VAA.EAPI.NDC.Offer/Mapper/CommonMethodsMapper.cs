using System.Linq;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class CommonMethodsMapper
    {
        public static NDCParties MapSeatAvailabilityNDCParties(DistributionChainLinkType[] distributionChain)
        {
            var commonDistributionChain = distributionChain.Select(x => (DistributionChainLinkType)x).OrderBy(x => int.Parse(x.Ordinal));

            return (NDCParties)commonDistributionChain.ToArray();
        }
    }
}
