using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
namespace VAA.EAPI.Automation.Core.UI.PageObjects.DotCom
{
    public class HomePage : WebBasePage
    {
        public HomePage(IWebDriver driver) : base(driver) { }

        // Only define critical stable elements
        protected override Dictionary<string, By[]> ElementLocators => new Dictionary<string, By[]>
        {
            { "holidaysButton", new[]
                {
                    By.CssSelector("button[data-cy='brand-toggle-VHOLS']"),
                    By.CssSelector("button[data-testid='button-component']"),
                    By.XPath("//button[span[text()='Holidays']]")
                }
            }
        };

        // High-level interaction
        public void ClickHolidaysButton()
        {
            Click("holidaysButton"); // will fallback dynamically if needed
        }

        public void SelectFromTypeahead(string value, string optionsXPath)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
            var option = wait.Until(d =>
            {
                var elements = d.FindElements(By.XPath(optionsXPath))
                                .Where(e => e.Displayed)
                                .ToList();
                return elements.FirstOrDefault(e => e.Text.Contains(value));
            });

            if (option != null)
                option.Click();
            else
                throw new Exception($"Typeahead option '{value}' not found using XPath '{optionsXPath}'");
        }
    }
}