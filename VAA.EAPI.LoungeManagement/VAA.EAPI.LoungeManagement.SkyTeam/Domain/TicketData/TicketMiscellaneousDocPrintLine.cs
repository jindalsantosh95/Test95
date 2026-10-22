
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class TicketMiscellaneousDocPrintLine
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string miscDocPrtLnIdField;

        private string pTASpnrInfoTxtField;

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



        public string MiscDocPrtLnId
        {
            get
            {
                return this.miscDocPrtLnIdField;
            }
            set
            {
                this.miscDocPrtLnIdField = value;
            }
        }



        public string PTASpnrInfoTxt
        {
            get
            {
                return this.pTASpnrInfoTxtField;
            }
            set
            {
                this.pTASpnrInfoTxtField = value;
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
