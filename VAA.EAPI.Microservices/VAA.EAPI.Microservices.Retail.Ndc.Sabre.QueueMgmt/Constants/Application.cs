namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Constants
{
    public static class Application
    {
        public static readonly string QueueAccessAction = "QueueAccessLLSRQ";
        public static readonly string QueueNumber = "20";
        public static readonly string GetReservationAction = "GetReservationRQ";
        public const string GetReservationRequestType = "Stateful";
        public const string GetReservationRQVersion = "1.19.22";
        public static readonly string[] GetReservationSubjectAreas = ["HEADER"];
        public const string GetReservationViewName = "Simple";
        public const int MaxSabreErrorMessages = 3;
        public const int MaxSabreErrorMessageLength = 256;

    }
}
