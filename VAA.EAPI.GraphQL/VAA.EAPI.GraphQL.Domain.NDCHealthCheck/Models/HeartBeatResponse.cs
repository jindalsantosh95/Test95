namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models
{
    public class HeartBeatResponse
    {
        public Dictionary<string , HeartBeatInfo> Entries { get; set; }
        public string Status { get; set; }
    }
}
