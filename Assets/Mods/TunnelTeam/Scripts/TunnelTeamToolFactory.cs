using TimberApi.Tools.ToolSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.TunnelTeam.Scripts
{
    public class TunnelTeamToolFactory : IToolFactory
    {
        private readonly ITunnelTeamTool _TunnelTeamTool;
        public string Id => "TunnelTeam";
        public TunnelTeamToolFactory(ITunnelTeamTool TunnelTeamTool)
        {
            _TunnelTeamTool = TunnelTeamTool;
        }

        public Tool Create(ToolSpec toolSpecification, ToolGroup toolGroup = null)
        {
            _TunnelTeamTool.SetToolGroup(toolGroup);
            return (Tool)_TunnelTeamTool;
        }

    }
}
