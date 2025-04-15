﻿using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    public class PlantingOverrideTreeToolFactory : IToolFactory
    {
        private readonly IPlantingOverrideTreeTool _PlantingOverrideTool;
        public string Id => "PlantingOverrideToolTrees";
        public PlantingOverrideTreeToolFactory(IPlantingOverrideTreeTool PlantingOverrideTool)
        {
            _PlantingOverrideTool = PlantingOverrideTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _PlantingOverrideTool.SetToolGroup(toolGroup);
            return (Tool)_PlantingOverrideTool;
        }

    }
}
