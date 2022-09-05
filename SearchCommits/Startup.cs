using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System;
using Configurations;
using AzureDevopsGitApi;
using ElasticApi;

[assembly: FunctionsStartup(typeof(SearchCommits.Startup))]
namespace SearchCommits
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Dependency Injection: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
            var serviceProvider = builder.Services.BuildServiceProvider();

            builder
                .Services
                .AddSingleton((s) =>
                {
                    return serviceProvider.GetRequiredService<IConfiguration>();
                })
                .AddSingleton<IElasticApiConfiguration, ElasticApiConfiguration>()
                .AddSingleton<IGitApiConfiguration, GitApiConfiguration>()
                .AddSingleton<IGitApiClient, GitApiClient>()
                .AddSingleton<IElasticApiClient, ElasticApiClient>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
    }
}
