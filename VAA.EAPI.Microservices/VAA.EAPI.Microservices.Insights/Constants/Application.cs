namespace VAA.EAPI.Microservices.Insights.Constants
{
    public static class Application
    {
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string APIMApiVersion = "2022-08-01";
        public static readonly string DeploymentStatus = "insights:redis:deploymentstatus:eventing";

        public const string ADOPipelineTopic = "adopipelineevents";
        public const string ADOPipelineSubscription = "%ADOPipelineSubscription%";
        public const string ADOPipelineSource = "VAA.EAPI.Microservices.Insights.ADOServiceBusTrigger";

    }
}