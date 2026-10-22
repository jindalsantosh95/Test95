namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Link
    {
        //private static string callback_Search = ConfigReader.GetConfigurationSetting(Constants.Environment.CallBack_Search);
        //private static string callback_Replace = ConfigReader.GetConfigurationSetting(Constants.Environment.CallBack_Replace);

        public string Rel { get; set; }

        private string _href;
        public string Href
        {
            get
            {
                return _href;
            }
            set
            {
                _href = value; // value.Replace(callback_Search, callback_Replace, System.StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}
