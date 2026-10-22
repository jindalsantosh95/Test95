
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocTranasactionAgencyReason
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tDTnAgyRsnSqNbField;

        private string tDTnAgyRsnCdField;

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



        public string TDTnAgyRsnSqNb
        {
            get
            {
                return this.tDTnAgyRsnSqNbField;
            }
            set
            {
                this.tDTnAgyRsnSqNbField = value;
            }
        }



        public string TDTnAgyRsnCd
        {
            get
            {
                return this.tDTnAgyRsnCdField;
            }
            set
            {
                this.tDTnAgyRsnCdField = value;
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
