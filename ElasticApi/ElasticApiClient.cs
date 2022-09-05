using Configurations;
using Configurations.Extensions;
using ElasticApi.Extensions;
using ElasticApi.Models;
using Nest;
using Polly;
using Policy = Polly.Policy;

namespace ElasticApi
{
    public class ElasticApiClient : IElasticApiClient
    {
        private readonly ElasticClient _api;
        private readonly string _indexName;
        private readonly int _updateBatchSize;
        private readonly int _searchBatchSize;
        private readonly int _tasksLimit;
        
        /// <summary>
        /// This value will be used in Elastic to store the sync time
        /// </summary>
        private readonly string _syncId = "00000000000000000000000000000001";

        private readonly AsyncPolicy _retryPolicy;

        public ElasticApiClient(IElasticApiConfiguration configuration)
        {
            var configModel = configuration.GetConfiguration();

            var connectionSettings = new ConnectionSettings(new Uri(configModel.Url));
            connectionSettings.BasicAuthentication(
                configModel.User,
                configModel.Password);

            _api = new ElasticClient(connectionSettings);

            _indexName = configModel.IndexName;
            _updateBatchSize = configModel.UpdateBatchSize;
            _searchBatchSize = configModel.SearchBatchSize;
            _tasksLimit = configModel.TasksLimit;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => 
                {
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)); 
                });
        }

        /// <summary>
        /// Sync time is saved in Elastic itself - see _syncId 
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime?> GetSyncTimeAsync()
        {
            var result = new List<ElasticCommitModel>();

            var response = await _api.SearchAsync<ElasticCommitModel>(
                                         s => s
                                             .Index(new[] { _indexName })
                                             .Query(q => q.Ids(s => s.Values(_syncId)))
                                             .From(0)
                                             .Size(1)
                                             .Sort(GetSortFunc()));

            if (response != null && response.Documents != null && response.Documents.Count > 0)
            {
                return response.Documents.ToList()[0].CommitterDate;
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateSyncTimeAsync(DateTime syncTime)
        {
            await AddDocumentsAsync(new List<ElasticCommitModel>
            {
                new ElasticCommitModel
                {
                    Id = _syncId,
                    CommitterDate = syncTime
                }
            });
        }

        public async Task RebuildIndexAsync()
        {
            await _api.Indices.DeleteAsync(_indexName);
            await _api.Indices.CreateAsync(_indexName, i => i
                .Settings(s => s
                    .Analysis(a => a
                        .Normalizers(n => n
                            .Custom("sort_normalizer", cu => cu
                                .Filters("lowercase", "asciifolding")))))
                .Map<ElasticCommitModel>(m => m
                    .Dynamic(false)
                    .Properties(p =>
                    {
                        p = p
                            .Keyword(x => x.Name(pr => pr.CommitId).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.BranchName).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.Url).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))

                            .Keyword(x => x.Name(pr => pr.AuthorEmail).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.AuthorName).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.AuthorDateString).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))

                            .Keyword(x => x.Name(pr => pr.CommitterEmail).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.CommitterName).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.CommitterDateString).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))

                            .Keyword(x => x.Name(pr => pr.ProjectName).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.RepositoryId).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Keyword(x => x.Name(pr => pr.RepositoryName).CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))

                            .Date(x => x.Name(pr => pr.AuthorDate))
                            .Date(x => x.Name(pr => pr.CommitterDate))

                            .Text(x => x
                                .Name(pr => pr.Comment)
                                .Analyzer("standard").CopyTo(c => c.Field(nameof(ElasticCommitModel.AllData).ToCamelCase())))
                            .Text(x => x
                                .Name(pr => pr.AllData)
                                .Analyzer("standard"));

                        return p;
                    })
                )
            );
        }

        public async Task<List<ElasticCommitModel>> SearchCommitsAsync(PostSearchRequestModel request)
        {
            var from = (request == null || request.From < 0)
                    ? 0
                    : request.From;
            var size = (request == null || request.Size <= 0)
                ? _searchBatchSize
                : request.Size;
            var searchQuery = (request == null || string.IsNullOrEmpty(request.SearchQuery))
                ? string.Empty
                : request.SearchQuery;

            var result = new List<ElasticCommitModel>();

            ISearchResponse<ElasticCommitModel> response;
            if(string.IsNullOrEmpty(searchQuery))
            {
                response = await _api.SearchAsync<ElasticCommitModel>(
                                         s => s
                                             .Index(new[] { _indexName })
                                             .From(from)
                                             .Size(size)
                                             .Sort(GetSortFunc()));
            }
            else
            {
                response = await _api.SearchAsync<ElasticCommitModel>(
                                         s => s
                                             .Index(new[] { _indexName })
                                             .From(from)
                                             .Size(size)
                                             .Query(q => q.QueryString(s => s.Query(searchQuery).DefaultOperator(Operator.And)))
                                             .Sort(GetSortFunc()));
            }

            if (response != null && response.Documents != null && response.Documents.Count > 0)
            {
                result = response.Documents.Where(i => i.Id != _syncId).ToList();
            }

            return result;
        }

        /// <summary>
        /// Documents will be added in Batches
        /// _updateBatchSize sets the limit for batch size 
        /// _tasksLimit sets the limit for simultaneous operations
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task AddDocumentsAsync(List<ElasticCommitModel> models)
        {
            // prepare batches
            var commitQueues = models.GetQueues(_updateBatchSize);

            // prepare tasks queues for update
            var tasksQueues = commitQueues.GetQueues(_tasksLimit);

            foreach (var taskQueue in tasksQueues)
            {
                var tasks = new List<Task>();
                foreach (var queue in taskQueue)
                {
                    tasks.Add(IndexDocumentsBatchWithRetriesAsync(queue));
                }

                await Task.WhenAll(tasks);
            }
        }

        private async Task IndexDocumentsBatchWithRetriesAsync(List<ElasticCommitModel> models)
        {
            await _retryPolicy.ExecuteAsync(() => IndexDocumentsBatchAsync(models));
        }

        private async Task IndexDocumentsBatchAsync(List<ElasticCommitModel> models)
        {
            var response = await _api.IndexManyAsync(models, _indexName);
            
            // Response must be checked to be sure that indexing was completed fine
            if(response.Errors)
            {
                if(response.OriginalException != null)
                {
                    
                    throw response.OriginalException;
                }
                else
                {
                    var message = $"{DateTime.Now} Cannot update documents in Elastic!";
                    if(response.ServerError != null && response.ServerError.Error != null)
                    {
                        message += $" ServerError: {response.ServerError.Error}";
                    }
                    
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Commits will be ordered by CommiterDate descending 
        /// (The most newest commits will be displayed in the top)
        /// </summary>
        /// <returns></returns>
        private Func<SortDescriptor<ElasticCommitModel>, IPromise<IList<ISort>>> GetSortFunc()
        {
            return q 
                => 
            q.Field(new Field(nameof(ElasticCommitModel.CommitterDate).ToCamelCase()), 
            SortOrder.Descending);
        }
    }
}