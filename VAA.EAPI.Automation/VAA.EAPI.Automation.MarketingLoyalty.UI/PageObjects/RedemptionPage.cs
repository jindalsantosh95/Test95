using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects
{
    public class RedemptionPage
    {
        private readonly IWebDriver _webDriver;

        public RedemptionPage(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void NavigateToRedemptionPage()
        {
            _webDriver.FindElementAndClick(MemberProfile.MemberDetailMenuBtn, "Menu Bar on Member Detail page");

            _webDriver.FindElementAndClick(Redemption.RedemptionTabBtn, "Redemption Tab");

            _webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads);
        }

        public void EnterPartnerCode(string partnerCode)
        {
            if (!string.IsNullOrEmpty(partnerCode))
            {
                _webDriver.FindElementAndClick(Redemption.PartnerCodeDropDownBtn,
                    "Partner Code dropdown");

                _webDriver.FindElementAndClick(Redemption.PartnerCodeSelect.Replace("partnerCode", partnerCode),
                    $" partner code {partnerCode} from the dropdown list ");
            }
        }

        public void EnterProduct(string product)
        {
            if (!string.IsNullOrEmpty(product))
            {
                _webDriver.FindElementAndClick(Redemption.ProductDropDownBtn,
                    "Product dropdown");

                _webDriver.FindElementAndClick(Redemption.ProductSelect.Replace("productName", product),
                    $"Product {product} from the dropdown list ");
            }
        }

        public void EnterActivityDate(string activityDate) => _webDriver.FindElementAndEnterText(Redemption.ActiviyDateTxtBox, activityDate, "Activity date Text Box", 10);

        public void EnterPoints(string points) => _webDriver.FindElementAndEnterText(Redemption.PointsTxtBox, points, "Points Text Box", 10);
        public void EnterComments(string comments) => _webDriver.FindElementAndEnterText(Redemption.CommentsTxtBox, comments, "Comments Text Box", 10);
        public void EnterOriginAirportCode(string originAirportCode)
        {
            if (!string.IsNullOrEmpty(originAirportCode))
            {
                _webDriver.FindElementAndClick(Redemption.OriginAirportCodeDropdown,
                    "Origin Airport Code dropdown");

                _webDriver.FindElementAndClick(Redemption.OriginAiportCodeselect.Replace("OriginAirportCode", originAirportCode),
                    $"OriginAirportCode {originAirportCode} from the dropdown list ");
            }
        }

        public void EnterDestinationAirportCode(string destinationAirportCode)
        {
            if (!string.IsNullOrEmpty(destinationAirportCode))
            {
                _webDriver.FindElementAndClick(Redemption.DestinationAirportCodeDropdown,
                    "Destination Airport Code dropdown");

                _webDriver.FindElementAndClick(Redemption.DestinationAiportCodeselect.Replace("DestinationAirportCode", destinationAirportCode),
                    $"DestinationAirportCode {destinationAirportCode} from the dropdown list ");
            }
        }

        public void EnterPNR(string pnr) => _webDriver.FindElementAndEnterText(Redemption.PNRTxtBox, pnr, "PNR Text Box");

        public void EnterBookingClass(string bookingClass) => _webDriver.FindElementAndEnterText(Redemption.BookingClassTxtBox, bookingClass, "BookingClass Text Box");

        public void EnterNoOfSegments(string noOfSegments) => _webDriver.FindElementAndEnterText(Redemption.NoOfSegmentsTxtBox, noOfSegments, "No Of Segments Text Box");

        public void EnterNoOfPassengers(string noOfPassengers) => _webDriver.FindElementAndEnterText(Redemption.NoOfPasngrsTxtBox, noOfPassengers, "No Of Passengers Text Box");
        public void EnterTicketNumber(string ticketNumber) => _webDriver.FindElementAndEnterText(Redemption.TicketNumberTxtBox, ticketNumber, "Ticket Number Text Box");
        public void ClickOnApplyRedemptionButton() => _webDriver.FindElementAndClick(Redemption.ApplyRedemptionBtn, "Apply Redemption Button");
        public bool IsApplyRedemptionButtonDisabled()
        {
            try
            {
                return _webDriver.GetAttributeTextValue(Redemption.ApplyRedemptionBtn, "disabled").Equals("true");
            }
            catch (Exception)
            {
                return false;
            }
            //returns true if button is disabled
        }

        public string GetPopUpMessageContent()
        {
            _webDriver.FindElement(ManualAdjustment.SuccessPopupMsg, "Manual Adjustment Pop Up Message", 120);

            return _webDriver.GetAttributeTextValue(ManualAdjustment.SuccessPopupMsg, "textContent");
        }

        public void ClickOnClearAllButton() => _webDriver.FindElementAndClick(Redemption.ClearAllBtn, "ClearAll Details Button");

        public void SelectVoucher(string voucherNumber)
        {
            _webDriver.FindElementAndClick(Redemption.MyOffers_VouchersDropdown, "MyOffers Vouchers dropdown");

            _webDriver.FindElementAndClick(Redemption.MyOffers_VoucherName.Replace("VoucherNumber", voucherNumber), $"Voucher Number {voucherNumber} from the dropdown list ");
        }

        public void EnterVoucherPointsToBeCredited(string voucherPoints) => _webDriver.FindElementAndEnterText(Redemption.VirginPointsToBeCreditedtextBox, voucherPoints, "Enter Virgin points to Be credited for the voucher accrual");

        public void ClickOnClosePopupIcon()
        {
            _webDriver.FindElementAndClick(Redemption.PopUpCloseBtn, "Click on Close Successful redemption popup on the screen", 15);

            _webDriver.TryFindElementAndClick(MemberProfile.AlertCloseButton, "Alert Pop Up", 10);

            _webDriver.WaitForElementToBePresent(MemberProfile.MemberDetailsScreenLoadStatus);
        }

        public void EnterVoucherProduct(string voucherProduct)
        {
            if (!voucherProduct.IsNullOrEmpty())
            {
                _webDriver.FindElementAndClick(Redemption.VoucherDropDownBtn, "Voucher dropdown");

                _webDriver.FindElementAndClick(Redemption.VoucherSelect.Replace("VoucherName", voucherProduct), $" Voucher {voucherProduct} from the dropdown list ");
            }
        }

        public void EnterVoucherPoints(string voucherPoints) => _webDriver.FindElementAndEnterText(Redemption.VoucherpointsTxtBox, voucherPoints, "Voucher Points Text Box", 15);

    }
}
