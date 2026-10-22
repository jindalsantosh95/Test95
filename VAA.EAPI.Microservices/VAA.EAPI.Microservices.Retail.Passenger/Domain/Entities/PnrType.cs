namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class PnrType
    {
        public DebugInfoType DebugInfo { get; set; }

        public AdvisoryMessageType[] AdvisoryMessage { get; set; }

        public Ptc[] Ptc { get; set; }

        public SeatOa[] SeatOa { get; set; }

        public RecordLocator[] RecordLocator { get; set; }

        public HistoryCreditOrSignature[] HistoryCreditOrSignature { get; set; }

        public TicketNumberData3[] TicketNumberData3 { get; set; }

        public ApprovalCode[] ApprovalCode { get; set; }

        public OtherServiceInformation[] OtherServiceInformation { get; set; }

        public Dti[] Dti { get; set; }

        public SegmentDl[] SegmentDl { get; set; }

        public TicketNumberData1[] TicketNumberData1 { get; set; }

        public HistoryPnrCreation[] HistoryPnrCreation { get; set; }

        public TicketPassengerInfo[] TicketPassengerInfo { get; set; }

        public TicketingTimeLimit[] TicketingTimeLimit { get; set; }

        public Phone[] Phone { get; set; }

        public PassengerSpecial[] PassengerSpecial { get; set; }

        public TicketInformation[] TicketInformation { get; set; }

        public SegmentOa[] SegmentOa { get; set; }

        public TicketingField[] TicketingField { get; set; }

        public TicketCouponInfo[] TicketCouponInfo { get; set; }

        public TicketingInstruction[] TicketingInstruction { get; set; }

        public DtiSegmentInfo[] DtiSegmentInfo { get; set; }

        public FareCalc[] FareCalc { get; set; }

        public SeatDl[] SeatDl { get; set; }

        public Remark[] Remark { get; set; }

        public SpecialServiceRequest[] SpecialServiceRequest { get; set; }

        public StoredFareData[] StoredFareData { get; set; }

        public Passenger[] Passenger { get; set; }

        public FormOfPayment[] FormOfPayment { get; set; }

        public SegmentAdditionalInfo[] SegmentAdditionalInfo { get; set; }

        public TicketData[] TicketData { get; set; }

        public TicketNumberData2[] TicketNumberData2 { get; set; }

        public DtiTerminatingCity[] DtiTerminatingCity { get; set; }

        public FrequentFlyer[] FrequentFlyer { get; set; }

        public HistoryActionItem[] HistoryActionItem { get; set; }

        public ElectronicTicketingData[] ElectronicTicketingData { get; set; }

        public StandbyListItin[] StandbyListItin { get; set; }
    }
}
