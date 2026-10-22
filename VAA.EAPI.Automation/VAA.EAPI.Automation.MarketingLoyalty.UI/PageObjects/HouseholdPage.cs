using System.Collections.ObjectModel;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Reporting;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using Status = AventStack.ExtentReports.Status;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class HouseholdPage
    {
        private readonly IWebDriver _webDriver;

        private ReadOnlyCollection<IWebElement> elements;

        public HouseholdPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void ClickOnRelationShipTab(ExtentTest logger)
        {
            IJavaScriptExecutor je = (IJavaScriptExecutor)_webDriver;

            IWebElement element = _webDriver.FindElement(By.XPath(Household.RelationshipTab));

            je.ExecuteScript("arguments[0].scrollIntoView(true);", element);

            _webDriver.FindElementAndClick(Household.RelationshipTab, "RelationShip Tab", 60);

            _webDriver.TryFindElement(Household.RelationShipHistoryHeader);

            _webDriver.WaitForElementToBePresent(Household.HouseholdScreenLoadStatus);

            logger.Log(Status.Info, "Screenshot after clicking on Relationship Tab " + CaptureScreenShot.EmbedImage(_webDriver));
        }

        public bool IsLinkButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Household.LinkButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {Household.LinkButton}.");
            }
            //returns true if button is disabled
        }

        public bool IsDeLinkButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Household.DelinkButton, "disabled").Equals("true");
            }
            catch (Exception ex)
            {
                return !ex.Message.Equals($"Couldn't find the attribute disabled on the element {Household.DelinkButton}.");
            }
            //returns true if button is disabled
        }

        public bool IsValidateAndLinkButtonEnabled()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_webDriver, new TimeSpan(0, 0, 30));

                IWebElement element = wait.Until(driver =>
                {
                    IWebElement candidate =
                        driver.FindElement(By.XPath(Household.ValidateAndLinkBtn));

                    return candidate.Displayed && candidate.Enabled ? candidate : null;
                });

                return element is not null;
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is Enabled
        }

        public void ClickOnValidateAndLink()
        {
            _webDriver.FindElementAndClick(Household.ValidateAndLinkBtn, "validate and Link button on the member link pop up");
        }

        public bool IsChildAccountPresentInHousehold(string childMemberNumber)
        {
            try
            {
                return _webDriver.TryFindElement(Household.ChildMemberNumber.Replace("ChildMemberNumber", childMemberNumber)) is not null;
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if Child Member is Found in the Active Member List

        }

        public void ClickOnDelinkChildMember(string subOrdinateAccountMemberNumber)
        {

            int count = _webDriver.FindElements(By.XPath(Household.MemberIdActive)).Count;

            int associationPeriod;

            bool flag = false;

            for (int i = 1; i <= count; i++)
            {
                string memberIdActive = _webDriver.GetElementText(Household.MemberIdActive + "[" + i + "]");

                if (memberIdActive.Equals(subOrdinateAccountMemberNumber))
                {
                    _webDriver.FindElementAndClick(Household.MemberIdActive + "[" + i + "]", "Child Member to be delinked");

                    DateTime associationDate = DateTime.Parse(_webDriver.GetElementText(Household.MemberAssociationDateActive + "[" + i + "]"));

                    DateTime currentDate = DateTime.UtcNow.Date;

                    associationPeriod = Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays);

                    if (associationPeriod < 365)
                    {
                        _webDriver.FindElementAndClick(Household.MemberOverrideCheckBoxActive + "[" + i + "]", "Override checkbox");

                        _webDriver.FindElementAndClick(Household.DelinkButton, "Delink button");

                        //Less than 12 months pop up message

                        _webDriver.FindElementAndClick(Household.DelinkLessThanTweleveMonthsPopupButton, "Delnk - Less than 12 months Popup- Yes Button");
                    }
                    else
                    {
                        Assert.True(_webDriver.GetAttributeTextValue(Household.MemberOverrideCheckBoxActive + "[" + i + "]", "disabled").Equals("true")
                            , $"Expected the Override Flag Checkbox status to be disabled but found Enabled.");

                        _webDriver.FindElementAndClick(Household.DelinkButton, "Delink button");
                    }

                    flag = true;
                }
                if (flag)
                { break; }
            }

            Assert.True(flag, $"The Child Member {subOrdinateAccountMemberNumber} Not found in the active linked Members List");



        }


        public bool IsChildAccountPresentInHouseholdHistory(string subOrdinateAccountMemberNumber)
        {

            _webDriver.WaitForElementToBeVisible(Household.DelinkHistoryMemberSection);

            elements = _webDriver.FindElements(By.XPath(Household.DelinkHistoryMemberSection));

            foreach (IWebElement childEle in elements)
            {
                string MemberIdActiveInHistorySection = childEle.Text;

                if (MemberIdActiveInHistorySection.Equals(subOrdinateAccountMemberNumber))
                {
                    return true;
                }
            }
            return false;
        }

        public void ClickOnLinkButton() =>
            _webDriver.FindElementAndClick(Household.LinkButton, "Link Button On Relationship(HouseHold Page)");

        public void ClickOnDeLinkButton() =>
            _webDriver.FindElementAndClick(Household.DelinkButton, "DeLink Button On Relationship(HouseHold Page)");

        public string GetPopUpMessage() => _webDriver.GetElementText(Household.LinkDelinkPopUpMessage);

        public void ClosePopUp() => _webDriver.FindElementAndClick(Household.LinkMemberClosePopup, "Link/Delink Pop Up Close Button On Relationship(HouseHold Page)");

        public void EnterChildMemberNumber(string childOrSubOrdinateMemberNumber)
        {
            _webDriver.WaitForElementToBeVisible(Household.SearchChildFCAccountTxtBox);

            _webDriver.FindElementAndEnterText(Household.SearchChildFCAccountTxtBox, childOrSubOrdinateMemberNumber, "Child Or SubOrdinate Member Number Text Box");
        }

        public void ClickOnChildMemberSearchButton() => _webDriver.FindElementAndClick(Household.SearchChildFCAccountButton, "Search Button On Link Member Screen");

        public string GetLinkPopUpErrorMessage() => _webDriver.GetElementText(Household.LinkPopUpErrorMessage);

        public bool ChildAccountPresentWithLinkedByAttribute()
        {
            try
            {
                return _webDriver.TryFindElement(Household.MemberLinkedBy) is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ChildAccountPresentWithLinkedOnAttribute()
        {
            try
            {
                return _webDriver.TryFindElement(Household.MemberLinkedOn) is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ChildAccountPresentWithLinkedByAttributeInHouseholdHistory()
        {
            try
            {
                return _webDriver.TryFindElement(Household.MemberLinkedByHistory) is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ChildAccountPresentWithLinkedOnAttributeInHouseholdHistory()
        {
            try
            {
                return _webDriver.TryFindElement(Household.MemberLinkedOnHistory) is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
