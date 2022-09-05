namespace Configurations.Models
{
    public class ElasticApiConfigurationModel
    {
        public string Url { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IndexName { get; set; } = "git-commits-index";
        public int UpdateBatchSize { get; set; } = 200;
        public int SearchBatchSize { get; set; } = 100;
        public int TasksLimit { get; set; } = 32;
    }
}
