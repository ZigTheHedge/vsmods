using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace edc.src.CommandHandlers
{
    public static class SetSpawn
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            SyncedEntityPos currentPosition = player.Entity.Pos;
            player.SetSpawnPosition(new PlayerSpawnPos((int)currentPosition.X, (int)currentPosition.Y, (int)currentPosition.Z));
            player.SendMessage(0, "New Spawnpoint set: " + currentPosition.ToString(), EnumChatType.CommandSuccess);
        }
    }
}
