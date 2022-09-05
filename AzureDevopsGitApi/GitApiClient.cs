using System.Globalization;
using AzureDevopsGitApi.Extensions;
using AzureDevopsGitApi.Models;
using Configurations;
using Configurations.Extensions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Polly;

namespace AzureDevopsGitApi
{
    public class GitApiClient : IGitApiClient
    {
        // https://docs.microsoft.com/en-us/vsts/integrate/concepts/dotnet-client-libraries?view=vsts
        private readonly GitHttpClient _client;
        private readonly List<string> _branchNames;
        private readonly List<string> _ignoreComments;
        private readonly List<string> _ignoreCommitterEmails;
        private readonly int _commitsPerPage;
        private readonly int _tasksLimit;

        private readonly string _branchNotFoundMessagePrefix = "TF401175";

        private readonly AsyncPolicy _retryPolicy;

        public GitApiClient(IGitApiConfiguration configuration)
        {
            var configModel = configuration.GetConfiguration();

            var vstsUrl = $"{configModel.Url.TrimEnd('/')}/DefaultCollection";
            var credential = new VssBasicCredential(configModel.User, configModel.Token);
            var connection = new VssConnection(new Uri(vstsUrl), credential);
            
            _client = connection.GetClient<GitHttpClient>();
            
            _branchNames = configModel.BranchNames;
            _ignoreComments = configModel.IgnoreComments;
            _ignoreCommitterEmails = configModel.IgnoreCommitterEmails;
            _commitsPerPage = configModel.CommitsPerPage;
            _tasksLimit = configModel.TasksLimit;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                {
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                });
        }

        public async Task<List<CommitModel>> GetCommitsAsync(DateTime fromDate)
        {
            var commits = new List<CommitModel>();
            var repositories = await GetRepositoriesAsync();
            var queues = repositories.GetQueues(_tasksLimit);

            foreach (var queue in queues)
            {
                var tasks = new List<Task<List<CommitModel>>>();
                foreach (var repository in queue)
                {
                    tasks.Add(GetCommitsPerRepositoryAsync(repository, fromDate));
                }

                var results = await Task.WhenAll(tasks);
                foreach (var result in results)
                {
                    if (result.Count > 0)
                    {
                        commits.AddRange(result);
                    }
                }
            }

            return commits.Distinct().ToList();
        }

        public async Task<List<RepositoryModel>> GetRepositoriesAsync()
        {
            var repositoryModels = new List<RepositoryModel>();
            var repositories = await _client.GetRepositoriesAsync();
            foreach (var repository in repositories)
            {
                repositoryModels.Add(repository.GetModel());
            }

            return repositoryModels;
        }

        private async Task<List<CommitModel>> GetCommitsPerRepositoryAsync(RepositoryModel repository, DateTime fromDate)
        {
            var commitModels = new List<CommitModel>();

            var searches = new List<GitQueryCommitsCriteria>();
            foreach (var branchName in _branchNames)
            {
                searches.Add(new GitQueryCommitsCriteria
                {
                    FromDate = fromDate.ToString(CultureInfo.InvariantCulture),
                    ItemVersion = new GitVersionDescriptor
                    {
                        Version = branchName,
                        VersionType = GitVersionType.Branch
                    }
                });
            }

            foreach (var search in searches)
            {
                var searchCommits = await GetCommitsPerRepositoryBranchAsync(repository, search);
                if (searchCommits != null && searchCommits.Count > 0)
                {
                    commitModels.AddRange(searchCommits);
                }
            }

            return commitModels;
        }

        private async Task<List<CommitModel>> GetCommitsPerRepositoryBranchAsync(RepositoryModel repository, GitQueryCommitsCriteria search)
        {
            var allCommits = new List<CommitModel>();
            var count = 0;

            var stopSearch = false;
            while (!stopSearch)
            {
                var commits = await GetCommitsPerPageWithRetryAsync(repository, search, count);
                if (commits.Count > 0)
                {
                    foreach (var commit in commits)
                    {
                        // filter commits
                        var filtered = _ignoreComments.Any(filter => commit.Comment.StartsWith(filter, StringComparison.OrdinalIgnoreCase)) ||
                                       _ignoreCommitterEmails.Any(filter => commit.CommitterEmail.StartsWith(filter, StringComparison.OrdinalIgnoreCase));

                        // add commits
                        if (!filtered)
                        {
                            allCommits.Add(commit);
                        }
                    }
                }

                if (commits.Count < _commitsPerPage)
                {
                    stopSearch = true;
                }

                count += _commitsPerPage;
            }

            return allCommits;
        }

        private async Task<List<CommitModel>> GetCommitsPerPageWithRetryAsync(RepositoryModel repository, GitQueryCommitsCriteria search, int count)
        {
            return await _retryPolicy.ExecuteAsync(() => GetCommitsPerPageAsync(repository, search, count));
        }

        private async Task<List<CommitModel>> GetCommitsPerPageAsync(RepositoryModel repository, GitQueryCommitsCriteria search, int count)
        {
            try
            {
                var result = new List<CommitModel>();
                var commitRefs = await _client.GetCommitsAsync(repository.ProjectId, repository.RepositoryId, search, count, _commitsPerPage);
                foreach(var commitRef in commitRefs)
                {
                    result.Add(commitRef.GetModel(repository, search.ItemVersion.Version));
                }

                return result;
            }
            catch (VssServiceException ex)
            {
                if (ex.Message.StartsWith(_branchNotFoundMessagePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // Return empty if branch not found
                    return new List<CommitModel>();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
