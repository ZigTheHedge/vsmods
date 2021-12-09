using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace edc.src.CommandHandlers
{
    public static class HungerSet
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            //new_hunger_value
            if (args.Length < 1) return;
            float newHunger;
            if (!float.TryParse(args[0], out newHunger)) return;
            if(newHunger < 0)
            {
                player.SendMessage(0, "Hunger level must be a positive value!", EnumChatType.CommandError);
                return;
            }
            player.Entity.WatchedAttributes.GetTreeAttribute("hunger").SetFloat("currentsaturation", newHunger);
            player.Entity.WatchedAttributes.MarkPathDirty("hunger");
            player.SendMessage(0, "Hunger level has been set to: " + newHunger.ToString(), EnumChatType.Notification);
        }
    }
}
