using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class PassengerSemantics
    {
        public PassengerName PassengerName { get; set; }
        public string MembershipProgramStatus { get; set; }
        public string MembershipProgramName { get; set; }
        public string BoardingGroup { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
        public string SeatClass { get; set; }
        public string SecurityProgramLogo { get; set; }
        public string BoardingPrivilegeImage { get; set; }
        public string BarCode_Value { get; set; }
        public string Remarks { get; set; }
        public string PNR { get; set; }
        public IEnumerable<string> PassengerCapabilities { get; set; }
        public IEnumerable<string> PassengerEligibleSecurityPrograms { get; set; }
        public string PriorityStatus { get; set; }
        public bool InternationalDocumentsAreVerified { get; set; }
        public string? InternationalDocumentsVerifiedDeclarationName { get; set; }
        public string? TicketFareClass { get; set; }
    }
}
