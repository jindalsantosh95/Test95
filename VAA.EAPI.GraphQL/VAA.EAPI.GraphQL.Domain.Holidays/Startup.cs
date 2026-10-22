using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using VAA.EAPI.GraphQL.Domain.Holidays.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Holidays
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
            services.AddSingleton<StoreServiceProvider>();
            services.AddSingleton<IStoreLocatorServiceProvider>(provider => provider.GetRequiredService<StoreServiceProvider>());
            services.AddSingleton<IAppointmentBookingServiceProvider>(provider => provider.GetRequiredService<StoreServiceProvider>());
            services.AddSingleton<IStoreManagerServiceProvider>(provider => provider.GetRequiredService<StoreServiceProvider>());
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                RegisterStoreManagerMutations(builder);
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<SaveAppointmentBookingMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                RegisterStoreManagerQueries(builder);
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<NearbyStoresQuery>();
                builder.TryAddType<StoreDetailsQuery>();
                builder.TryAddType<SlotAvailabilityQuery>();
                builder.TryAddType<StoreCalendarAvailabilityQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }

        private static void RegisterStoreManagerMutations(IRequestExecutorBuilder builder)
        {
            builder.TryAddType<BookingCancellationReasonType>();
            builder.TryAddType<CreateClosureMutation>();
            builder.TryAddType<CancelBookingMutation>();
            builder.TryAddType<DeleteClosureMutation>();
            builder.TryAddType<ReplaceOpeningHoursMutation>();
            builder.TryAddType<ReplaceSlotRulesMutation>();
            builder.TryAddType<UpdateAdminStoreMutation>();
            builder.TryAddType<UpdateSlotAllocationStatusMutation>();
            builder.TryAddType<UpdateSlotStatusMutation>();
            builder.TryAddType<UpdateStoreMutation>();
        }

        private static void RegisterStoreManagerQueries(IRequestExecutorBuilder builder)
        {
            builder.TryAddType<StoreManagerBookingStatusType>();
            builder.TryAddType<GetAdminStoreByIdStoreManagerQuery>();
            builder.TryAddType<GetAdminStoresStoreManagerQuery>();
            builder.TryAddType<GetAvailabilityStoreManagerQuery>();
            builder.TryAddType<GetAvailabilityRangeStoreManagerQuery>();
            builder.TryAddType<GetClosuresStoreManagerQuery>();
            builder.TryAddType<GetMeStoreManagerQuery>();
            builder.TryAddType<GetBookingsStoreManagerQuery>();
            builder.TryAddType<GetMyBookingsStoreManagerQuery>();
            builder.TryAddType<GetOpeningHoursStoreManagerQuery>();
            builder.TryAddType<GetSlotRulesStoreManagerQuery>();
            builder.TryAddType<GetSlotAllocationsStoreManagerQuery>();
            builder.TryAddType<GetSlotsStoreManagerQuery>();
            builder.TryAddType<GetStoreByIdStoreManagerQuery>();
        }
    }
}
