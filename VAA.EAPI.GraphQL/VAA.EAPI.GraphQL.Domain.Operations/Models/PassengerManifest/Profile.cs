namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class Profile
    {
        public bool isStaffBooking { get; set; }
        public bool isCrewPosition { get; set; }
        public bool isVHols { get; set; }
        public bool isCorp { get; set; }
        public bool isNonRev { get; set; }
    }
}
