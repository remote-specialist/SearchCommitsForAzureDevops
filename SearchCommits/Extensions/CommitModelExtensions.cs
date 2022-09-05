using AzureDevopsGitApi.Models;
using ElasticApi.Models;

namespace SearchCommits.Extensions
{
    public static class CommitModelExtensions
    {
        public static ElasticCommitModel GetElasticModel(this CommitModel gitCommit)
        {
            return new ElasticCommitModel
            {
                Id = $"{gitCommit.Id}{gitCommit.RepositoryId}",
                CommitId = gitCommit.Id,
                BranchName = gitCommit.BranchName,
                Comment = gitCommit.Comment,
                ProjectName = gitCommit.ProjectName,
                RepositoryId = gitCommit.RepositoryId,
                RepositoryName = gitCommit.RepositoryName,
                Url = gitCommit.Url,
                AuthorName = gitCommit.AuthorName,
                AuthorEmail = gitCommit.AuthorEmail,
                AuthorDate = gitCommit.AuthorDate,
                AuthorDateString = $"{gitCommit.AuthorDate.Year}-{gitCommit.AuthorDate.Month}-{gitCommit.AuthorDate.Day}",
                CommitterName = gitCommit.CommitterName,
                CommitterEmail = gitCommit.CommitterEmail,
                CommitterDate = gitCommit.CommitterDate,
                CommitterDateString = $"{gitCommit.CommitterDate.Year}-{gitCommit.CommitterDate.Month}-{gitCommit.CommitterDate.Day}"
            };
        }
    }
}
