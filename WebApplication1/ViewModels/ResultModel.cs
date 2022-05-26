namespace WebApplication1.ViewModels
{
    public class ResultModel
    {
        
        
        public string Value { get; set; }
        public string Id_test { get; set; }

        public List<Dictionary<string, string>> Answers { get; set; }

        public Dictionary<string, double> Percentage_category { get; set; }

        public double Percent { get; set; }

        public string recommendations { get; set; }
    }
}
