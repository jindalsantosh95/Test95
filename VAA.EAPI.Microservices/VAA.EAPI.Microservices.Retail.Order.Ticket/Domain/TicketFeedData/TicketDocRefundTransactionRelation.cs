
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocRefundTransactionRelation
    {
        private string rfddTktDocNbField;

        private string rfddTkDcIssLDtField;

        private string rfddTktDocSqNbField;

        private string asRfTktDocNbField;

        private string asRfTkDcIssLDtField;

        private string asRfTktDocSqNbField;

        private string tkDcRTrnsSqNbField;

        private string additionalInformationField;



        public string RfddTktDocNb
        {
            get
            {
                return this.rfddTktDocNbField;
            }
            set
            {
                this.rfddTktDocNbField = value;
            }
        }



        public string RfddTkDcIssLDt
        {
            get
            {
                return this.rfddTkDcIssLDtField;
            }
            set
            {
                this.rfddTkDcIssLDtField = value;
            }
        }



        public string RfddTktDocSqNb
        {
            get
            {
                return this.rfddTktDocSqNbField;
            }
            set
            {
                this.rfddTktDocSqNbField = value;
            }
        }



        public string AsRfTktDocNb
        {
            get
            {
                return this.asRfTktDocNbField;
            }
            set
            {
                this.asRfTktDocNbField = value;
            }
        }



        public string AsRfTkDcIssLDt
        {
            get
            {
                return this.asRfTkDcIssLDtField;
            }
            set
            {
                this.asRfTkDcIssLDtField = value;
            }
        }



        public string AsRfTktDocSqNb
        {
            get
            {
                return this.asRfTktDocSqNbField;
            }
            set
            {
                this.asRfTktDocSqNbField = value;
            }
        }



        public string TkDcRTrnsSqNb
        {
            get
            {
                return this.tkDcRTrnsSqNbField;
            }
            set
            {
                this.tkDcRTrnsSqNbField = value;
            }
        }



        public string AdditionalInformation
        {
            get
            {
                return this.additionalInformationField;
            }
            set
            {
                this.additionalInformationField = value;
            }
        }
    }
}
