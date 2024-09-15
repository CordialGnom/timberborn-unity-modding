using Timberborn.Modding;
using Timberborn.SettingsSystem;
using ModSettings.Core;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolSettings : ModSettingsOwner
    {
        public ModSetting<int> IntSetting { get; } =
          new(2, ModSettingDescriptor.CreateLocalized("Cordial.CutterTool.IntSetting"));

        public ModSetting<float> FloatSetting { get; } =
          new(1.1f, ModSettingDescriptor.CreateLocalized("Cordial.CutterTool.FloatSetting"));

        public ModSetting<string> StringSetting { get; } =
          new("default",
              ModSettingDescriptor.CreateLocalized("Cordial.CutterTool.StringSetting"));

        public ModSetting<bool> BoolSetting { get; } =
          new(false, ModSettingDescriptor.CreateLocalized("Cordial.CutterTool.BoolSetting"));

        public CutterToolSettings(ISettings settings,
                                     ModSettingsOwnerRegistry modSettingsOwnerRegistry,
                                     ModRepository modRepository) : base(
            settings, modSettingsOwnerRegistry, modRepository)
        {

        }
        public override string HeaderLocKey => "Cordial.CutterTool.SimpleSettingsHeader";

        protected override string ModId => "Cordial.CutterTool";
    }
}
