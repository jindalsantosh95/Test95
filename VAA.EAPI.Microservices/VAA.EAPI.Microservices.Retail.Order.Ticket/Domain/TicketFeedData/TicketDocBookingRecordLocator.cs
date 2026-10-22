

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData
{
    public class TicketDocBookingRecordLocator
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tDBkgRLocrSqNbField;

        private string bkgRecLocrTxtField;

        private string bRLocrDlmCtGTsField;

        private string resSysPvdrCdField;

        private string orglBkgRecLocrTxtField;

        private string orglBRLocrDlmCtGTsField;

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



        public string TDBkgRLocrSqNb
        {
            get
            {
                return this.tDBkgRLocrSqNbField;
            }
            set
            {
                this.tDBkgRLocrSqNbField = value;
            }
        }



        public string BkgRecLocrTxt
        {
            get
            {
                return this.bkgRecLocrTxtField;
            }
            set
            {
                this.bkgRecLocrTxtField = value;
            }
        }



        public string BRLocrDlmCtGTs
        {
            get
            {
                return this.bRLocrDlmCtGTsField;
            }
            set
            {
                this.bRLocrDlmCtGTsField = value;
            }
        }



        public string ResSysPvdrCd
        {
            get
            {
                return this.resSysPvdrCdField;
            }
            set
            {
                this.resSysPvdrCdField = value;
            }
        }



        public string OrglBkgRecLocrTxt
        {
            get
            {
                return this.orglBkgRecLocrTxtField;
            }
            set
            {
                this.orglBkgRecLocrTxtField = value;
            }
        }



        public string OrglBRLocrDlmCtGTs
        {
            get
            {
                return this.orglBRLocrDlmCtGTsField;
            }
            set
            {
                this.orglBRLocrDlmCtGTsField = value;
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
