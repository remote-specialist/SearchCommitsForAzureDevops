namespace ElasticApi.Models
{
    public class ElasticCommitModel
    {
        /// <summary>
        /// 'repositoryIdcommitId' is used for Id in Elastic
        /// </summary>
        public string Id { get; set; } = string.Empty;

        public string CommitId { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string RepositoryId { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public DateTime AuthorDate { get; set; }
        
        /// <summary>
        /// Year-Month-Day is used to allow easy search by date
        /// </summary>
        public string AuthorDateString { get; set; } = string.Empty;

        public string CommitterName { get; set; } = string.Empty;
        public string CommitterEmail { get; set; } = string.Empty;
        public DateTime CommitterDate { get; set; }
        
        /// <summary>
        /// Year-Month-Day is used to allow easy search by date
        /// </summary>
        public string CommitterDateString { get; set; } = string.Empty;

        /// <summary>
        /// Service property - will be used in Elastic - to allow easy search on all properties in simple query
        /// </summary>
        public string AllData { get; set; } = string.Empty;
    }
}
