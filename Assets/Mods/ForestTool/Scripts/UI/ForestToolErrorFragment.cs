using Timberborn.CoreUI;
using Timberborn.Localization;
using Timberborn.PlantingUI;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolErrorPrompt
    {

        private static readonly string UnlockPromptLocKey = "Cordial.ForestTool.ForestToolError.Description";
        private static readonly string ToolBuildingLocKey = "Building.Forester.DisplayName";


        // localizations
        private readonly ILoc _loc;
        private readonly DialogBoxShower _dialogBoxShower;

        public ForestToolErrorPrompt(   DialogBoxShower dialogBoxShower,
                                        ILoc loc)
        {
            _dialogBoxShower = dialogBoxShower;
            _loc = loc;
        }

        public void ShowLockedMessage()
        {
            string text = this._loc.T<string>(UnlockPromptLocKey, _loc.T(ToolBuildingLocKey));
            this._dialogBoxShower.Create().SetMessage(text).SetConfirmButton( EmptyCallback ).Show();
        }

        private static void EmptyCallback()
        {
            // do nothing
        }

    }
}
