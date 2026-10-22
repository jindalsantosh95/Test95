
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocCommission
    {

        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tktDocCmsnSqNbField;

        private string tktDocCmsnAmtField;

        private string tkDcCmsnCurrCdField;

        private string tktDocCmsnRtField;

        private string tkDcCmsnTypCdField;

        private string additionalInformationField;



        public string TktDocNb
        {
            get
            {
                return this.tktDocNbField;
            }
            set
            {
                this.tktDocNbField = value;
            }
        }



        public string TktDocIssLDt
        {
            get
            {
                return this.tktDocIssLDtField;
            }
            set
            {
                this.tktDocIssLDtField = value;
            }
        }



        public string TktDocSqNb
        {
            get
            {
                return this.tktDocSqNbField;
            }
            set
            {
                this.tktDocSqNbField = value;
            }
        }



        public string TktDocCmsnSqNb
        {
            get
            {
                return this.tktDocCmsnSqNbField;
            }
            set
            {
                this.tktDocCmsnSqNbField = value;
            }
        }



        public string TktDocCmsnAmt
        {
            get
            {
                return this.tktDocCmsnAmtField;
            }
            set
            {
                this.tktDocCmsnAmtField = value;
            }
        }



        public string TkDcCmsnCurrCd
        {
            get
            {
                return this.tkDcCmsnCurrCdField;
            }
            set
            {
                this.tkDcCmsnCurrCdField = value;
            }
        }



        public string TktDocCmsnRt
        {
            get
            {
                return this.tktDocCmsnRtField;
            }
            set
            {
                this.tktDocCmsnRtField = value;
            }
        }



        public string TkDcCmsnTypCd
        {
            get
            {
                return this.tkDcCmsnTypCdField;
            }
            set
            {
                this.tkDcCmsnTypCdField = value;
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
