
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocSuspendStatusRemark
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tkDcSSttRkSqNbField;

        private string suspSttRmrkTxtField;

        private string tkDcSuspSttCdField;

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



        public string TkDcSSttRkSqNb
        {
            get
            {
                return this.tkDcSSttRkSqNbField;
            }
            set
            {
                this.tkDcSSttRkSqNbField = value;
            }
        }



        public string SuspSttRmrkTxt
        {
            get
            {
                return this.suspSttRmrkTxtField;
            }
            set
            {
                this.suspSttRmrkTxtField = value;
            }
        }



        public string TkDcSuspSttCd
        {
            get
            {
                return this.tkDcSuspSttCdField;
            }
            set
            {
                this.tkDcSuspSttCdField = value;
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
