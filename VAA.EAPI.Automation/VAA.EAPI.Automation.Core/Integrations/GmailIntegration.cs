using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Integrations
{
    public class GmailIntegration
    {
        private static readonly string[] Scopes = { GmailService.Scope.GmailReadonly }; // Only read access to Gmail
        private static readonly string ApplicationName = "Gmail API Integration";

        // Authenticate and create the Gmail service
        public static GmailService GetGmailService(string clientSecretsFileContents, out Exception exception)
        {
            exception = null;

            try
            {
                if (System.Buffers.Text.Base64.IsValid(clientSecretsFileContents))
                    clientSecretsFileContents = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(clientSecretsFileContents));

                FileInfo googleClientSecretsFileInfo = new FileInfo(@".\.credentials\googleClientSecrets.json");

                if (!googleClientSecretsFileInfo.Exists)
                {
                    Directory.CreateDirectory(googleClientSecretsFileInfo.DirectoryName);

                    File.WriteAllText(googleClientSecretsFileInfo.FullName, clientSecretsFileContents);
                }

                Google.Apis.Auth.OAuth2.ClientSecrets clientSecrets = GoogleClientSecrets.FromFile(googleClientSecretsFileInfo.FullName).Secrets;
                FileInfo credentialsFileInfo = new FileInfo(@".\.credentials\gmail-dotnet-quickstart.json");
                Google.Apis.Util.Store.FileDataStore fileDataStore = new FileDataStore(credentialsFileInfo.FullName, true);

                // Authenticate and authorize the user
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, Scopes, "user", CancellationToken.None, fileDataStore)
                                                                        .GetAwaiter()
                                                                        .GetResult();

                // Create the Gmail API service
                return new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception ex)
            {
                exception = ex;

                return null;
            }
        }

        // Retrieve the latest email
        public static string GetLatestEmailSnippet(GmailService service)
        {
            int count = 5;

        Start:

            //Sleeping to wait for the email to be in the account need to think of a better way
            System.Threading.Thread.Sleep(10000);

            // Retrieve the list of messages from the Gmail account (limit to 1 message)
            Google.Apis.Gmail.v1.UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
            request.MaxResults = 10; // Limit to the latest message
            Google.Apis.Gmail.v1.Data.ListMessagesResponse messages = request.Execute();

            if (messages.Messages == null || messages.Messages.Count == 0)
            {
                return "No new messages found.";
            }

            // Retrieve the most recent message
            string messageId = messages.Messages[0].Id;
            Google.Apis.Gmail.v1.Data.Message message = service.Users.Messages.Get("me", messageId).Execute();

            DateTime messageDate = GetHeader(message, "Date").Parse<DateTime>();

            if ((DateTime.Now - messageDate).TotalMinutes > 1 && count > 0)
            {
                count--;
                goto Start;
            }

            string snippet = message.Snippet;
            string code = Text.RegexCompiled.Match(snippet, "(?<=Your code is: )([0-9]+)").Value;

            return code;
        }

        // Helper method to extract specific header fields (like Subject, From)
        private static string GetHeader(Google.Apis.Gmail.v1.Data.Message message, string headerName)
        {
            Google.Apis.Gmail.v1.Data.MessagePartHeader header = message.Payload.Headers.FirstOrDefault(h => h.Name == headerName);
            return header != null ? header.Value : "Unknown";
        }
    }
}
