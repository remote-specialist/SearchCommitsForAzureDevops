using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AzureDevopsGitApi;
using ElasticApi;
using ElasticApi.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SearchCommits.Extensions;

namespace SearchCommits
{
    public class UpdateIndexFunction
    {
        private readonly IGitApiClient _gitClient;
        private readonly IElasticApiClient _elasticClient;

        public UpdateIndexFunction(IGitApiClient gitClient, IElasticApiClient elasticClient)
        {
            _gitClient = gitClient;
            _elasticClient = elasticClient;
        }

        [FunctionName("UpdateIndexFunction")]
        public async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"{DateTime.Now} Function {nameof(UpdateIndexFunction)} started.");
            
            // Skip updates for weekend and nights
            if (!IsWorkingTime(DateTime.Now))
            {
                log.LogInformation($"Give me some rest!");
                return;
            }

            // Check if previous sync was run
            var start = new DateTime(1980, 5, 7);
            var currentSyncTime = DateTime.Now;
            var lastSyncTime = await _elasticClient.GetSyncTimeAsync();

            if(lastSyncTime != null)
            {
                log.LogInformation($"{DateTime.Now} Last successful sync found. LastSyncTime: {lastSyncTime}.");
                start = (DateTime) lastSyncTime;
            }
            else
            {
                log.LogInformation($"{DateTime.Now} Last successful sync not found. Rebuild index is triggered.");
                await _elasticClient.RebuildIndexAsync();
            }
            
            // Get commits from Git
            log.LogInformation($"{DateTime.Now} Search commits from {start} started.");

            var gitCommits = await _gitClient.GetCommitsAsync(start);

            // Prepare models for Elastic
            var elasticCommits = new List<ElasticCommitModel>();
            foreach (var gitCommit in gitCommits)
            {
                elasticCommits.Add(gitCommit.GetElasticModel());
            }

            log.LogInformation($"{DateTime.Now} Search commits from {start} completed. CommitsCount: {elasticCommits.Count}");

            // Add commit models to Elastic
            log.LogInformation($"{DateTime.Now} Add commits to Elastic index started.");
            
            await _elasticClient.AddDocumentsAsync(elasticCommits);
            log.LogInformation($"{DateTime.Now} Add commits to Elastic index completed.");

            // Everything is ok - all commits were synced to Elastic. Let's update sync time
            log.LogInformation($"{DateTime.Now} Update sync time started.");
            await _elasticClient.UpdateSyncTimeAsync(currentSyncTime);
            log.LogInformation($"{DateTime.Now} Update sync time completed.");

            // It's done!
            log.LogInformation($"{DateTime.Now} Function {nameof(SearchFunction)} completed.");
        }

        private static bool IsWorkingTime(DateTime d)
        {
            var dateTime = d.ToUniversalTime() + TimeSpan.FromHours(3.0);
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return false;
                default:
                    return dateTime.Hour >= 7 && dateTime.Hour <= 23;
            }
        }
    }
}
