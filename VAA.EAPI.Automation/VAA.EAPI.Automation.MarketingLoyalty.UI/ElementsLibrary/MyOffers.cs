namespace VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary
{
    public static class MyOffers
    {

        public static readonly string MyOffersTab = "(//div[@title='Navigate to My Offers'])[1]";

        public static readonly string VoucherAddButton = "(//button[@title='Click to add a new voucher to the member account'])[1]";

        public static readonly string VoucherTypeDropDown = "//div[@data-control-name='cbVoucherType']/div/div/div/div/div[2]";

        public static readonly string VoucherOption = "//*[contains(@id,'powerapps-flyout-react-combobox-view-')]/div/ul/li/div/span[text()='NAME']";

        public static readonly string SelectVoucher = "//ul[@role='listbox']/li/div/span[text() ='VoucherName']";

        public static readonly string VoucherAllOption = "//*[contains(@id,'powerapps-flyout-react-combobox-view')]/div/ul/li/div/span";

        public static readonly string AddButtonOnPopUp = "//div[@data-control-name='btnSaveAddVoucher']/div/div/div/div/button[@title='Click to add a new voucher to the member account']";

        public static readonly string VoucherAddMessage = "//div[@data-control-name='lblResultMessage_3' and not(contains(@style,'display: none;'))]/div/div/div/div/div";

        public static readonly string VoucherClaimButton = "//div[@data-control-name='btnClaimVoucher']/div/div/div/div/button[@title='Click to claim the selected voucher']";

        public static readonly string VoucherCancelGiftButton = "//div[@data-control-name='btnCancelGiftedVoucher']/div/div/div/div/button[@title='Click to cancel the gifting voucher']";

        public static readonly string VoucherTypeSearchTextField = "//*[contains(@id,'powerapps-flyout-react-combobox-view')]/div/div/div/div/input";

        public static readonly string VoucherFirstOption = "//*[contains(@id,'powerapps-flyout-react-combobox-view-')]/div/ul/li[1]/div";

        public static readonly string VoucherFirstRecord = "//*[@data-control-name='MyVouchersDataTable']/div/div/div/div/div/div/div/div/div[2]/table/tbody/tr/td[2]/div/div/span";

        public static readonly string VoucherGiftButtonContainer = "//div[@data-control-name='btnGiftVoucher']/div";

        public static readonly string VoucherStatusAllOptions = "//*[contains(@id,'powerapps-flyout-react-combobox-view')]/div/ul/li/div/span";

        public static readonly string VoucherStatusDropDown = "//*[contains(@data-control-name,'cbVoucherUpdateStatus')]/div/div/div/div/div[2]";

        public static readonly string VoucherUpdateVoucherButton = "//*[@data-control-name='btnUpdateVoucher']/div/div/div/div/button[@title='Click to update the selected voucher']";

        public static readonly string VoucherCalenderButton = "(//div[@aria-label='Open calendar to select a date'])[2]";

        public static readonly string VoucherCalenderYearDropDown = "//select[@aria-label='Year']";

        public static readonly string VoucherCalenderYearDropDownOptions = "//select[@aria-label='Year']/option";

        public static readonly string VoucherCalenderMonthDropDown = "//select[@aria-label='Month']";

        public static readonly string VoucherCalenderMonthDropDownOptions = "//select[@aria-label='Month']/option";

        public static readonly string VoucherCalenderDateDropDown = "//button[@data-pika-day='NAME']";

        public static readonly string VoucherCalenderOkButton = "//button[text()='Ok']";

        public readonly static string PopUpMessageContent = "//div[@data-control-name='lblMessage']/div/div/div/div/div";

        public readonly static string PopUpCloseButton = "//div[@data-control-name='iconClosePopup']/div/div/div/div";

        public readonly static string MyOffersPageDataTable = "//div[@data-control-name='MyVouchersDataTable']//div/table/tbody/tr";

        public readonly static string MyOffersAuditPageDataTable = "//div[@data-control-name='dtVoucherAuditHistory']//div/table/tbody/tr";

        public readonly static string NavigateBackToMyOffersPageButton = "//div[@data-control-name='IconBackVoucherAudit']/div/div/div/div";

    }

}
