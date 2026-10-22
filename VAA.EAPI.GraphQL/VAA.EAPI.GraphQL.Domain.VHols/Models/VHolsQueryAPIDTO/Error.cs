namespace VAA.EAPI.GraphQL.Domain.VHols.Models.VHolsQueryAPIDTO;

public class Error
{
    public string cause { get; set; }
    public string locations { get; set; }
    public string errorType { get; set; }
    public string message { get; set; }
    public string path { get; set; }
    public string extensions { get; set; }
    public string localizedMessage { get; set; }
    public object[] suppressed { get; set; }
}