

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class TicketDocPaymentRemark
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tDPRmrkSqNbField;

        private string tDPRmrkSqlNbField;

        private string tDPRmrkTxtField;

        private string tDPRmrkTypCdField;

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



        public string TDPRmrkSqNb
        {
            get
            {
                return this.tDPRmrkSqNbField;
            }
            set
            {
                this.tDPRmrkSqNbField = value;
            }
        }



        public string TDPRmrkSqlNb
        {
            get
            {
                return this.tDPRmrkSqlNbField;
            }
            set
            {
                this.tDPRmrkSqlNbField = value;
            }
        }



        public string TDPRmrkTxt
        {
            get
            {
                return this.tDPRmrkTxtField;
            }
            set
            {
                this.tDPRmrkTxtField = value;
            }
        }



        public string TDPRmrkTypCd
        {
            get
            {
                return this.tDPRmrkTypCdField;
            }
            set
            {
                this.tDPRmrkTypCdField = value;
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
