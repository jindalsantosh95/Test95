namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

public partial class AgencyOCNData
{
    public string Environment { get; set; }
    public string EnvironmentName { get; set; }
    public string EndPoint { get; set; }
    public string Protocol { get; set; }
    public string SoapAction { get; set; }
    public string AuthType { get; set; }
    public string TokenUrl { get; set; }
    public string ClientKey { get; set; }
    public string ClientSecret { get; set; }
    public int? OrgId { get; set; }
    public string AgencyIdType { get; set; }
    public string AgencyClass { get; set; }
    public string CompanyName { get; set; }
    public string AgencyId { get; set; }
    public string BusinessType { get; set; }
    public int ParentId { get; set; }
    public string Status { get; set; }
    public int? PubSub { get; set; }
    public int? RequestOrgId { get; set; }
    public string RequestOrgName { get; set; }
    public string RequestOrgStatus { get; set; }
    public string RequestAgencyId { get; set; }
    public string RequestAgencyIdType { get; set; }
    public int? RequestOrgIdParent { get; set; }
    public string KeyType { get; set; }
    public int Id { get; set; }
    public int EndPointId { get; set; }
}
