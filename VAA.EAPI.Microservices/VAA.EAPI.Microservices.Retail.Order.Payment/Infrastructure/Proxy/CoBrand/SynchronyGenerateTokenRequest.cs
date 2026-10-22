using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    
    public class SynchronyGenerateTokenRequest
    {
        public Applicantinfo ApplicantInfo { get; set; }
        public Merchantinfo MerchantInfo { get; set; }
        public Strategyinfo StrategyInfo { get; set; }
        public string ApplicationType { get; set; }
        public Featuresinfo FeaturesInfo { get; set; }
        public Partnerconfiginfo[] PartnerConfigInfo { get; set; }
        public Applicationstatus ApplicationStatus { get; set; }
        public Analyticsdata AnalyticsData { get; set; }
        public string LanguageIndicator { get; set; }
        public string ReturnURL { get; set; }
        public string ReturnToMerchantUrl { get; set; }

        public static explicit operator SynchronyGenerateTokenRequest(GenerateSynchronyTokenServiceCommand source)
        {
            return new SynchronyGenerateTokenRequest()
            {
                ApplicantInfo = new()
                {
                    CipherfirstName = source.ApplicantDetails.FirstName,
                    CipherlastName = source.ApplicantDetails.LastName,
                    CipheremailAddress = source.ApplicantDetails.Contacts.Email,
                    CipherdateOfBirth = Convert.ToDateTime(source.ApplicantDetails.DateOfBirth).ToString("yyyy-MM-dd") ,
                    MembershipInfo = new()
                    {
                        CipherrewardsNumber = source.ApplicantDetails.LoyaltyMemberDetails.MemberNumber
                    },
                    CipherinitialPurchaseAmount = source.InitialPurchaseAmount
                },
                MerchantInfo = new()
                {
                    ClientCode = Constants.Application.SynchronyClientCode,
                    SiteCode = ConfigReader.Try(Constants.Application.SynchronySiteCode)
                },
                StrategyInfo = new() { AcorId = ConfigReader.Try(Constants.Application.SynchronyAcorId), AccountInfo = source.AccountInfo },
                ApplicationType = "APPLY"
            };
        }
    }





   

    

}
