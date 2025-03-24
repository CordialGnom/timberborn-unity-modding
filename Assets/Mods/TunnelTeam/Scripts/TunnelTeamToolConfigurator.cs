using Bindito.Core;
using TimberApi.Tools.ToolSystem;

namespace Cordial.Mods.TunnelTeam.Scripts
{
    [Context("Game")]
    public class TunnelTeamToolConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ITunnelTeamTool>().To<TunnelTeamToolService>().AsSingleton();
            containerDefinition.Bind<TunnelTeamToolService>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<TunnelTeamToolFactory>().AsSingleton();
        }


    }
}