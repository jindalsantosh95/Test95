using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class MemberTransactionDetailPage
    {

        private readonly IWebDriver _webDriver;

        public MemberTransactionDetailPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void RetrieveTxnFromMemberDetailsTransactionSection(ExtentTest logger)
        {
            _webDriver.WaitForElementToBePresent(TransactionDetail.CancelTransactionButton);

            logger.Log(Status.Info, "Retail therapy transaction details are present on Member txn details Page");
        }

        public void RetrieveTxnFromMemberDetailsTransactionSectionWithCommentTextVisible(ExtentTest logger)
        {
            _webDriver.WaitForElementToBePresent(TransactionDetail.CommentText);

            logger.Log(Status.Info, "Retail therapy Cancel transaction details are present on Member txn details Page");
        }


        public string RetrieveVredTxnId() => _webDriver.GetElementText(TransactionDetail.TransactionId);

        public string RetrieveComment() => _webDriver.GetElementText(TransactionDetail.Comment);

    }
}
