namespace AzureDevopsGitApi.Models
{
    public class RepositoryModel
    {
        public string ProjectName { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string RepositoryName { get; set; } = string.Empty;
        public Guid RepositoryId { get; set; }
    }
}
