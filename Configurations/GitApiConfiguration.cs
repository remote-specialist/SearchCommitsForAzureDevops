using Configurations.Models;
using Microsoft.Extensions.Configuration;

namespace Configurations
{
    public class GitApiConfiguration : IGitApiConfiguration
    {
        private readonly IConfiguration _configuration;
        public GitApiConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public GitApiConfigurationModel GetConfiguration()
        {
            return _configuration.GetSection("Vsts").Get<GitApiConfigurationModel>();
        }
    }
}
