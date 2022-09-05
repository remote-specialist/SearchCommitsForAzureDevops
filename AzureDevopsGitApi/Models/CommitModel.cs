namespace AzureDevopsGitApi.Models
{
    public class CommitModel
    {
        public string Id { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;
        public string RepositoryId { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public DateTime AuthorDate { get; set; }

        public string CommitterName { get; set; } = string.Empty;
        public string CommitterEmail { get; set; } = string.Empty;
        public DateTime CommitterDate { get; set; }
    }
}
