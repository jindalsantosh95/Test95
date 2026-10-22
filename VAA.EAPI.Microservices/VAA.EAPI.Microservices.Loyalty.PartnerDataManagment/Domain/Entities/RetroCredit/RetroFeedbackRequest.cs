namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit
{
    public class RetroFeedbackRequest
    {
        /// <summary>
        /// Technical field for tracing purpose
        /// </summary>
        public string? TrackID { get; set; }
        /// <summary>
        /// Length -21 char
        /// First 2 char contain the IATA carrier code of the Operating carrier of FFP from where the request originates. The remaining 19 characters need to be a uniquely identifiable number per record
        /// </summary>
        public string? UniqueIdentificationNumber { get; set; }
        /// <summary>
        /// IATA carrier code of the carrier code whose FF program the customer belongs to
        /// /// Mandatory
        /// </summary>
        public string? FFPCarrierCode { get; set; }
        /// <summary>
        /// IATA carrier code of the operating carrier
        /// Mandatory
        /// </summary>
        public string? OperatingCarrierCode { get; set; }
        /// <summary>
        /// Date the miles for this flight item was posted to the member’s account – format YYYYMMDD
        /// </summary>
        public string? PostingDate { get; set; }
        /// <summary>
        /// Indication if miles were posted or not
        /// Y: Indicates miles have been retro-credited
        /// N: Indicates miles were not credited
        /// </summary>
        public string? PostingStatus { get; set; }
        /// <summary>
        /// Miles posted to the member’s account for this flight segment
        /// </summary>
        public string? BaseMiles { get; set; }
        /// <summary>
        /// Class of service Bonus miles awareded
        /// </summary>
        public string? ClassOfServiceBonusMiles { get; set; }
        /// <summary>
        /// Miles that count toward elite status on this flight segment.
        /// </summary>
        public string? EliteTierBonusMiles { get; set; }
        /// <summary>
        /// Promotional miles awarded on this flight segment
        /// </summary>
        public string? PromotionalBonusMiles { get; set; }

    }
}
