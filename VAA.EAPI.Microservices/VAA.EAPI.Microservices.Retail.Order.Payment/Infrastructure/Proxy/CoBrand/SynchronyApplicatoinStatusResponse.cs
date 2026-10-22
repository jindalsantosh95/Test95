using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class SynchronyApplicatoinStatusResponse
    {
        public string DataShareToken { get; set; }
        public Applicantinfo ApplicantInfo { get; set; }
        public Jointapplicantinfo JointApplicantInfo { get; set; }
        public Merchantinfo Merchantinfo { get; set; }
        public Partnerconfiginfo[] PartnerConfigInfo { get; set; }
        public string ApplicationType { get; set; }
        public string TokenStatus { get; set; }
        public string CipherdataShareToken { get; set; }
        public Applicationstatus ApplicationStatus { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
