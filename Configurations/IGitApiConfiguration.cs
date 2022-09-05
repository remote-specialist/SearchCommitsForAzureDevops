using Configurations.Models;

namespace Configurations
{
    public interface IGitApiConfiguration
    {
        GitApiConfigurationModel GetConfiguration();
    }
}