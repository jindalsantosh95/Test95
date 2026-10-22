
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class TicketDocumentSource
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tkDcRcvdSrcCdField;

        private string tkDcSrcRcvdGTsField;

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



        public string TkDcRcvdSrcCd
        {
            get
            {
                return this.tkDcRcvdSrcCdField;
            }
            set
            {
                this.tkDcRcvdSrcCdField = value;
            }
        }



        public string TkDcSrcRcvdGTs
        {
            get
            {
                return this.tkDcSrcRcvdGTsField;
            }
            set
            {
                this.tkDcSrcRcvdGTsField = value;
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
