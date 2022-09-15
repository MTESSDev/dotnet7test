using System;
using System.Threading.Tasks;

namespace FRW.TR.Commun.Services
{
    public class ConfigurationApplicationMock : ConfigurationApplicationBase
    {

        public ConfigurationApplicationMock()
        {
        }

        public override Task Initialiser()
        {
            EstInitialise = true;
            return Task.CompletedTask;
        }

        public override Task ObtenirConfigurationApplication()
        {
            throw new NotImplementedException();
        }
    }
}
