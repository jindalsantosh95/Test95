namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class MemberSearch
    {
        public readonly static string RefreshBtn = "//button[contains(@class,'ms-Button ms-Button--default notificationButton')]/span/span/span[contains(text(),'Refresh')]";

        public readonly static string SignInBtn = "//button[contains(@class,'ms-Button')]/span/span/span[contains(text(),'Sign in')]";

        public readonly static string WaitUntilPALoads = "//div[contains(@class,'progressIndicatorPaused')]";

        public readonly static string HomeButton = "//div[@data-control-name='btnHome']/div/div/div/div";

        public readonly static string CurrentUser = "//div[@data-control-name='lblWelcomeUser']/div/div/div/div/div";

        //Member Search Fields

        public readonly static string SearchPageIframeName = "fullscreen-app-host";

        public readonly static string MemPopUpCloseButton = "/html/body/div[1]/div/div/div/div[3]/div/div/div[14]/div/div/div/div";

        public readonly static string SrchMemAcctNum = "//*[@appmagic-control='srchMemberAccNotextbox']";

        public readonly static string SrchMemSrchBtn = "//*[@title='Search' and @aria-Label='Search for Members']";

        public readonly static string SrchMemClrBtn = "//*[@title='Clear All']";

        public readonly static string SrchMemFName = "//*[@appmagic-control='srchMemberFNametextbox']";

        public readonly static string SrchMemSName = "//*[@appmagic-control='srchMemberSurnametextbox']";

        public readonly static string SrchMemDOB = "//*[@appmagic-control='srchMemberDOBtextbox']";

        public readonly static string SrchMemEmail = "//*[@appmagic-control='srchMemberEmailtextbox']";

        public readonly static string SrchMemPstCde = "//*[@appmagic-control='srchMemberPostCodetextbox']";

        public readonly static string MemberCountryDropdown = "//*[@id='react-combobox-view-2']/div[2]";

        public readonly static string SelectCountry = "//div[contains(@class,'itemTemplateContainer')]/span[text()='CountryName']";

        public readonly static string TierDropdown = "//*[@id='react-combobox-view-1']/div[2]";

        public readonly static string SelectTier = "//div[contains(@class,'itemTemplateContainer')]/span[text()='TierCode']";
    }

    public class MemberSearchResultTableDetails
    {
        //Member Search Results Table Details

        public readonly static string MemberSearchResultSet = "//table/tbody/tr";

        public readonly static string SrAcctNum = "//span[@aria-label='Account Number']";

        public readonly static string SrAccName = "//span[@aria-label='Full Name']";

        public readonly static string SrAcctStatus = "//span[@aria-label='Status']";

        public readonly static string SrAcctTier = "//span[@aria-label='Tier']";

        public readonly static string SrAcctDOB = "//span[@aria-label='DOB']";

        public readonly static string SrAcctEmail = "//span[@aria-label='Email']";

    }


}

