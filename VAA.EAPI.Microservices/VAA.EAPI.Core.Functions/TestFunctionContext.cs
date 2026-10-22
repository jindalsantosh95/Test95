using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestFunctionContext : FunctionContext
    {
        private readonly BindingContext bindingContext;
        private readonly TestFunctionDefinition functionDefinition = new();
        private readonly TestInvocationFeatures features = new();
        private readonly IDictionary<object, object> items = new Dictionary<object, object>();
        private IServiceProvider serviceProvider;

        public TestFunctionContext() : this(new Dictionary<string, object?>(), new TestFunctionDefinition(), null) { }
        public TestFunctionContext(Dictionary<string, object?> bindingData) : this(bindingData, new TestFunctionDefinition()) { }
        public TestFunctionContext(IReadOnlyDictionary<string, object?> bindingData, TestFunctionDefinition functionDefinition) : this(bindingData, functionDefinition, null) { }
        public TestFunctionContext(IReadOnlyDictionary<string, object?> bindingData, TestFunctionDefinition functionDefinition, IServiceProvider serviceProvider)
        {
            items = new Dictionary<object, object>();

            bindingData = bindingData ?? new Dictionary<string, object>
            {
                { "Headers", Json.Serialize( new Dictionary<string, string>(){{ "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" }, { "X-Forwarded-For" , "85.115.54.201,64.252.85.104,10.10.217.89,10.10.219.11" } } ) }
            }!;

            bindingContext = new TestBindingContext(bindingData ?? new Dictionary<string, object>());

            features.Set<IHttpRequestDataFeature>(TestHttpRequestDataFeature.Instance);

            if (serviceProvider is null)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();

                this.serviceProvider = serviceCollection.BuildServiceProvider();
            }

            // set-up default http request data
            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(this, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" }, { "X-Forwarded-For", "85.115.54.201,64.252.85.104,10.10.217.89,10.10.219.11" } });

            features.Set<HttpRequestData>(httpRequestData);
            features.Set<IFunctionBindingsFeature>(new TestFunctionBindingsFeature());
        }

        public override string InvocationId => Guid.NewGuid().ToString();

        public override string FunctionId => "TestFunction";

        public override TraceContext TraceContext => new TestTraceContext();

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
            set
            {
                items.Clear();

                if (value is not null)
                    foreach (var item in value)
                        items.Add(item);
            }
        }

        public override IInvocationFeatures Features => features;
    }
}
