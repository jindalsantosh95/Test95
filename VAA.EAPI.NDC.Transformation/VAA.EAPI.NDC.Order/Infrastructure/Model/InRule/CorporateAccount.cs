using VAA.EAPI.NDC.Order.Infrastructure.Model;
using SourceCorporateAccount = VAA.EAPI.NDC.Order.Infrastructure.Model.CorporateAccount;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class CorporateAccount
    {
        public string AccountName { get; set; }
        public string AgencyID { get; set; }
        public string CorporateCode { get; set; }
        public string TicketDesignator { get; set; }
        public bool ActiveContract { get; set; }

        public static explicit operator CorporateAccount(SourceCorporateAccount source) =>
            source is null
                ? null
                : new CorporateAccount
                {
                    AccountName = source.AccountName,
                    AgencyID = source.AgencyID,
                    CorporateCode = source.CorporateCode,
                    TicketDesignator = source.TicketDesignator,
                    ActiveContract = source.ActiveContract
                };
    }
}
