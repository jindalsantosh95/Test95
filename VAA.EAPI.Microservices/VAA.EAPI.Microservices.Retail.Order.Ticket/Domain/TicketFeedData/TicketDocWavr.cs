
namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocWavr
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string wavrCdField;

        private string wavrGTsField;

        private string wavrRecLocrTxtField;

        private string wavrApldCdField;

        private string wavrApldGTsField;

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



        public string WavrCd
        {
            get
            {
                return this.wavrCdField;
            }
            set
            {
                this.wavrCdField = value;
            }
        }



        public string WavrGTs
        {
            get
            {
                return this.wavrGTsField;
            }
            set
            {
                this.wavrGTsField = value;
            }
        }



        public string WavrRecLocrTxt
        {
            get
            {
                return this.wavrRecLocrTxtField;
            }
            set
            {
                this.wavrRecLocrTxtField = value;
            }
        }



        public string WavrApldCd
        {
            get
            {
                return this.wavrApldCdField;
            }
            set
            {
                this.wavrApldCdField = value;
            }
        }



        public string WavrApldGTs
        {
            get
            {
                return this.wavrApldGTsField;
            }
            set
            {
                this.wavrApldGTsField = value;
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
