using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Xunit.Contexts
{
    internal enum AttachmentType
    {
        GeneralAttachment = 0,
        AfnStrip,
        BugFilingData,
        CodeCoverage,
        IntermediateCollectorData,
        RunConfig,
        TestImpactDetails,
        TmiTestRunDeploymentFiles,
        TmiTestRunReverseDeploymentFiles,
        TmiTestResultDetail,
        TmiTestRunSummary
    };

    internal sealed class AzureDevOpsContext : ITestContext
    {
        private readonly bool canAccess = true;

        public static ITestContext Current { get; private set; } = new AzureDevOpsContext();


        public bool CanAccess
        {
            get
            {
                if (!canAccess)
                    return false;
                if (addAttachmentDevOpsUrl.IsNullOrWhiteSpace())
                    return false;

                return true;
            }
        }
        public dynamic TestRun { get; private set; }

        private readonly Random random = new();
        private readonly string organisation;
        private readonly string project;
        private readonly string apiVersion;
        private readonly string testResultDirectory;
        private readonly string authorizationHeader;
        private readonly string addAttachmentDevOpsUrl;
        private readonly string updateTestRunDevOpsUrl;


        private AzureDevOpsContext()
        {
            organisation = RegexCompiled.Replace(new Uri(ConfigReader.Get("DTA.TeamFoundationCollectionUri")).Host, ".visualstudio.com$", string.Empty);
            project = System.Uri.EscapeDataString(ConfigReader.Get("DTA.ProjectName", string.Empty));
            apiVersion = ConfigReader.Get("Automation:DevOps:ApiVersion", "7.0");
            testResultDirectory = ConfigReader.Get("DTA.TestResultDirectory", ".");

            if (!testResultDirectory.Is("."))
                Directory.CreateDirectory(testResultDirectory);

            string devOpsAcessToken = ConfigReader.Get("SYSTEM_ACCESSTOKEN");
            string owner = ConfigReader.Get("DTA.Owner", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            string tiaContext = ConfigReader.Get("DTA.TiaContext", string.Empty);
            string releaseId = ConfigReader.Get("DTA.ReleaseId", string.Empty);
            string releaseUri = ConfigReader.Get("DTA.ReleaseUri", string.Empty);
            string buildId = ConfigReader.Get("DTA.BuildId", "0");
            string runName = ConfigReader.Get("DTA.Runname", owner);
            string testPlanId = ConfigReader.Get("TEST_PLANID", string.Empty);

            string definitionRunId = string.Empty;
            string definitionId = string.Empty;

            if (tiaContext.Is("CD"))
            {
                // Release context. Passing Release Id.
                definitionRunId = releaseId;
                definitionId = ConfigReader.Get("RELEASE_DEFINITIONID");
            }
            else
            {
                // Build context. Passing build id.
                definitionRunId = buildId;
                definitionId = ConfigReader.Get("SYSTEM_DEFINITIONID");
            }

            canAccess &= !devOpsAcessToken.IsNullOrWhiteSpace();
            canAccess &= !organisation.IsNullOrEmpty();
            canAccess &= !project.IsNullOrEmpty();
            canAccess &= !definitionRunId.IsNullOrEmpty();
            canAccess &= !testPlanId.IsNullOrEmpty();

            if (canAccess)
            {
                authorizationHeader = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($":{devOpsAcessToken}"))}";

                string createTestRunDevOpsUrl = $"https://dev.azure.com/{organisation}/{project}/_apis/test/runs?api-version={apiVersion}";
                NameValueCollection headers = new NameValueCollection() { { "Authorization", authorizationHeader } };
                object request = new
                {
                    build = new
                    {
                        id = buildId,
                    },
                    name = runName,
                    automated = true,
                    buildFlavor = "release",
                    plan = new
                    {
                        id = testPlanId
                    },
                    state = "InProgress",
                    startedDate = DateTime.Now.ToString("yyyy-MM-dd")
                };

                System.Net.Http.HttpResponseMessage response = WebRequest.Json.Post(createTestRunDevOpsUrl, JsonConvert.SerializeObject(request), headers);

                if (response.IsSuccessStatusCode)
                {
                    TestRun = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    addAttachmentDevOpsUrl = $"https://dev.azure.com/{organisation}/{project}/_apis/test/runs/{TestRun.id}/attachments?api-version={apiVersion}";
                    updateTestRunDevOpsUrl = $"https://dev.azure.com/{organisation}/{project}/_apis/test/runs/{TestRun.Id}?api-version={apiVersion}";
                }
            }
        }

        public bool Attach(string filePath) => Attach(filePath, out _);
        public bool Attach(string filePath, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            string fileName = Path.GetFileName(filePath);

            using Stream stream = File.OpenRead(filePath);

            return Attach(stream, fileName, out exception);
        }

        public bool Attach(StringBuilder stringBuilder, string fileName) => Attach(stringBuilder, fileName, out _);
        public bool Attach(StringBuilder stringBuilder, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            using MemoryStream memoryStream = new();
            using StreamWriter streamWriter = new(memoryStream);

            streamWriter.Write(stringBuilder.ToString());

            streamWriter.Flush();

            memoryStream.Position = 0;

            return Attach(memoryStream, fileName, out exception);
        }

        public bool Attach(ReadOnlySpan<byte> bytes, string fileName) => Attach(bytes, fileName, out _);
        public bool Attach(ReadOnlySpan<byte> bytes, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            using MemoryStream memoryStream = new();

            memoryStream.Write(bytes);

            memoryStream.Position = 0;

            return Attach(memoryStream, fileName, out exception);
        }

        public bool Attach(Stream stream, string fileName) => Attach(stream, fileName, out _);
        public bool Attach(Stream stream, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            using MemoryStream attachmentBuffer = new();
            stream.CopyTo(attachmentBuffer);
            byte[] buffer = attachmentBuffer.ToArray();

            object request = new
            {
                stream = Convert.ToBase64String(buffer),
                fileName,
                comment = $"Test attachment '{Path.GetFileName(fileName)}' upload.",
                attachmentType = AttachmentType.GeneralAttachment.ToString()
            };

            bool attached = false;

            try
            {
                NameValueCollection headers = new NameValueCollection() { { "Authorization", authorizationHeader } };

                System.Net.Http.HttpResponseMessage response = WebRequest.Json.Post(addAttachmentDevOpsUrl, JsonConvert.SerializeObject(request), headers, null).EnsureSuccessStatusCode();

                attached = response.IsSuccessStatusCode;

                if (!attached)
                    throw new HttpRequestException(response.ReadResponseAsString(), null, response.StatusCode);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (!testResultDirectory.Is("."))
                    Directory.CreateDirectory(testResultDirectory);

                File.WriteAllBytes(Path.Combine(testResultDirectory, fileName), buffer);
            }

            return attached;
        }

        public void Complete() => Complete(out _);
        public void Complete(out Exception exception)
        {
            exception = default;

            try
            {
                string resultFile = Directory.GetFiles(testResultDirectory, "*.trx").FirstOrDefault();

                if ((!resultFile.IsNullOrWhiteSpace()))
                    Attach(resultFile);

                NameValueCollection headers = new NameValueCollection() { { "Authorization", authorizationHeader } };
                object request = new
                {
                    state = "Completed"
                };

                System.Net.Http.HttpResponseMessage response = WebRequest.Json.Patch(updateTestRunDevOpsUrl, JsonConvert.SerializeObject(request), headers).EnsureSuccessStatusCode();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(response.ReadResponseAsString(), null, response.StatusCode);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }
    }
}
