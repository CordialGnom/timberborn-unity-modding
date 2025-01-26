using Timberborn.Pollination;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolRegisterHiveEvent
    {
        public Hive Hive { get; }

        public PlantBeehiveToolRegisterHiveEvent(Hive hive)
        {
            this.Hive = hive;
        }

    }
}
