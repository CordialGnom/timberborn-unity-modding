
namespace Cordial.Mods.BoosterJuice.Scripts.Events
{
    public class GrowthFertilizationConfigChangeEvent
    {
        public bool FertilizeYield { get; }

        public GrowthFertilizationConfigChangeEvent(bool fertilizeYield)
        {
            this.FertilizeYield = fertilizeYield;
        }

    }
}
