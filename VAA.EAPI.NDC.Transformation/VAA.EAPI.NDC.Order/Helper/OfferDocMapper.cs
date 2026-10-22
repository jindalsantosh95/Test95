using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Helper
{
    public static class OfferDocMapper
    {
        public static OfferResponse UpdateOfferDocWithAgentType(OfferResponse offerDoc, ValidationRS validationMSRS)
        {
            if (offerDoc?.Parties?.Participants == null || validationMSRS?.ValidateResponse?.AgencyDetails == null)

                return offerDoc;

            var agencyType = validationMSRS?.ValidateResponse?.AgencyDetails?.RequestAgencyIdType;

            foreach (var participant in offerDoc.Parties.Participants)
            {
                participant.RequestAgentType = agencyType;
            }

            return offerDoc;
        }
    }
}
