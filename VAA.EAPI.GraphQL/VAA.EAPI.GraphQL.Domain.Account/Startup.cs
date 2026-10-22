using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Handlers;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Domain.Account.Queries;
using VAA.EAPI.GraphQL.Domain.Account.Queries.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account
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
            services.AddSingleton(typeof(ICustomerTransactionQueryProvider<>), typeof(CustomerTransactionQueryProvider<>))
                    .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
                    .AddSingleton(typeof(IOTPMutationProvider<>), typeof(OTPMutationProvider<>))
                    .AddSingleton(typeof(IEnrolmentProvider), typeof(EnrolmentProvider))
                    .AddSingleton<IAddressLookupV3Provider, AddressLookupV3Provider>()
                    .AddSingleton<YoungFlyerAccountRequestFactory>()
                    .AddSingleton(typeof(IVoucherQueryProvider), typeof(VoucherQueryProvider))
                    .AddSingleton(typeof(IFlightQueryProvider), typeof(FlightQueryProvider))
                    .AddSingleton(typeof(IActivityQueryProvider), typeof(ActivityQueryProvider))
                    .AddSingleton(typeof(IAccountCustomerService), typeof(CustomerService))
                    .AddSingleton(typeof(IRefDataService<>), typeof(RefDataService<>))
                    .AddSingleton(typeof(ITransactionMutationProvider), typeof(TransactionMutationProvider))
                    .AddSingleton(typeof(IVredPointProvider), typeof(VredPointProvider))
                    .AddSingleton(typeof(IHouseHoldMutationProvider), typeof(HouseHoldMutationProvider))
                    .AddSingleton(typeof(IPDMProvider), typeof(PDMProvider))
                    .AddSingleton(typeof(IRulesEngineProvider<>), typeof(RulesEngineProvider<>))
                    .AddSingleton(typeof(ILoyaltyMemberManagementProvider), typeof(LoyaltyMemberManagementProvider))
                    .AddSingleton(typeof(ISkyRetroProvider<>), typeof(SkyRetroProvider<>))
                    .AddSingleton(typeof(ICustomerMutationProvider<>), typeof(CustomerMutationProvider<>))
                    .AddSingleton(typeof(IPnrDetailsProvider), typeof(PnrDetailsProvider))
                    .AddSingleton(typeof(IPnrUpdateMutationsProvider), typeof(PnrUpdateMutationsProvider))
                    .AddSingleton(typeof(IStampingPNRsHandler), typeof(StampingPNRsHandler))
                    .AddSingleton(typeof(IPnrQueryProvider), typeof(PnrQueryProvider))
                    .AddSingleton(typeof(IMemberCustomAttributeProvider), typeof(MemberCustomAttributeProvider))
                    .AddSingleton(typeof(IEntraServicesProvider<>), typeof(EntraServicesProvider<>))
                    .AddSingleton(typeof(IInflightConnectivityMutationProvider), typeof(InflightConnectivityMutationProvider))
                    .AddSingleton(typeof(ICampaignProvider), typeof(CampaignProvider))
                    .AddSingleton(typeof(IReferenceDataProvider), typeof(ReferenceDataProvider))
                    .AddSingleton<IFlyingClubTokenProvider, FlyingClubTokenProvider>()
                    .AddSingleton<IFlyingClubCustomerProfileProvider, FlyingClubCustomerProfileProvider>()
                    .AddSingleton<IFlyingClubAccountSummaryProvider, FlyingClubAccountSummaryProvider>()
                    .AddSingleton<IAccountLinkingProvider, AccountLinkingProvider>()
                    .AddSingleton<IAccountLinkingEligibilityService, AccountLinkingEligibilityService>()
                    .AddSingleton<IManagedYoungFlyerAccountProvider, ManagedYoungFlyerAccountProvider>()
                    .AddSingleton<IManagedYoungFlyerRelationshipProvider, ManagedYoungFlyerRelationshipProvider>()
                    .AddSingleton(TimeProvider.System);
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            // Customer proof creation is available on both hosts; the operation enforces authentication.
            builder.TryAddType<AccountLinkingProofMutation>();
            var configuredModes = Common.Helper.ConfigReader.Try(Common.Constants.Environment.OperationsVisibilityMode) ?? string.Empty;
            if (isSecure || System.Diagnostics.Debugger.IsAttached ||
                configuredModes.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Any(mode => mode.Is("Secure")))
                builder.TryAddType<AccountLinkingMutation>();
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<TransactionMutations>()
                       .TryAddType<FlyingClubTokenMutation>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<AddressLookupV3Mutation>()
                       .TryAddType<EnrolFlyingClubMemberMutation>()
                       .TryAddType<CreateYoungFlyerAccountMutation>()
                       .TryAddType<AccountSignInMutation>()
                       .TryAddType<ResendVerificationEmailMutation>()
                       .TryAddType<PasswordResetConfirmationMutation>()
                       .TryAddType<EmailMutation>()
                       .TryAddType<OTPGenerationMutation>()
                       .TryAddType<OTPValidationMutation>()
                       .TryAddType<UpdateEmailAddressMutation>()
                       .TryAddType<UpdateMarketingPreferenceMutation>()
                       .TryAddType<UpdatePreferenceMutation>()
                       .TryAddType<UpdateAddressMutation>()
                       .TryAddType<UpdatePhoneNumberMutation>()
                       .TryAddType<InviteMemberMutation>()
                       .TryAddType<ClaimVSPointsMutation>()
                       .TryAddType<InviteActionMutation>()
                       .TryAddType<ClaimPartnerFlightsPointsMutation>()
                       .TryAddType<VirginAccountUpgradeMutation>()
                       .TryAddType<ManageBannerBannerMutation>()
                       .TryAddType<PnrLinkMutation>()
                       .TryAddType<RefreshTokenMutation>()
                       .TryAddType<PasswordResetStartMutation>()
                       .TryAddType<EntraOtpValidationMutation>()
                       .TryAddType<EntraNewPasswordSubmissionMutation>()
                       .TryAddType<AccountEnableIFWifiForDeviceMutation>()
                       .TryAddType<RegisterCampaignMutation>()
                       .TryAddType<InflightEnrolMutation>()
                       .TryAddType<EntraOTPGenerationMutation>()
                       .TryAddType<EntraEnrolFlyingClubMemberMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<ManageAccountQuery>()
                       .TryAddType<FlyingClubCustomerProfileQuery>()
                       .TryAddType<MemberDetailsQuery>()
                       .TryAddType<MemberDetailsQueryResolver>()
                       .TryAddType<FlyingClubAccountSummaryQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<AddressLookupV3Query>()
                       .TryAddType<LoginServiceQuery>()
                       .TryAddType<AddressDetailsQuery>()
                       .TryAddType<MemberDetailsQuery>()
                       .TryAddType<VouchersQuery>()
                       .TryAddType<MemberVouchersQuery>()
                       .TryAddType<FlightDetailsQuery>()
                       .TryAddType<ActivityQuery>()
                       .TryAddType<PointsDotComQuery>()
                       .TryAddType<ManageAccountQuery>()
                       .TryAddType<RetrieveHouseHoldAccountQuery>()
                       .TryAddType<PasswordPolicyQuery>()
                       .TryAddType<GenerateWalletPassQuery>()
                       .TryAddType<MemberDetailsQueryResolver>()
                       .TryAddType<BannerDetailsQuery>()
                       .TryAddType<TenureDetailsQuery>()
                       .TryAddType<ValidatePcmEligibilityQuery>()
                       .TryAddType<ManagedYoungFlyerAccountsQuery>()
                       .TryAddType<ManagedYoungFlyerAccountsQueryResolver>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
