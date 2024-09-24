using Bindito.Core;

namespace Cordial.Mods.ForesterUpdate.Scripts
{
    [Context("Game")]
    public class CutterToolConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ForesterUpdateStateService>().AsSingleton();
        }


    }
}