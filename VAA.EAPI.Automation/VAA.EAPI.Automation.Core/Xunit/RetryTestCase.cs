namespace VAA.EAPI.Automation.Core.Xunit
{
    public static class RetryHelper
    {
        public static void RunWithRetry(Action action, int maxRetries, int delay = 500)
        {
            int attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    action();  // Run the test logic
                    return; // Test passed, exit the loop
                }
                catch (Exception ex)
                {
                    Thread.Sleep(delay);
                    if (attempts >= maxRetries)
                        throw; // Max retries reached, rethrow the exception
                }
            }
        }

        public static async Task RunWithRetryAsync(Func<Task> action, int maxRetries, int delay = 500)
        {
            int attempts = 0;

            while (true)
            {
                try
                {
                    attempts++;
                    await action();  // Run the test logic
                    return; // Test passed, exit the loop
                }
                catch (Exception ex)
                {
                    await Task.Delay(delay);
                    if (attempts >= maxRetries)
                        throw; // Max retries reached, rethrow the exception
                }
            }
        }
    }
}