using OpenQA.Selenium;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using VAA.EAPI.Automation.Core.UI.PageObjects.DotCom;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.VirginHolidays
{
    public class FrontEndMonitoring : IDisposable
    {
        private readonly IWebDriver _driver;

        private readonly HomePage _homePage;

        public static string url = "https://stg.virginatlantic.com/";

        public FrontEndMonitoring()
        {
            _driver = GetBrowser(BrowserType.Edge, url);
            
            _homePage = new HomePage(_driver);
        }

        [Fact(DisplayName = "Dynamic Fallback Test Using Text Only")]
        public void Flights_Test()
        {
            // This will use the auto-locator fallback
            _homePage.Click("Yes, I Agree");
            _homePage.Click("flights_from");
        }

        [Theory(DisplayName = "Dynamic Fallback Test Using Text Only")]
        [InlineData("Miami, Florida", "London Heathrow", "10", "20")]
        [InlineData("New York, USA", "London Heathrow", "10", "17")]
        [InlineData("Maldives, Indian Ocean", "London Heathrow", "5", "12")]
        public void Holiday_Test(string destination, string flyingFrom, string depatureDay, string returnDay)
        {
            var js = "Object.defineProperty(navigator, 'webdriver', {get: () => undefined});";
            ((IJavaScriptExecutor)_driver).ExecuteScript(js);

            // This will use the auto-locator fallback
            _homePage.Click("Yes, I Agree");
            _homePage.Click("Holidays");
            _homePage.EnterText($"flight_hotel_destination", destination);
            _homePage.Click($"//button/label[normalize-space() = '{destination}']");
            _homePage.Click($"//span[@data-testid='typography-component' and normalize-space(text())='{flyingFrom}']");
            _homePage.Click($"//*[@id='popover-when']/section/div/section/section/div[2]/div[3]/div/button[{depatureDay}]");
            _homePage.Click($"//*[@id='popover-when']/section/div/section/section/div[2]/div[3]/div/button[{returnDay}]");
            _homePage.Click("//*[@data-cy='submit-button']");
        }

        public void Dispose()
        {
            _driver.DriverCleanUp();
        }
    }
}
