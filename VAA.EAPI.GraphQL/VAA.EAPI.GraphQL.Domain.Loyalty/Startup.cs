using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Helper.Caching;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mutations;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Domain.Loyalty.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty
{
    public sealed class Startup : IStartup
    {
        public void ConfigureAppConfiguration(IConfigurationBuilder config)
        {

        }

        public void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder worker)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IContactNoteQueryProvider, ContactNoteQueryProvider>()
                    .AddSingleton<ICustomerProvider, CustomerService>()
                    .AddSingleton<ITargetedMembershipQueryProvider, TargetedMembershipQueryProvider>()
                    .AddSingleton<IMemberCampaignQueryProvider, MemberCampaignQueryProvider>()
                    .AddSingleton<IMemberAccountQueryProvider, MemberAccountQueryProvider>()
                    .AddSingleton<ICampaignTypesQueryProvider, CampaignTypesQueryProvider>()
                    .AddSingleton<IPartnersQueryProvider, PartnersQueryProvider>()
                    .AddSingleton(typeof(ITargetedMembershipMutationProvider<>), typeof(TargetedMembershipMutationProvider<>))
                    .AddSingleton<ICustomerMutationProvider, CustomerMutationProvider>()
                    .AddSingleton(typeof(IContactNoteMutationProvider<>), typeof(ContactNoteMutationProvider<>))
                    .AddSingleton<ICampaignManagementMutationProvider, CampaignManagementMutationProvider>()
                    .AddSingleton<IPromotionManagementMutationProvider, PromotionManagementMutationProvider>()
                    .AddSingleton<IPromotionSearchQueryProvider, PromotionSearchQueryProvider>()
                    .AddSingleton<IPromotionCampaignSearchQueryProvider, PromotionCampaignSearchQueryProvider>()
                    .AddSingleton<IPromotionEnrolledMembersQueryProvider, PromotionEnrolledMembersQueryProvider>()
                    .AddSingleton<IPromotionsListQueryProvider, PromotionsListQueryProvider>()
                    .AddSingleton<IMemberHouseholdQueryProvider, MemberHouseholdQueryProvider>()
                    .AddSingleton<IHouseHoldMutationProvider, HouseHoldMutationProvider>()
                    .AddSingleton<ISupplementarySilverAccountQueryProvider, SupplementarySilverAccountQueryProvider>()
                    .AddSingleton<ISupplementarySilverMutationProvider, SupplementarySilverMutationProvider>()
                    .AddSingleton<ICampaignSearchQueryProvider, CampaignSearchQueryProvider>()
                    .AddSingleton<ICampaignRetrieveQueryProvider, CampaignRetrieveQueryProvider>()
                    .AddSingleton<IAssociatedMembersQueryProvider, AssociatedMembersQueryProvider>()
                    .AddSingleton<ICampaignsListQueryProvider, CampaignsListQueryProvider>()
                    .AddSingleton<IAddressHistoryQueryProvider, AddressHistoryQueryProvider>()
                    .AddSingleton<IContactHistoryQueryProvider, ContactHistoryQueryProvider>()
                    .AddSingleton<IPreferenceAuditHistoryQueryProvider, PreferenceAuditHistoryQueryProvider>()
                    .AddSingleton<IAwardAuditHistoryQueryProvider, AwardAuditHistoryQueryProvider>()
                    .AddSingleton<IAmexCenturionAuditHistoryQueryProvider, AmexCenturionAuditHistoryQueryProvider>()
                    .AddSingleton<ICustomerProfileAuditHistoryQueryProvider, CustomerProfileAuditHistoryQueryProvider>()

                    .AddSingleton<ISupplementarySilverAuditHistoryQueryProvider, SupplementarySilverAuditHistoryQueryProvider>()
                    .AddSingleton<ISkyPartnerRetroMutationProvider, SkyPartnerRetroMutationProvider>()
                    .AddSingleton<IReferenceDataMutationProvider, ReferenceDataMutationProvider>()
                    .AddSingleton<IInRuleMutationProvider, InRuleMutationProvider>()

                    .AddSingleton<ISupplementarySilverAuditHistoryQueryProvider, SupplementarySilverAuditHistoryQueryProvider>()
                    .AddSingleton<IVredCardDetailsQueryProvider, VredCardDetailsQueryProvider>()
                    .AddSingleton<IPartnersProfilesQueryProvider, PartnersProfilesQueryProvider>()
                    .AddSingleton(typeof(ICustomerTransactionMutationProvider<>), typeof(CustomerTransactionMutationProvider<>))
                    .AddSingleton<IPartnerMembershipDocQueryProvider, PartnerMembershipDocQueryProvider>()
                    .AddSingleton<IProductsQueryProvider, ProductsQueryProvider>()
                    .AddSingleton<ICustomerTransactionQueryProvider, CustomerTransactionQueryProvider>()
                    .AddSingleton<IVredMutationProvider, VredMutationProvider>()
                    .AddSingleton<ICacheService, CacheService>()
                    .AddSingleton<IPNRQueryProvider, PNRQueryProvider>()
                    .AddSingleton(typeof(ILoyaltySyncMutationProvider<>), typeof(LoyaltySyncMutationProvider<>))
                    .AddSingleton<ILoyaltyMemberMgtProvider, LoyaltyMemberMgtProvider>()
                    .AddSingleton<IFutureFlightsQueryProvider, FutureFlightsQueryProvider>()
                    .AddSingleton(typeof(IMemberProfileMutationProvider<>), typeof(MemberProfileMutationProvider<>))
                    .AddSingleton<ILoyaltyReferenceDataProvider, LoyaltyReferenceDataProvider>()
                    .AddSingleton<ICustomerQueryProvider, CustomerQueryProvider>()
                    .AddSingleton<IStampFCNumToPNRMutationsProvider, StampFCNumToPNRMutationsProvider>()
                    .AddSingleton<IEligiblePNRsQueryProvider, EligiblePNRsQueryProvider>()
                    .AddSingleton<IAddressSearchLookupQueryProvider, AddressSearchLookupQueryProvider>()
                    .AddSingleton<IBulkMemberUpdateMutationProvider, BulkMemberUpdateMutationProvider>()
                    .AddSingleton<IPartnerDataManagementQueryProvider, PartnerDataManagementQueryProvider>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<CampaignManagementMutations>()
                       .TryAddType<ContactNoteMutation>()
                       .TryAddType<CustomerMutation>()
                       .TryAddType<TargetedMembershipMutation>()
                       .TryAddType<PromotionManagementMutations>()
                       .TryAddType<HouseHoldMutations>()
                       .TryAddType<SupplementarySilverMutation>()
                       .TryAddType<PartnerMembershipMutations>()
                       .TryAddType<SkyPartnerRetroMutation>()
                       .TryAddType<TransactionManagementMutations>()
                       .TryAddType<LoyaltySyncMutations>()
                       .TryAddType<PNRAndFareCalcMutation>()
                       .TryAddType<MemberMergeMutations>()
                       .TryAddType<UpdateMemberProfileMutation>()
                       .TryAddType<UpgradeVACMemberMutation>()
                       .TryAddType<StampFCNumToPNRMutations>()
                       .TryAddType<BulkMemberUpdateMutation>()
                       .TryAddType<PartnerDataManagementMutation>()
                       .TryAddType<RetroClaimMutation>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<EnrolCustomerMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<ContactNotesQuery>()
                       .TryAddType<CustomersQuery>()
                       .TryAddType<CustomerType>()
                       .TryAddType<TargetedMembershipQuery>()
                       .TryAddType<MemberCampaignQuery>()
                       .TryAddType<MemberAccountQuery>()
                       .TryAddType<CampaignTypesQuery>()
                       .TryAddType<PartnersQuery>()
                       .TryAddType<PromotionSearchQuery>()
                       .TryAddType<PromotionCampaignSearchQuery>()
                       .TryAddType<PromotionEnrolledMembersQuery>()
                       .TryAddType<PromotionsListQuery>()
                       .TryAddType<MemberHouseholdQuery>()
                       .TryAddType<SupplementarySilverAccountQuery>()
                       .TryAddType<CampaignSearchQuery>()
                       .TryAddType<CampaignRetrieveQuery>()
                       .TryAddType<CampaignAssociatedMembersQuery>()
                       .TryAddType<CampaignsListQuery>()
                       .TryAddType<AddressHistoryQuery>()
                       .TryAddType<ContactHistoryQuery>()
                       .TryAddType<PreferenceAuditHistoryQuery>()
                       .TryAddType<AwardAuditHistoryQuery>()
                       .TryAddType<AmexCenturionAuditHistoryQuery>()
                       .TryAddType<CustomerProfileAuditHistoryQuery>()
                       .TryAddType<SupplementarySilverAuditHistoryQuery>()
                       .TryAddType<MemberCardLinksQuery>()
                       .TryAddType<FutureFlightsQuery>()
                       .TryAddType<MemberSearchPhoneQuery>()
                       .TryAddType<AddressSearchLookupQuery>()
                       .TryAddType<AddressSearchLookupRetrieveQuery>()
                       .TryAddType<EligiblePNRsQuery>()
                       .TryAddType<TransactionManagementQuery>()
                       .TryAddType<PartnerDataManagementQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
