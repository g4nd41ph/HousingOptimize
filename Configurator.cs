using Bindito.Core;
using System.Diagnostics.Tracing;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace HousingOptimize
{
    [Configurator(SceneEntrypoint.InGame)]
    public class Configurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<EventListener>().AsSingleton();
        }
    }
}
