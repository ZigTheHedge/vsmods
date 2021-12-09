using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace edc.src.CommandHandlers
{
    public static class TemporalSet
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            //new_temporal_stability_value
            if (args.Length < 1) return;
            double newStability;
            if (!double.TryParse(args[0], out newStability)) return;
            if(newStability < 0 || newStability > 1)
            {
                player.SendMessage(0, "Temporal Stability level must be between 0 and 1!", EnumChatType.CommandError);
                return;
            }
            player.Entity.GetBehavior<EntityBehaviorTemporalStabilityAffected>().OwnStability = newStability;
            player.SendMessage(0, "Temporal Stability level has been set to: " + newStability.ToString(), EnumChatType.Notification);
        }
    }
}
