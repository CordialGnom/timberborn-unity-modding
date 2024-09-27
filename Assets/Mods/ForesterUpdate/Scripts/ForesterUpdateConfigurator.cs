using Bindito.Core;
using Cordial.Mods.ForesterUpdate.Scripts.UI;
using Timberborn.EntityPanelSystem;
using Timberborn.Planting;

namespace Cordial.Mods.ForesterUpdate.Scripts
{
    [Context("Game")]
    public class ForesterUpdateConfigurator : IConfigurator {

        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ForesterUpdateStateService>().AsSingleton();
            containerDefinition.Bind<ForesterUpdateFragment>().AsSingleton();
            containerDefinition.Bind<PanelFragment>().AsSingleton();
            containerDefinition.Bind<ForesterUpdateTreeDropDownProvider>().AsSingleton();
            containerDefinition.Bind<ForesterUpdatePrefabSpecService>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>().ToProvider<ForesterUpdateConfigurator.EntityPanelModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            readonly ForesterUpdateFragment _foresterUpdateFragment;

            public EntityPanelModuleProvider(ForesterUpdateFragment foresterUpdateFragment)
            {
                _foresterUpdateFragment = foresterUpdateFragment;
            }

            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddBottomFragment(_foresterUpdateFragment);
                return builder.Build();
            }
        }

    }
}