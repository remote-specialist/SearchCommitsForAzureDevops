using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ElasticApi.Models;
using ElasticApi;
using System;

namespace SearchCommits
{
    public class SearchFunction
    {
        private readonly IElasticApiClient _elasticClient;
        public SearchFunction(IElasticApiClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        [FunctionName("SearchFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] PostSearchRequestModel request,
            ILogger log)
        {
            log.LogInformation($"{DateTime.Now} Function {nameof(SearchFunction)} started.");
            var response = await _elasticClient.SearchCommitsAsync(request);
            log.LogInformation($"{DateTime.Now} Function {nameof(SearchFunction)} completed successfully.");

            return new OkObjectResult(response);
        }
    }
}
