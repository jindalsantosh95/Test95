using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestFunctionContext : FunctionContext
    {
        private readonly BindingContext bindingContext;
        private readonly TestFunctionDefinition functionDefinition = new();
        private readonly TestInvocationFeatures features = new();
        private IDictionary<object, object> items = new Dictionary<object, object>();
        private IServiceProvider serviceProvider;

        public TestFunctionContext() : this(new Dictionary<string, object>()) { }
        public TestFunctionContext(IReadOnlyDictionary<string, object?> bindingData)
        {
            bindingContext = new TestBindingContext(bindingData);

            features.Set<IHttpRequestDataFeature>(TestHttpRequestDataFeature.Instance);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public override string InvocationId => throw new NotImplementedException();

        public override string FunctionId => throw new NotImplementedException();

        public override TraceContext TraceContext => throw new NotImplementedException();

        public override BindingContext BindingContext => bindingContext;

        public override RetryContext RetryContext => throw new NotImplementedException();

        public override IServiceProvider InstanceServices
        {
            get => serviceProvider;
            set => serviceProvider = value;
        }

        public override FunctionDefinition FunctionDefinition => functionDefinition;

        public override IDictionary<object, object> Items
        {
            get => items;
            set => items = value;
        }

        public override IInvocationFeatures Features => features;
    }
}
