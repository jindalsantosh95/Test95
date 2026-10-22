namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models
{
    public class HeartBeatInfo
    {
        public HeartBeatData Data { get; set; }
        public string Description {  get; set; }
        public string Status {  get; set; }
        public string Duration {  get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
