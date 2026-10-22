
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class ElectronicAddress
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tktDocEAdrSqNbField;

        private string curElcrAdrIndField;

        private string elcrAdrField;

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



        public string TktDocEAdrSqNb
        {
            get
            {
                return this.tktDocEAdrSqNbField;
            }
            set
            {
                this.tktDocEAdrSqNbField = value;
            }
        }



        public string CurElcrAdrInd
        {
            get
            {
                return this.curElcrAdrIndField;
            }
            set
            {
                this.curElcrAdrIndField = value;
            }
        }



        public string ElcrAdr
        {
            get
            {
                return this.elcrAdrField;
            }
            set
            {
                this.elcrAdrField = value;
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
