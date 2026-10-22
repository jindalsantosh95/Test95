namespace VAA.EAPI.HealthStatusApp.Models.MetricsModels
{
    public class LatestHeimdallResult
    {
        private DateTime _timeStamp;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                ParseMessage(value);
            }
        }
        private string _message;

        public int NumberofTests { get; set; }
        public int NumberSuccesful { get; set; }
        public int NumberFailed { get; set; }
        public int NumberSkipped { get; set; }

        public DateTime TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(value, TimeZoneInfo.Local.Id, "GMT Standard Time");
            }
        }

        private void ParseMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var match = System.Text.RegularExpressions.Regex.Match(
                message,
                @"Heimdall Finished: (\d+) tests\((\d+) passed, (\d+) failed, (\d+) skipped\)");

            if (match.Success)
            {
                NumberofTests = int.Parse(match.Groups[1].Value);
                NumberSuccesful = int.Parse(match.Groups[2].Value);
                NumberFailed = int.Parse(match.Groups[3].Value);
                NumberSkipped = int.Parse(match.Groups[4].Value);
            }
        }
    }
}
