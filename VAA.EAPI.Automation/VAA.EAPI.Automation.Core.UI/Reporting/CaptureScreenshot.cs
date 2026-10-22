using OpenQA.Selenium;


namespace VAA.EAPI.Automation.Core.UI.Reporting
{
    public sealed class CaptureScreenShot
    {
        public static string EmbedImage(IWebDriver driver, int width = 640)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();

            return $"<img width=\"{width}\" src=\"data:image/png;base64,{screenshot.AsBase64EncodedString}\">";
        }
    }
}