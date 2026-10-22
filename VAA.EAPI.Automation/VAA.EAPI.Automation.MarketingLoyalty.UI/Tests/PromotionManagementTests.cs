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
    [Collection("PromotionManagementTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class PromotionManagementTests : IDisposable, IClassFixture<ReportFixture>
    {
        private ReportFixture reportFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private PromotionManagementPage promotionManagementPage;
        private CampaignManagementPage campaignManagementPage;

        public PromotionManagementTests(ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            memberSearchPage = new MemberSearchPage(webDriver);

            campaignManagementPage = new CampaignManagementPage(webDriver);

            promotionManagementPage = new PromotionManagementPage(webDriver);
        }


        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_PromotionSearchUsingPromotionName_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_PromotionSearchUsingPromotionNameSuccess");

            #region - PreRequisite search for a valid Promotion in Azure Reference DB for a valid campaign code

            string queryForCampaignSearch = "Select Top 1 P.RowId,P.Name as PromotionName,P.Active,P.StartDate,P.EndDate,PP.Name as PartnerName,CP.CampaignId\r\nfrom ((Promotions P Inner join CampaignsPromotionsMapping CP \r\nOn P.RowId=CP.PromotionId ) Inner Join PartnerProfile PP On P.PartnerId=PP.RowId) order by NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string promotionId = promotionDetails[0][0]?.ToString();

            string promotionName = promotionDetails[0][1]?.ToString();

            string promotionActive = promotionDetails[0][2]?.ToString();

            string promotionStartDate = Convert.ToDateTime(promotionDetails[0][3]?.ToString()).ToString("dd-MMM-yyyy");

            string promotionEndDate = Convert.ToDateTime(promotionDetails[0][4]?.ToString()).ToString("dd-MMM-yyyy");

            string partnerName = promotionDetails[0][5]?.ToString();

            string campaignLinked = string.IsNullOrEmpty(promotionDetails[0][6]?.ToString()) ? "N" : "Y";

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (promotionActive.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Promotion Found with the same PromotionName {promotionName}");


            string? promId = result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

            string? promName = result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

            string? promActiveFlag = result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

            string? promStartDate = result.FirstOrDefault()?.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

            string? promEndDate = result.FirstOrDefault()?.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

            string? partner = result.FirstOrDefault()?.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

            string? campaignLinkedFlag = result.FirstOrDefault()?.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");

            Assert.True(promotionId.Equals(promId),
                $"Expected PromotionId to be {promotionId} but found {promId}");

            Assert.True(promotionName.Equals(promName),
                $"Expected PromotionName to be {promotionName} but found {promName}");

            Assert.True(promotionActive.Equals(promActiveFlag),
                $"Expected PromotionActiveFlag to be {promotionActive} but found {promActiveFlag}");

            Assert.True(promotionStartDate.Equals(promStartDate),
                $"Expected PromotionStartDate to be {promotionStartDate} but found {promStartDate}");

            Assert.True(promotionEndDate.Equals(promEndDate),
                $"Expected PromotionEndDate to be {promotionEndDate} but found {promEndDate}");

            Assert.True(partnerName.Equals(partner),
                $"Expected PartnerName to be {partnerName} but found {partner}");

            Assert.True(campaignLinked.Equals(campaignLinkedFlag),
                $"Expected Campaign Linked Flag to be {campaignLinked} but found {campaignLinkedFlag}");

            result.FirstOrDefault()?.FindElement(By.XPath("td[1]/div/div/span")).Click();


            string pName = webDriver.GetAttributeTextValue(PromotionManagement.PromotionNameUpdateTextBox, "value");

            string pStartDate = webDriver.GetAttributeTextValue(PromotionManagement.PromotionStartDateUpdateTextBox, "value");

            string pEndDate = webDriver.GetAttributeTextValue(PromotionManagement.PromotionEndDateUpdateTextBox, "value");

            string pId = webDriver.GetAttributeTextValue(PromotionManagement.PromotionIdUpdate, "value");

            string pActiveFlag = webDriver.FindElement(By.XPath(PromotionManagement.PromotionActiveFlagCheckbox)).GetAttribute("checked");

            string pCampaignLinked = webDriver.FindElement(By.XPath(PromotionManagement.CampaignLinked)).GetAttribute("checked");

            pActiveFlag = pActiveFlag.Equals("true") ? "Y" : "N";

            pCampaignLinked = pCampaignLinked.Equals("true") ? "Y" : "N";

            Assert.True(pId.Equals(promotionId),
                $"The Promotion Id does not match on Promotion Details Page. Expected {pId} but found {promotionId}.");

            Assert.True(pName.Equals(promotionName),
                $"The Promotion Name does not match on Promotion Details Page. Expected {pName} but found {promotionName}.");

            Assert.True(promotionStartDate.Equals(pStartDate),
                $"The StartDate does not match on Promotion Details Page. Expected {promotionStartDate} but found {pStartDate}.");

            Assert.True(promotionEndDate.Equals(pEndDate),
                $"The EndDate does not match on Promotion Details Page. Expected {promotionEndDate} but found {pEndDate}.");

            Assert.True(promotionActive.Equals(pActiveFlag),
                $"The Active Flag does not match on Promotion Details Page. Expected {promotionActive} but found {pActiveFlag}.");

            Assert.True(campaignLinked.Equals(pCampaignLinked),
                $"The Active Flag does not match on Promotion Details Page. Expected {campaignLinked} but found {pCampaignLinked}.");

        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_PromotionSearchUsingPartnerName_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC02_PromotionSearchUsingPartnerNameSuccess");

            #region - PreRequisite search for a valid Promotion in Azure Reference DB for a valid campaign code

            string queryForCampaignSearch = "Select TOP 1 P.Active,PP.Name as PartnerName from Promotions P Inner Join PartnerProfile PP On P.PartnerId=PP.RowId order by NEWID()\r\n";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForCampaignSearch);

            string activeFlag = promotionDetails[0][0]?.ToString();

            string partnerName = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.EnterAndSelectThePartnerNameFromThePartnerNameDropdown(partnerName);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {

                string partner = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                Assert.True(partnerName.Equals(partner),
                    $"Expected PartnerName to be {partnerName} but found {partner}");


            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_PromotionSearchUsingActiveFlag_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_PromotionSearchUsingActiveFlagSuccess");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string activeFlag = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                Assert.True(activeFlag.Equals("Y"),
                    $"Expected Active Flag to be Y but found {activeFlag}");

            }

            promotionManagementPage.ClickOnClearButton();

            promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string activeFlag = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                Assert.True(activeFlag.Equals("N"),
                    $"Expected Active Flag to be N but found {activeFlag}");

            }

        }


        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_PromotionSearchUsingValidStartDateFromAndTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC04_PromotionSearchUsingValidStartDateFromAndTo_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string startDateTo = DateTime.Now.ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterStartDateFrom(startDateFrom);

            promotionManagementPage.EnterStartDateTo(startDateTo);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom) && startDate <= Convert.ToDateTime(startDateTo),
                    $"Expected the startDate range between {startDateFrom} and {startDateTo} but found {startDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_PromotionSearchUsingValidEndDateFromAndTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC05_PromotionSearchUsingValidEndDateFromAndTo_Success");

            string endDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterEndDateFrom(endDateFrom);

            promotionManagementPage.EnterEndDateTo(endDateTo);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value"));

                Assert.True(endDate >= Convert.ToDateTime(endDateFrom) && endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the EndDate range between {endDateFrom} and {endDateTo} but found {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_PromotionSearchUsingValidStartDateFromAndEndDateTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC06_PromotionSearchUsingValidStartDateFromAndEndDateTo_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterStartDateFrom(startDateFrom);

            promotionManagementPage.EnterEndDateTo(endDateTo);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom) && endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the Date range of startDate to be >= {startDateFrom} and EndDate Range to be below <= {endDateTo} but found {startDate.ToString("dd-MMM-yyyy")} and {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(7)]
        public void TC07_PromotionSearchUsingValidStartDateFrom_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC07_PromotionSearchUsingValidStartDateFrom_Success");

            string startDateFrom = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterStartDateFrom(startDateFrom);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                DateTime startDate = Convert.ToDateTime(element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value"));

                Assert.True(startDate >= Convert.ToDateTime(startDateFrom),
                    $"Expected the Date range of startDate to be >= {startDateFrom}  but found {startDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(8)]
        public void TC08_PromotionSearchUsingValidEndDateTo_Success()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_PromotionSearchUsingValidEndDateTo_Success");

            string endDateTo = DateTime.Now.AddYears(+25).ToString("dd-MMM-yyyy");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterEndDateTo(endDateTo);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {

                DateTime endDate = Convert.ToDateTime(element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value"));

                Assert.True(endDate <= Convert.ToDateTime(endDateTo),
                    $"Expected the Date range  EndDate Range to be below <= {endDateTo} but found {endDate.ToString("dd-MMM-yyyy")}");

            }
        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(9)]
        public void TC09_PromotionSearchUsingPromotionNameWildCardCharacter()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC09_PromotionSearchUsingPromotionNameWildCardCharacter");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 Name,Active from [dbo].[Promotions] Where len(Name)>'3' ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            string[] wildCards = { "FirstWordWildCard", "LastWordWildCard", "MiddleWordWildCard" };

            Random random = new();

            string wildCard = wildCards[random.Next(wildCards.Length)];

            int rvalue = random.Next(promotionName.Length);

            switch (wildCard)
            {
                case "FirstWordWildCard":
                    promotionName = string.Concat("*", promotionName.Substring(rvalue, promotionName.Length - rvalue));
                    break;

                case "LastWordWildCard":
                    promotionName = string.Concat(promotionName.Substring(0, promotionName.Length - rvalue), "*");
                    break;

                case "MiddleWordWildCard":
                    promotionName = string.Concat("*", promotionName.Substring(rvalue, promotionName.Length - rvalue), "*");
                    break;

                default:
                    break;
            }

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string promName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                if (wildCard.Equals("LastWordWildCard"))
                    Assert.StartsWith(promotionName.Replace("*", ""), promName, StringComparison.InvariantCultureIgnoreCase);
                else if (wildCard.Equals("FirstWordWildCard"))
                    Assert.EndsWith(promotionName.Replace("*", ""), promName, StringComparison.InvariantCultureIgnoreCase);
                else
                    Assert.True(promName.Contains(promotionName.Replace("*", ""), StringComparison.InvariantCultureIgnoreCase),
                        $"Expected the Searched Promotion Name with Wild Card Character {promotionName} in the result PromotionName:{promName} ");
            }

        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(10)]
        public void TC10_ValidateThatThePromotionIdFieldValueAndCampaignLinkedFlagValueAreDisabledOnPromotionDetailsPage()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC10_ValidateThatThePromotionIdFieldValueAndCampaignLinkedFlagValueAreDisabledOnPromotionDetailsPage");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[3]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate, "No Campaigns Linked Alert Pop Up On Update Page", 10);

            string isPromIdValueDisabled = webDriver.FindElement(By.XPath(PromotionManagement.PromotionIdUpdate)).GetAttribute("disabled");

            string isPromCampaignLinkedFlagDisabled = webDriver.FindElement(By.XPath(PromotionManagement.CampaignLinked)).GetAttribute("disabled");

            Assert.True(isPromIdValueDisabled.Equals("true"),
                "The PromotionId Field Value is enabled. Expected the field to be in Disabled state but found in Enabled State");

            Assert.True(isPromCampaignLinkedFlagDisabled.Equals("true"),
                "The Campaign Linked CheckBox is enabled. Expected the field to be in Disabled state but found in Enabled State");


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(11)]
        public void TC11_PromotionUpdate_PartnerNameUpdate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC11_PromotionUpdate_PartnerNameUpdate_Success");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 PR.Name,PR.Active from Promotions PR Inner Join PartnerProfile PP On PR.PartnerId=PP.RowId and PP.active=1 ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate, "No Campaigns Linked Alert Pop Up On Update Page", 10);

            string queryForPartnerTable = "SELECT TOP 1 PP.Name from PartnerProfile PP Inner Join PartnerTerms PT on PP.RowId=PT.PartnerId and PP.active=1 ORDER BY NEWID()";

            string partnerName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForPartnerTable);

            promotionManagementPage.UpdatePartnerName(partnerName);

            promotionManagementPage.UpdatePromotionDetailsByClickingOnSaveButton();

            string expectedMessage = "Promotion details updated successfully";

            string actualMessage = promotionManagementPage.UpdatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
                $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnUpdatePromotionPage();

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {

                string partner = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                Assert.True(partnerName.Equals(partner),
                    $"Expected PartnerName to be {partnerName} after the Update but found {partner}");
            }


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(12)]
        public void TC12_PromotionUpdate_StartDateUpdate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC12_PromotionUpdate_StartDateUpdate_Success");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 PR.Name,PR.Active from Promotions PR Inner Join PartnerProfile PP On PR.PartnerId=PP.RowId and PP.active=1 ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate,
                "No Campaigns Linked Alert Pop Up On Update Page", 10);

            string startDate = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            promotionManagementPage.UpdateStartDate(startDate);

            promotionManagementPage.UpdatePromotionDetailsByClickingOnSaveButton();

            string expectedMessage = "Promotion details updated successfully";

            string actualMessage = promotionManagementPage.UpdatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
                $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnUpdatePromotionPage();

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string actualStartDate = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                Assert.True(startDate.Equals(actualStartDate),
                    $"Expected the StartDate to be {startDate} after the Update but found {actualStartDate}");
            }


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(13)]
        public void TC13_PromotionUpdate_EndDateUpdate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC13_PromotionUpdate_EndDateUpdate_Success");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 PR.Name,PR.Active from Promotions PR Inner Join PartnerProfile PP On PR.PartnerId=PP.RowId and PP.active=1 ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate,
                "No Campaigns Linked Alert Pop Up On Update Page", 10);

            string endDate = DataGenerator.RandomFutureDate().ToString("dd-MMM-yyyy");

            promotionManagementPage.UpdateEndDate(endDate);

            promotionManagementPage.UpdatePromotionDetailsByClickingOnSaveButton();

            string expectedMessage = "Promotion details updated successfully";

            string actualMessage = promotionManagementPage.UpdatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
                $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnUpdatePromotionPage();

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet);

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string actualEndDate = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                Assert.True(endDate.Equals(actualEndDate),
                    $"Expected the EndDate to be {endDate} after the Update but found {actualEndDate}");
            }


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(14)]
        public void TC14_PromotionUpdate_ActiveFlagUpdate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC14_PromotionUpdate_ActiveFlagUpdate_Success");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 PR.Name,PR.Active from Promotions PR Inner Join PartnerProfile PP On PR.PartnerId=PP.RowId and PP.active=1 ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate,
                "No Campaigns Linked Alert Pop Up On Update Page", 10);

            promotionManagementPage.UpdateActiveFlag();

            promotionManagementPage.UpdatePromotionDetailsByClickingOnSaveButton();

            string expectedMessage = "Promotion details updated successfully";

            string actualMessage = promotionManagementPage.UpdatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
                $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnUpdatePromotionPage();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtoncreate, "No Data Found Pop Up Alert On Search page after Name update", 10);

            promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 20);

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            foreach (IWebElement element in result)
            {
                string actualActiveFlag = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string expectedActiveFlag = activeFlag.Equals("Y") ? "N" : "Y";

                Assert.True(expectedActiveFlag.Equals(actualActiveFlag),
                    $"Expected the Active Flag  to be {expectedActiveFlag} after the Update but found {actualActiveFlag}");
            }


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(15)]
        public void TC15_PromotionUpdate_PromotionNameUpdate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC15_PromotionUpdate_PromotionNameUpdate_Success");

            #region  Test Data from Reference DB      

            string queryForPromotionSearch = "SELECT TOP 1 PR.Name,PR.Active from Promotions PR Inner Join PartnerProfile PP On PR.PartnerId=PP.RowId and PP.active=1 ORDER BY NEWID()";

            object[][] promotionDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForPromotionSearch);

            string promotionName = promotionDetails[0][0]?.ToString();

            string activeFlag = promotionDetails[0][1]?.ToString();

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            result.FirstOrDefault()?.FindElement(By.XPath("td[2]/div/div/span")).Click();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtonUpdate,
                "No Campaigns Linked Alert Pop Up On Update Page", 15);

            string promotionNameNew = "TestPromo" + DataGenerator.RandomString(10).ToLowerInvariant();

            promotionManagementPage.UpdatePromotionName(promotionNameNew);

            promotionManagementPage.UpdatePromotionDetailsByClickingOnSaveButton();

            string expectedMessage = "Promotion details updated successfully";

            string actualMessage = promotionManagementPage.UpdatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
                $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnUpdatePromotionPage();

            webDriver.TryFindElementAndClick(PromotionManagement.PopUpCloseButtoncreate, "No Data Found Pop Up Alert On Search page after Name update", 10);

            promotionManagementPage.ClearThePromotionSearchResults();

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionNameNew);

            if (activeFlag.Equals("N"))
                promotionManagementPage.ClickOnActiveFlagCheckBox();

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            Assert.True(result.Any(),
                $"Expected the Promotion Details when Searched using new Promotion Name {promotionNameNew} after the update but found no results");


        }

        [Trait("Category", Traits.PromotionManagementUIRegression)]
        [Fact, RunOrder(16)]
        public void TC16_PromotionCreate_Success()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC16_PromotionCreate_Success");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            campaignManagementPage.ClickOnBreadCrum(logger);

            promotionManagementPage.ClickOnPromotionManagementLink(logger);

            promotionManagementPage.ClickOnPromotionCreateButton();

            string promotionName = "AutoPromo" + DataGenerator.RandomString(10).ToLowerInvariant();

            string queryForPartnerTable = "SELECT TOP 1 PP.Name from PartnerProfile PP where PP.active=1 ORDER BY NEWID()";

            string partnerName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForPartnerTable);

            string startDate = DataGenerator.RandomPastDate().ToString("dd-MMM-yyyy");

            string endDate = DataGenerator.RandomFutureDate().ToString("dd-MMM-yyyy");

            promotionManagementPage.PromotionCreate_EnterPromotionName(promotionName);

            promotionManagementPage.PromotionCreate_EnterPartnerName(partnerName);

            promotionManagementPage.PromotionCreate_EnterStartDate(startDate);

            promotionManagementPage.PromotionCreate_EnterEndDate(endDate);

            promotionManagementPage.PromotionCreate_SelectActiveFlag();

            promotionManagementPage.CreatePromotionByClickingOnSaveButton();

            string expectedMessage = "Promotion created successfully";

            string actualMessage = promotionManagementPage.CreatePromotionPopUpContent();

            Assert.True(expectedMessage.Equals(actualMessage),
               $"Expected the Update Success Message to be {expectedMessage} but found {actualMessage}.");

            promotionManagementPage.ClosePopUpOnCreatePromotionPage();

            promotionManagementPage.EnterThePromotionNameIntoPromotionNameTextBox(promotionName);

            promotionManagementPage.ClickOnSearchButton();

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);

            webDriver.WaitForElementToBeVisible(PromotionManagement.SearchResultDataSet, 10);

            IEnumerable<IWebElement> result = webDriver.FindElements(By.XPath(PromotionManagement.SearchResultDataSet));

            Assert.True(result.Any(),
               $"No Promotion Found with the same PromotionName {promotionName}");

            foreach (IWebElement element in result)
            {

                string promName = element.FindElement(By.XPath("td[2]/div/div/span")).GetAttribute("value");

                string promActiveFlag = element.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value");

                string promStartDate = element.FindElement(By.XPath("td[4]/div/div/span")).GetAttribute("value");

                string promEndDate = element.FindElement(By.XPath("td[5]/div/div/span")).GetAttribute("value");

                string partner = element.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value");

                string campaignLinkedFlag = element.FindElement(By.XPath("td[7]/div/div/span")).GetAttribute("value");


                Assert.True(promotionName.Equals(promName),
                    $"Expected PromotionName to be {promotionName} but found {promName}");

                Assert.True(startDate.Equals(promStartDate),
                    $"Expected PromotionStartDate to be {startDate} but found {promStartDate}");

                Assert.True(endDate.Equals(promEndDate),
                    $"Expected PromotionEndDate to be {endDate} but found {promEndDate}");

                Assert.True(partnerName.Equals(partner),
                    $"Expected PartnerName to be {partnerName} but found {partner}");

                Assert.True(campaignLinkedFlag.Equals("N"),
                    $"Expected Campaign Linked Flag to be N but found {campaignLinkedFlag}");

                Assert.True(promActiveFlag.Equals("Y"),
                   $"Expected Promotion Active Flag to be Y but found {promActiveFlag}");

            }
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
