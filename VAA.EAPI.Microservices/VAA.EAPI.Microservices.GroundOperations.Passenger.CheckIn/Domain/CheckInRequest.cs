using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class CheckInRequest
    {
        public string PnrLocator { get; set; }
        public string ETicketNumber { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string OriginCode { get; set; }

        public static explicit operator DeltaSsci.InputObject(CheckInRequest source)
        {
            return new DeltaSsci.InputObject()
            {
                RecordLocator = source.PnrLocator,
                TicketNumber = source.ETicketNumber,
                OriginCode = source.OriginCode
            };
        }

        public static explicit operator DeltaTrm.RequirementsRequest(CheckInRequest source)
        {
            if (source.PnrLocator.IsNullOrEmpty() == false)
                return new DeltaTrm.RequirementsRequest()
                {
                    Using = DeltaTrm.RequestOptionCodeType.CONFIRMATION,
                    GivenNames = source.GivenNames,
                    Surname = source.Surname,
                    ConfirmationNum = source.PnrLocator
                };

            if (source.ETicketNumber.IsNullOrEmpty() == false)
                return new DeltaTrm.RequirementsRequest()
                {
                    Using = DeltaTrm.RequestOptionCodeType.TICKET,
                    GivenNames = source.GivenNames,
                    Surname = source.Surname,
                    ConfirmationNum = source.ETicketNumber
                };

            throw new ArgumentException("PnrLocator or ETicketNumber must be provided.");
        }
    }
}