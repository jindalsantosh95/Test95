using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.UI.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("CampaignManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class CampaignManagementTests : IDisposable, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private CampaignManagementPage campaignManagementPage;

        public CampaignManagementTests(ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            memberSearchPage = new MemberSearchPage(webDriver);

            campaignManagementPage = new CampaignManagementPage(webDriver);
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_CampaignSearchByCampaignCode_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_CampaignSearchByCampaignCode_SuccessScenario");

            #region - PreRequisite search for a valid campaign in Azure Reference DB for a valid campaign code

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type,RegistrationRequired from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignName = campaignDetails[0][1]?.ToString();

            string campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignStatus = campaignDetails[0][4]?.ToString();

            string campaignType = campaignDetails[0][5]?.ToString();

            string registrationRequiredFlag = campaignDetails[0][6]?.ToString();

            #endregion

            #region - Search by campaign Code

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            string TotalResultsRetrieved = campaignManagementPage.ValidateTotalResultsRetrieved();

            Assert.True(TotalResultsRetrieved.Contains("1"),
                $"Expected CamapignCount retrived is 1 but Found {TotalResultsRetrieved}");

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the CampaignCode {campaignCode}");

            string? campCode = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? campName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? campStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? campEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? campStatus = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? campType = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            Assert.True(campaignCode.Equals(campCode),
                $"Expected CampaignCode to be {campaignCode} but found {campCode}");

            Assert.True(campaignName.Equals(campName),
                $"Expected CampaignName to be {campaignName} but found {campName}");

            Assert.True(campaignStartDate.Equals(campStartDate),
                $"Expected CampaignStartDate to be {campaignStartDate} but found {campStartDate}");

            Assert.True(campaignEndDate.Equals(campEndDate),
                $"Expected CampaignEndDate to be {campaignEndDate} but found {campEndDate}");

            Assert.True(campaignStatus.Equals(campStatus),
                $"Expected CampaignStatus to be {campaignStatus} but found {campStatus}");

            Assert.True(campaignType.Equals(campType),
                $"Expected Campaign Type to be {campaignType} but found {campType}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            string cpCode = webDriver.GetAttributeTextValue(CampaignManagement.UpdateCampaignCodeTextBox, "value");

            string cpName = webDriver.GetAttributeTextValue(CampaignManagement.UpdateCampaignNameTextBox, "value");

            string cpType = webDriver.GetElementText(CampaignManagement.UpdateCampaignType);

            string cpStatus = webDriver.GetElementText(CampaignManagement.UpdateCampaignStatus);

            string cpStartDate = webDriver.GetAttributeTextValue(CampaignManagement.UpdateCampaignStartDateTextBox, "value");

            string cpEndDate = webDriver.GetAttributeTextValue(CampaignManagement.UpdateCampaignEndDateTextBox, "value");

            string regReqFlag = webDriver.FindElement(By.XPath(CampaignManagement.UpdateCampaignRegReqFlagCheckBox)).GetAttribute("checked");

            regReqFlag = regReqFlag.Equals("true") ? "Y" : "N";

            Assert.True(cpCode.Equals(campaignCode),
                $"The Campaign Code does not match on Campaign Details Page. Expected {campaignCode} but found {cpCode}.");

            Assert.True(cpName.Equals(campaignName),
                $"The Campaign Name does not match on Campaign Details Page. Expected {campaignName} but found {cpName}.");

            Assert.True(cpStartDate.Equals(campaignStartDate),
                $"The StartDate does not match on Campaign Details Page. Expected {campaignStartDate} but found {cpStartDate}.");

            Assert.True(cpEndDate.Equals(campaignEndDate),
                $"The EndDate does not match on Campaign Details Page. Expected {campaignEndDate} but found {cpEndDate}.");

            Assert.True(cpType.Equals(campaignType),
                $"The Campaign Type does not match on Campaign Details Page. Expected {campaignType} but found {cpType}.");

            Assert.True(cpStatus.Equals(campaignStatus),
                $"The Campaign Status does not match on Campaign Details Page. Expected {campaignStatus} but found {cpStatus}.");

            Assert.True(regReqFlag.Equals(registrationRequiredFlag),
                "The Registration Required Flag does not match on Campaign Details Page." +
                $" Expected {registrationRequiredFlag} but found {regReqFlag}.");


            #endregion

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_CampaignSearchByCampaignName_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC02_CampaignSearchByCampaignName_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignName = campaignDetails[0][1]?.ToString();

            string campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignStatus = campaignDetails[0][4]?.ToString();

            string campaignType = campaignDetails[0][5]?.ToString();

            #endregion

            #region - Search by campaign Name

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignName(campaignName);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Name {campaignName}");

            foreach (IWebElement element in result)
            {
                string? campName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                Assert.True(campaignName.Equals(campName),
                    $"Expected Campaign Name to be {campaignName} but found {campName}");
            }

            #endregion

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_CampaignSearchByCampaignType_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_CampaignSearchByCampaignType_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 Type from [dbo].[Campaigns] order by NEWID()";

            string campaignType = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region - Search by campaign Name

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterAndSelectTheCampaignTypeFromTheCampaignTypeDropdown(campaignType);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Type {campaignType}");

            foreach (IWebElement element in result)
            {

                string? campType = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                Assert.True(campaignType.Equals(campType),
                    $"Expected Campaign Type to be {campaignType} but found {campType}");
            }

            #endregion

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_SearchCampaignWithActiveStatusAndPromotionLinked()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC04_SearchCampaignWithActiveStatusAndPromotionLinked");

            #region - PreRequisite search for a valid campaign in Azure Reference DB with Type 'Accrual' and Promotion Linked

            string queryForCampaignSearch = "SELECT Top 1 Campaigns.RowID, Campaigns.CampaignCode , CampaignsPromotionsMapping.PromotionId from Campaigns  " +
                "inner join CampaignsPromotionsMapping on Campaigns.RowID = CampaignsPromotionsMapping.CampaignId " +
                "where Campaigns.Type='Accrual' and Campaigns.Status ='Active' order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignId = campaignDetails[0][0]?.ToString();

            string campaignCode = campaignDetails[0][1]?.ToString();

            string promotionId = campaignDetails[0][2]?.ToString();

            string queryForPromotionSearch = $"Select Name from [dbo].[Promotions] where RowID='{promotionId}'";

            string promotionName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForPromotionSearch);

            #endregion

            #region - Search by campaign Code

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.ClickOnRefreshButtonIfEnabled();

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            string TotalResultsRetrieved = campaignManagementPage.ValidateTotalResultsRetrieved();

            Assert.True(TotalResultsRetrieved.Contains("1"),
                $"Expected CamapignCount retrived is 1 but Found {TotalResultsRetrieved}");

            string ActualCampaignCode = campaignManagementPage.ClickAndValidateCampaignDetails(campaignCode);

            Assert.True(ActualCampaignCode.Equals(campaignCode),
                $"Campaigncode retrieved is {ActualCampaignCode} and expected campaigncode is {campaignCode}");

            string ActualPromotionName = campaignManagementPage.ValidatePromotionName(promotionName);

            Assert.True(ActualPromotionName.Equals(promotionName),
                $"PromotionName retrieved is {ActualPromotionName} and expected campaigncode is {promotionName}");

            #endregion

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_CampaignSearchUsingValidStartDateFromAndTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC05_CampaignSearchUsingValidStartDateFromAndTo_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string startDateTo = DateTime.Now.ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterStartDateFrom(startDateFrom);

            campaignManagementPage.EnterStartDateTo(startDateTo);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaigns Found Whose StartDate is between {startDateFrom} and {startDateTo}");

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom) && startDate <= Convert.ToDateTime(startDateTo),
                    $"Expected the startDate range between {startDateFrom} and {startDateTo} but found {startDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_CampaignSearchUsingValidEndDateFromAndTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC06_CampaignSearchUsingValidEndDateFromAndTo_Success");

            string endDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterEndDateFrom(endDateFrom);

            campaignManagementPage.EnterEndDateTo(endDateTo);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaigns Found Whose EndDate is between {endDateFrom} and {endDateTo}");

            foreach (IWebElement element in result)
            {
                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                Assert.True(endDate >= Convert.ToDateTime(endDateFrom) && endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the EndDate range between {endDateFrom} and {endDateTo} but found {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(7)]
        public void TC07_CampaignSearchUsingValidStartDateFromAndEndDateTo_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC07_CampaignSearchUsingValidStartDateFromAndEndDateTo_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterStartDateFrom(startDateFrom);

            campaignManagementPage.EnterEndDateTo(endDateTo);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaigns Found Whose StartDate is >= {startDateFrom} and EndDate is <= {endDateTo} ");

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value"));

                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom) && endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the Date range of startDate to be >= {startDateFrom} and EndDate Range to be below <= {endDateTo} but found {startDate.ToString("dd-MMM-yyyy")} and {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(8)]
        public void TC08_CampaignSearchUsingValidStartDateFrom_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_CampaignSearchUsingValidStartDateFrom_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterStartDateFrom(startDateFrom);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaigns Found Whose StartDate is >= {startDateFrom} .");

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom),
                    $"Expected the Date range of startDate to be >= {startDateFrom}  but found {startDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(9)]
        public void TC09_CampaignSearchUsingValidEndDateTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC09_CampaignSearchUsingValidEndDateTo_Success");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterEndDateTo(endDateTo);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaigns Found Whose EndDate is <= {endDateTo} .");

            foreach (IWebElement element in result)
            {

                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                Assert.True(endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the Date range  EndDate Range to be below <= {endDateTo} but found {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(10)]
        public void TC10_CampaignSearchUsingCampaignCodeWildCardCharacter()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC10_CampaignSearchUsingCampaignCodeWildCardCharacter");

            #region  Test Data from Reference DB      

            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] Where len(CampaignCode)>'2' ORDER BY NEWID()";

            string campaignCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            string[] wildCards = { "FirstWordWildCard", "LastWordWildCard", "MiddleWordWildCard" };

            Random random = new();

            string wildCard = wildCards[random.Next(wildCards.Length)];

            int rvalue = random.Next(campaignCode.Length);

            switch (wildCard)
            {
                case "FirstWordWildCard":
                    campaignCode = string.Concat("*", campaignCode.Substring(rvalue, campaignCode.Length - rvalue));
                    break;

                case "LastWordWildCard":
                    campaignCode = string.Concat(campaignCode.Substring(0, campaignCode.Length - rvalue), "*");
                    break;

                case "MiddleWordWildCard":
                    campaignCode = string.Concat("*", campaignCode.Substring(rvalue, campaignCode.Length - rvalue), "*");
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string campCode = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                if (wildCard.Equals("LastWordWildCard"))
                    Assert.StartsWith(campaignCode.Replace("*", ""), campCode, StringComparison.InvariantCultureIgnoreCase);

                else if (wildCard.Equals("FirstWordWildCard"))
                    Assert.EndsWith(campaignCode.Replace("*", ""), campCode, StringComparison.InvariantCultureIgnoreCase);

                else
                    Assert.True(campCode.Contains(campaignCode.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase),
                        $"Expected the Searched Campaign Code with Wild Card Character {campaignCode} in the result CampaignCode:{campCode}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(11)]
        public void TC11_CampaignSearchUsingCampaignNameWildCardCharacter()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC11_CampaignSearchUsingCampaignNameWildCardCharacter");

            #region  Test Data from Reference DB      

            string queryForCampaignSearch = "SELECT TOP 1 Name from [dbo].[Campaigns] Where len(Name)>'4' ORDER BY NEWID()";

            string campaignName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            string[] wildCards = { "FirstWordWildCard", "LastWordWildCard", "MiddleWordWildCard" };

            Random random = new();

            string wildCard = wildCards[random.Next(wildCards.Length)];

            int rvalue = random.Next(campaignName.Length);

            switch (wildCard)
            {
                case "FirstWordWildCard":
                    campaignName = string.Concat("*", campaignName.Substring(rvalue, campaignName.Length - rvalue));
                    break;

                case "LastWordWildCard":
                    campaignName = string.Concat(campaignName.Substring(0, campaignName.Length - rvalue), "*");
                    break;

                case "MiddleWordWildCard":
                    campaignName = string.Concat("*", campaignName.Substring(rvalue, campaignName.Length - rvalue), "*");
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignName(campaignName);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string campName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                if (wildCard.Equals("LastWordWildCard"))
                    Assert.StartsWith(campaignName.Replace("*", ""), campName, StringComparison.InvariantCultureIgnoreCase);

                else if (wildCard.Equals("FirstWordWildCard"))
                    Assert.EndsWith(campaignName.Replace("*", ""), campName, StringComparison.InvariantCultureIgnoreCase);

                else
                    Assert.True(campName.Contains(campaignName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase),
                        $"Expected the Searched Campaign Name with Wild Card Character {campaignName} in the result CampaignName:{campName}");

            }
        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(12)]
        public void TC12_CampaignCreate_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC12_CampaignCreate_Success");

            string campCode = DataGenerator.RandomString(6).ToUpper();

            string campName = "TestCampaign";

            string endDate = DateTime.UtcNow.AddYears(20).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.ClickOnCampaignCreateButton();

            campaignManagementPage.CampaignCreate_EnterCampaignCode(campCode);

            campaignManagementPage.CampaignCreate_EnterCampaignName(campName);

            campaignManagementPage.CampaignCreate_EnterCampaignType("Flying Club Communication(CMD)");

            campaignManagementPage.CampaignCreate_EnterCampaignStatus("Active");

            campaignManagementPage.CampaignCreate_EnterEndDate(endDate);

            campaignManagementPage.CampaignCreate_EnterDescription("Test Campaign");

            campaignManagementPage.CampaignCreate_SelectRegistrationRequiredFlag();

            campaignManagementPage.CreateCampaignByClickingOnSaveButton();

            string expectedResponse = "Campaign created successfully";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(13)]
        public void TC13_CampaignUpdate_UpdateCampaignCode_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC13_CampaignUpdate_UpdateCampaignCode_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignName = campaignDetails[0][1]?.ToString();

            string campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignStatus = campaignDetails[0][4]?.ToString();

            string campaignType = campaignDetails[0][5]?.ToString();

            #endregion

            string newCampCode = DataGenerator.RandomString(6).ToUpper();

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Code {campaignCode}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            campaignManagementPage.CampaignUpdate_EnterNewCampaignCode(newCampCode);

            campaignManagementPage.CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign();

            string expectedResponse = "Campaign details updated successfully";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

            campaignManagementPage.EnterCampaignCode(newCampCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            string? campCode = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? campName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? campStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? campEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? campStatus = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? campType = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            Assert.True(campCode.Equals(newCampCode),
               $"The Campaign Code does not match on Campaign Details Page. Expected {newCampCode} but found {campCode}.");

            Assert.True(campName.Equals(campaignName),
                $"The Campaign Name does not match on Campaign Details Page. Expected {campaignName} but found {campName}.");

            Assert.True(campStartDate.Equals(campaignStartDate),
                $"The StartDate does not match on Campaign Details Page. Expected {campaignStartDate} but found {campStartDate}.");

            Assert.True(campEndDate.Equals(campaignEndDate),
                $"The EndDate does not match on Campaign Details Page. Expected {campaignEndDate} but found {campEndDate}.");

            Assert.True(campType.Equals(campaignType),
                $"The Campaign Type does not match on Campaign Details Page. Expected {campaignType} but found {campType}.");

            Assert.True(campStatus.Equals(campaignStatus),
                $"The Campaign Status does not match on Campaign Details Page. Expected {campaignStatus} but found {campStatus}.");

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(14)]
        public void TC14_CampaignUpdate_UpdateCampaignName_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC14_CampaignUpdate_UpdateCampaignName_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignName = campaignDetails[0][1]?.ToString();

            string campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignStatus = campaignDetails[0][4]?.ToString();

            string campaignType = campaignDetails[0][5]?.ToString();

            #endregion

            string newCampaignName = "TEST" + DataGenerator.RandomString(6).ToUpper();

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Code {campaignCode}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            campaignManagementPage.CampaignUpdate_EnterNewCampaignName(newCampaignName);

            campaignManagementPage.CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign();

            string expectedResponse = "Campaign details updated successfully";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            string? campCode = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? campName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? campStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? campEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? campStatus = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? campType = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            Assert.True(campCode.Equals(campaignCode),
               $"The Campaign Code does not match on Campaign Details Page. Expected {campaignCode} but found {campCode}.");

            Assert.True(campName.Equals(newCampaignName),
                $"The Campaign Name does not match on Campaign Details Page. Expected {newCampaignName} but found {campName}.");

            Assert.True(campStartDate.Equals(campaignStartDate),
                $"The StartDate does not match on Campaign Details Page. Expected {campaignStartDate} but found {campStartDate}.");

            Assert.True(campEndDate.Equals(campaignEndDate),
                $"The EndDate does not match on Campaign Details Page. Expected {campaignEndDate} but found {campEndDate}.");

            Assert.True(campType.Equals(campaignType),
                $"The Campaign Type does not match on Campaign Details Page. Expected {campaignType} but found {campType}.");

            Assert.True(campStatus.Equals(campaignStatus),
                $"The Campaign Status does not match on Campaign Details Page. Expected {campaignStatus} but found {campStatus}.");

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(15)]
        public void TC15_CampaignUpdate_UpdateCampaignStartDate_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC15_CampaignUpdate_UpdateCampaignStartDate_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string campaignCode = campaignDetails[0][0]?.ToString();

            string campaignName = campaignDetails[0][1]?.ToString();

            string campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string campaignStatus = campaignDetails[0][4]?.ToString();

            string campaignType = campaignDetails[0][5]?.ToString();

            #endregion

            string newStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).AddDays(-10).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Code {campaignCode}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            campaignManagementPage.CampaignUpdate_EnterNewStartDate(newStartDate);

            campaignManagementPage.CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign();

            string expectedResponse = "Campaign details updated successfully";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            string? campCode = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? campName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? campStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? campEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? campStatus = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? campType = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            Assert.True(campCode.Equals(campaignCode),
               $"The Campaign Code does not match on Campaign Details Page. Expected {campaignCode} but found {campCode}.");

            Assert.True(campName.Equals(campaignName),
                $"The Campaign Name does not match on Campaign Details Page. Expected {campaignName} but found {campName}.");

            Assert.True(campStartDate.Equals(newStartDate),
                $"The StartDate does not match on Campaign Details Page. Expected {newStartDate} but found {campStartDate}.");

            Assert.True(campEndDate.Equals(campaignEndDate),
                $"The EndDate does not match on Campaign Details Page. Expected {campaignEndDate} but found {campEndDate}.");

            Assert.True(campType.Equals(campaignType),
                $"The Campaign Type does not match on Campaign Details Page. Expected {campaignType} but found {campType}.");

            Assert.True(campStatus.Equals(campaignStatus),
                $"The Campaign Status does not match on Campaign Details Page. Expected {campaignStatus} but found {campStatus}.");

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(16)]
        public void TC16_CampaignUpdate_UpdateCampaignEndDate_SuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC16_CampaignUpdate_UpdateCampaignEndDate_SuccessScenario");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode,Name,StartDate,EndDate,Status,Type from [dbo].[Campaigns] order by NEWID()";

            object[][] campaignDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string? campaignCode = campaignDetails[0][0]?.ToString();

            string? campaignName = campaignDetails[0][1]?.ToString();

            string? campaignStartDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).ToString("dd-MMM-yyyy");

            string? campaignEndDate = Convert.ToDateTime(campaignDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string? campaignStatus = campaignDetails[0][4]?.ToString();

            string? campaignType = campaignDetails[0][5]?.ToString();

            #endregion

            string newEndDate = Convert.ToDateTime(campaignDetails[0][2]?.ToString()).AddDays(10).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Campaign Found with the Campaign Code {campaignCode}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            campaignManagementPage.CampaignUpdate_EnterNewEndDate(newEndDate);

            campaignManagementPage.CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign();

            string expectedResponse = "Campaign details updated successfully";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            string? campCode = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? campName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? campStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? campEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? campStatus = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? campType = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            Assert.True(campCode.Equals(campaignCode),
               $"The Campaign Code does not match on Campaign Details Page. Expected {campaignCode} but found {campCode}.");

            Assert.True(campName.Equals(campaignName),
                $"The Campaign Name does not match on Campaign Details Page. Expected {campaignName} but found {campName}.");

            Assert.True(campStartDate.Equals(campaignStartDate),
                $"The StartDate does not match on Campaign Details Page. Expected {campaignStartDate} but found {campStartDate}.");

            Assert.True(campEndDate.Equals(newEndDate),
                $"The EndDate does not match on Campaign Details Page. Expected {newEndDate} but found {campEndDate}.");

            Assert.True(campType.Equals(campaignType),
                $"The Campaign Type does not match on Campaign Details Page. Expected {campaignType} but found {campType}.");

            Assert.True(campStatus.Equals(campaignStatus),
                $"The Campaign Status does not match on Campaign Details Page. Expected {campaignStatus} but found {campStatus}.");

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(17)]
        public void TC17_CampaignCreate_DuplicateCampaignCodeErrorIftheNewCampaignCodeAlreadyExists()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC17_CampaignCreate_DuplicateCampaignCodeErrorIftheNewCampaignCodeAlreadyExists");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            string? campaignCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion            

            string campName = "TestCampaign";

            string endDate = DateTime.UtcNow.AddYears(20).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.ClickOnCampaignCreateButton();

            campaignManagementPage.CampaignCreate_EnterCampaignCode(campaignCode);

            campaignManagementPage.CampaignCreate_EnterCampaignName(campName);

            campaignManagementPage.CampaignCreate_EnterCampaignType("Flying Club Communication(CMD)");

            campaignManagementPage.CampaignCreate_EnterCampaignStatus("Active");

            campaignManagementPage.CampaignCreate_EnterEndDate(endDate);

            campaignManagementPage.CampaignCreate_EnterDescription("Test Campaign");

            campaignManagementPage.CampaignCreate_SelectRegistrationRequiredFlag();

            campaignManagementPage.CreateCampaignByClickingOnSaveButton();

            string expectedResponse = "This Campaign Code is already used. Please use a different code.";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                $"Campaign Create Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();

        }

        [Trait("Category", Traits.CampaignManagementUIRegression)]
        [Fact, RunOrder(18)]
        public void TC18_CampaignUpdate_DuplicateCampaignCodeErrorIftheNewCampaignCodeAlreadyExists()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC18_CampaignUpdate_DuplicateCampaignCodeErrorIftheNewCampaignCodeAlreadyExists");

            #region - PreRequisite search for a Valid campaign in Azure Reference DB

            string queryForCampaignSearch = "Select TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            string campaignCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            #endregion

            #region - PreRequisite search for a Existing CampaignCode

            string queryForDupCampaignCodeSearch = $"Select TOP 1 CampaignCode from [dbo].[Campaigns] where CampaignCode != '{campaignCode}' order by NEWID()";

            string newCampCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForDupCampaignCodeSearch);

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            campaignManagementPage.ClickOnCampaignManagementLink(logger);

            campaignManagementPage.EnterCampaignCode(campaignCode);

            campaignManagementPage.ClickOnSearchButton();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(CampaignManagement.SearchResultDataSet));

            Assert.True(result.Any(),
                   $"No Campaign Found with the Campaign Code {campaignCode}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();

            campaignManagementPage.CampaignUpdate_EnterNewCampaignCode(newCampCode);

            campaignManagementPage.CampaignUpdate_ClickOnSaveButtonToUpdateTheCampaign();

            string expectedResponse = "This Campaign Code is already used. Please use a different code.";

            string actualResponse = campaignManagementPage.CreateCampaignPopUpContent();

            Assert.True(expectedResponse.Equals(actualResponse),
                    $"The Response Message Does Not Match. Expected to be {expectedResponse} but was {actualResponse}.");

            campaignManagementPage.ClosePopUpOnCreateCampaignPage();
        }

        public void Dispose()
        {
            // Flush the report
            reportFixture.Current.Flush();
            // Close browser
            webDriver.CloseBrowser();
        }
    }
}
