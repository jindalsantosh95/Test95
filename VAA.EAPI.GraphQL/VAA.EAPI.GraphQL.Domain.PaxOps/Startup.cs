using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps
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
            services.AddSingleton(typeof(ICheckInServiceProvider), typeof(CheckInServiceProvider));
            services.AddSingleton(typeof(IReferenceServiceProvider), typeof(ReferenceServiceProvider));
            services.AddSingleton(typeof(IBaggageServiceProvider), typeof(BaggageServiceProvider));
            services.AddSingleton(typeof(ILogAnalytics), typeof(LogAnalytics));
            services.AddSingleton(typeof(IBasketServiceProvider), typeof(BasketServiceProvider));
            services.AddSingleton(typeof(IPaymentServiceProvider), typeof(PaymentServiceProvider));
            services.AddSingleton(typeof(ITripsDetailsQueryProvider), typeof(TripsDetailsQueryProvider));
            services.AddSingleton(typeof(ITravelRequirementsServiceProvider), typeof(TravelRequirementsServiceProvider));
            services.AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>));
            services.AddSingleton(typeof(IWalletServiceProvider), typeof(WalletServiceProvider));
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<SelectPassengersMutation>();
                builder.TryAddType<SavePassportMutation>();
                builder.TryAddType<SaveContactTracingMutation>();
                builder.TryAddType<EmergencyContactDetailsMutation>();
                builder.TryAddType<SaveReturnDateMutation>();
                builder.TryAddType<BoardingPassMutation>();
                builder.TryAddType<SaveBaggageMutation>();
                builder.TryAddType<ConfirmBaggageMutation>();
                builder.TryAddType<PaymentEligibilityCheckMutation>();
                builder.TryAddType<Mutations.TravelRequirements.SavePassportMutation>();
                builder.TryAddType<Mutations.TravelRequirements.TravelRequirementsSaveReturnDateMutation>();
                builder.TryAddType<Mutations.TravelRequirements.SaveEmergencyContactMutation>();
                builder.TryAddType<Mutations.TravelRequirements.SaveContactTracingMutation>();
                builder.TryAddType<Mutations.TravelRequirements.SaveGovernmentFormMutation>();
                builder.TryAddType<Mutations.TravelRequirements.SaveSecondaryDocumentMutation>();
                builder.TryAddType<SecureFlightInfoMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<CheckInQuery>();
                builder.TryAddType<GenderQuery>();
                builder.TryAddType<NationalityQuery>();
                builder.TryAddType<StatesOfUSAQuery>();
                builder.TryAddType<PassportCountryQuery>();
                builder.TryAddType<GetBoardigPassQuery>();
                builder.TryAddType<BaggageQuery>();
                builder.TryAddType<TravelRequirementsQuery>();
                builder.TryAddType<SecureFlightInfoQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
