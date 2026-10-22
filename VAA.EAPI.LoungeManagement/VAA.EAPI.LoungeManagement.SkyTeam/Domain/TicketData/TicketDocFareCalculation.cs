
namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData
{
    public class TicketDocFareCalculation
    {
        private string tktDocNbField;

        private string tktDocIssLDtField;

        private string tktDocSqNbField;

        private string tDFrCaltnTypCdField;

        private string fareCaltnTxtField;

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



        public string TDFrCaltnTypCd
        {
            get
            {
                return this.tDFrCaltnTypCdField;
            }
            set
            {
                this.tDFrCaltnTypCdField = value;
            }
        }



        public string FareCaltnTxt
        {
            get
            {
                return this.fareCaltnTxtField;
            }
            set
            {
                this.fareCaltnTxtField = value;
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
