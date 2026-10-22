namespace VAA.EAPI.HealthStatusApp.Models.ViewModels
{
    public class PieScoresModel
    {
        public int TotalScore { get { return FirstScore +  SecondScore; } }
        public int FirstScore { get; set; }
        public int SecondScore { get; set; }
        public int ThirdScore   { get; set; }
        public string PiePercentage 
        {
            get 
            { 
                if (FirstScore  + SecondScore == 0) return "0"; 
                return ((FirstScore * 100) / (FirstScore +  SecondScore)).ToString(); 
            } 
        }
    }
}
