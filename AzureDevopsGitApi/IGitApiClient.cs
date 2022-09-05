using AzureDevopsGitApi.Models;

namespace AzureDevopsGitApi
{
    public interface IGitApiClient
    {
        Task<List<RepositoryModel>> GetRepositoriesAsync();
        Task<List<CommitModel>> GetCommitsAsync(DateTime fromDate);
    }
}