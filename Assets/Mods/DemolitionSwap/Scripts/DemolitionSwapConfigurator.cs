using Bindito.Core;

namespace Cordial.Mods.DemolitionSwap.Scripts
{
    [Context("Game")]
    public class DemolitionSwapConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<DemolitionSwapService>().AsSingleton();
        }
    }
}