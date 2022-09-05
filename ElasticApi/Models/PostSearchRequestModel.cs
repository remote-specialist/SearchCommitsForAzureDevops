namespace ElasticApi.Models
{
    public class PostSearchRequestModel
    {
        public string SearchQuery { get; set; } = string.Empty;
        public int From { get; set; } = 0;
        public int Size { get; set; } = 100;
    }
}
