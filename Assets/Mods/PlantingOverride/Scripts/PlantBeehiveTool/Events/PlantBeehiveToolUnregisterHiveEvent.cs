using Timberborn.Pollination;

namespace Cordial.Mods.PlantBeehive.Scripts
{
    public class PlantBeehiveToolUnregisterHiveEvent
    {
        public Hive Hive { get; }

        public PlantBeehiveToolUnregisterHiveEvent(Hive hive)
        {
            this.Hive = hive;
        }

    }
}
