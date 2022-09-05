using Configurations.Models;
using Microsoft.Extensions.Configuration;

namespace Configurations
{
    public class ElasticApiConfiguration : IElasticApiConfiguration
    {
        private readonly IConfiguration _configuration;
        public ElasticApiConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ElasticApiConfigurationModel GetConfiguration()
        {
            return _configuration.GetSection("Elastic").Get<ElasticApiConfigurationModel>();
        }
    }
}
