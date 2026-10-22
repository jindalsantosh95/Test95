
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocTaxFacsimile
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tktDocTxSqNbField;

        private string tktDocTxTxtField;

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



        public string TktDocTxSqNb
        {
            get
            {
                return this.tktDocTxSqNbField;
            }
            set
            {
                this.tktDocTxSqNbField = value;
            }
        }



        public string TktDocTxTxt
        {
            get
            {
                return this.tktDocTxTxtField;
            }
            set
            {
                this.tktDocTxTxtField = value;
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
