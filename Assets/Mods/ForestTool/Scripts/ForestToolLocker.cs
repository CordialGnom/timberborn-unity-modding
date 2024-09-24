using System;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using Timberborn.ToolSystem;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolLocker : IToolLocker
    {
        private static readonly string UnlockPromptLocKey = "Planting.UnlockPrompt";
        private static readonly string ToolNameLocKey = "Cordial.ForestTool.DisplayName";
        private static readonly string ToolBuildingNameLocKey = "Building.Forester.DisplayName";
        private readonly ILoc _loc;
        private readonly DialogBoxShower _dialogBoxShower;
        private readonly ForestToolUnlockedPlantableRegistry _forestToolUnlockedPlantableRegistry;

        public ForestToolLocker(
            ILoc loc,
            DialogBoxShower dialogBoxShower,
            ForestToolUnlockedPlantableRegistry forestToolUnlockedPlantableRegistry)
        {
            this._loc = loc;
            this._dialogBoxShower = dialogBoxShower;
            this._forestToolUnlockedPlantableRegistry = forestToolUnlockedPlantableRegistry;
        }

        public bool ShouldLock(Tool tool)
        {
            return this._forestToolUnlockedPlantableRegistry.IsLocked();
        }

        public void TryToUnlock(Tool tool, Action successCallback, Action failCallback)
        {
            if (this._forestToolUnlockedPlantableRegistry.IsLocked())
                this.ShowLockedMessage(failCallback);
            else
                successCallback();
        }

        private void ShowLockedMessage(Action failCallback)
        {
            string text = this._loc.T<string, string>(ForestToolLocker.UnlockPromptLocKey, this._loc.T(ToolBuildingNameLocKey), this._loc.T(ToolNameLocKey));
            this._dialogBoxShower.Create().SetMessage(text).SetConfirmButton(failCallback).Show();
        }

    }
}
