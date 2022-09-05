using Configurations.Models;

namespace Configurations
{
    public interface IElasticApiConfiguration
    {
        ElasticApiConfigurationModel GetConfiguration();
    }
}
