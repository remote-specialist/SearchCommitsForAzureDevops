using ElasticApi.Models;

namespace ElasticApi
{
    public interface IElasticApiClient
    {
        Task RebuildIndexAsync();
        Task UpdateSyncTimeAsync(DateTime syncTime);
        Task<DateTime?> GetSyncTimeAsync();
        Task AddDocumentsAsync(List<ElasticCommitModel> models);
        Task<List<ElasticCommitModel>> SearchCommitsAsync(PostSearchRequestModel request);
    }
}
