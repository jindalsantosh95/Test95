namespace VAA.EAPI.Automation.Core.Helpers
{
    public class ValidationResultHelper
    {
        public bool IsValid => !(Message?.Any() ?? false);
        public List<string> Message { get; }

        public ValidationResultHelper()
        {
            Message = new List<string>();
        }
    }
}
