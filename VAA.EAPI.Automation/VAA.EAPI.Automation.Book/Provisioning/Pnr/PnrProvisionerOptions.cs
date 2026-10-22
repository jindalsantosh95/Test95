namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public sealed record PnrProvisionerOptions
    {
        public bool UsePlaywrightForHpp { get; init; }

        public CardPaymentAutomationMode CardPaymentMode { get; init; } = CardPaymentAutomationMode.Hpp;

        public bool HeadlessHpp { get; init; } = true;

        public static PnrProvisionerOptions ExistingConsumerDefaults { get; } = new();

        public static PnrProvisionerOptions PlaywrightHeadless { get; } = new()
        {
            UsePlaywrightForHpp = true,
            HeadlessHpp = true,
        };

        public static PnrProvisionerOptions ApiCardWithPlaywrightHppFallback { get; } = new()
        {
            CardPaymentMode = CardPaymentAutomationMode.Api,
            UsePlaywrightForHpp = true,
            HeadlessHpp = true,
        };
    }
}
