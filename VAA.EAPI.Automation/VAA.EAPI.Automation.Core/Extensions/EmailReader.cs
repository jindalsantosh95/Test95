using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class EmailReader
    {
        public static string emailTenantId = Vault.Default.GetSecretValue("MarketingLoyalty-EmailTenantId");

        public static string emailClientId = Vault.Default.GetSecretValue("MarketingLoyalty-EmailClientId");

        public static string emailClientSecret = Vault.Default.GetSecretValue("MarketingLoyalty-EmailClientSecret");

        public static string ReturnEmailBodyFromInbox(string subjectLine, string emailAddress)
        {
            ClientSecretCredential credentials = new ClientSecretCredential(
                emailTenantId,
                emailClientId,
                emailClientSecret,
                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

            GraphServiceClient graphServiceClient = new GraphServiceClient(credentials);

            // Get the latest 20 messages from the inbox
            MessageCollectionResponse emailMessages = graphServiceClient.Users[emailAddress].MailFolders["inbox"].Messages.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "subject", "body", "bodyPreview", "uniqueBody", "from", "receivedDateTime" };
                requestConfiguration.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                requestConfiguration.QueryParameters.Top = 20;
            }).GetAwaiter().GetResult();

            // Find the first message that contains the subject line
            Message matchingMessage = emailMessages.Value
                .FirstOrDefault(msg => msg.Subject != null && msg.Subject.Contains(subjectLine, StringComparison.OrdinalIgnoreCase));

            if (matchingMessage != null)
            {
                return matchingMessage.BodyPreview;
            }

            return string.Empty;
        }

        public static async Task DeleteEmailsBySubjectAsync(string subjectLine, string emailAddress)
        {
            ClientSecretCredential credentials = new ClientSecretCredential(
                emailTenantId,
                emailClientId,
                emailClientSecret,
                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

            GraphServiceClient graphServiceClient = new GraphServiceClient(credentials);

            // Get a batch of recent emails (e.g., top 20)
            MessageCollectionResponse messages = await graphServiceClient.Users[emailAddress].MailFolders["inbox"].Messages.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = new[] { "subject", "body", "bodyPreview", "uniqueBody", "from", "receivedDateTime" };
                requestConfiguration.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                requestConfiguration.QueryParameters.Top = 20;
            });

            // Filter messages that contain the subject line
            List<Message> matchingMessages = messages.Value
                .Where(msg => msg.Subject != null && msg.Subject.Contains(subjectLine, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Delete each matching message
            foreach (Message message in matchingMessages)
            {
                await graphServiceClient.Users[emailAddress].Messages[message.Id].DeleteAsync();
            }
        }
    }
}
