﻿using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolFactory : IToolFactory
    {
        private readonly IForestTool _ForestTool;
        public string Id => "ForestTool";
        public ForestToolFactory(IForestTool ForestTool)
        {
            _ForestTool = ForestTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _ForestTool.SetToolGroup(toolGroup);
            return (Tool)_ForestTool;
        }

    }
}
