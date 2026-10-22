namespace VAA.EAPI.Automation.ArchitectureTests
{
    /// <summary>
    /// The architectural layer a project belongs to, inferred from its name.
    /// </summary>
    internal enum ProjectLayer
    {
        /// <summary>Shared foundation libraries: Core, Core.Delta, Core.UI, Core.GraphQL.</summary>
        Core,

        /// <summary>API / GraphQL / SOAP automation suites.</summary>
        Domain,

        /// <summary>Selenium / Appium UI automation suites (names ending in .UI).</summary>
        Ui,

        /// <summary>Supporting tools and runners (Utility.* projects).</summary>
        Utility,
    }
}
