using AzureDevopsGitApi.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace AzureDevopsGitApi.Extensions
{
    public static class GitRepositoryExtensions
    {
        public static RepositoryModel GetModel(
            this GitRepository repository)
        {
            return new RepositoryModel
            {
                ProjectId = repository.ProjectReference.Id,
                ProjectName = repository.ProjectReference.Name,

                RepositoryId = repository.Id,
                RepositoryName = repository.Name
            };
        }
    }
}
