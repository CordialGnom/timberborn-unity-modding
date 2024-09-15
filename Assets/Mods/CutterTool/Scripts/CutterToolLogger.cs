using Timberborn.ModManagerScene;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts {
  internal class CutterToolLogger : IModStarter {

    public void StartMod()
    {
         var playerLogPath = Application.persistentDataPath + "/Player.log";
        Debug.Log("Cutter Tool, output in Player.log file at: " + playerLogPath);
    }

  }
}