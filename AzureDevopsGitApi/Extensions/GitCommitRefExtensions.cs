using AzureDevopsGitApi.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzureDevopsGitApi.Extensions
{
    public static class GitCommitRefExtensions
    {
        public static CommitModel GetModel(
            this GitCommitRef commit,
            RepositoryModel repositoryModel,
            string branchName)
        {
            return new CommitModel
            {
                Id = commit.CommitId,
                BranchName = branchName,
                Comment = commit.Comment,

                ProjectName = repositoryModel.ProjectName,
                RepositoryId = $"{repositoryModel.RepositoryId}",
                RepositoryName = repositoryModel.RepositoryName,

                Url = commit.RemoteUrl,

                AuthorName = commit.Author.Name,
                AuthorEmail = commit.Author.Email,
                AuthorDate = commit.Author.Date.ToUniversalTime(),

                CommitterName = commit.Committer.Name,
                CommitterEmail = commit.Committer.Email,
                CommitterDate = commit.Committer.Date.ToUniversalTime()
            };
        }
    }
}
