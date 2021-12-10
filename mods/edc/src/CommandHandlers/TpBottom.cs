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
    public static class TpBottom
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            SyncedEntityPos currentPosition = player.Entity.Pos;
            int Y = 1;
            int Ymax = (int)currentPosition.Y;
            while(Y < Ymax)
            {
                Block biq = player.Entity.World.BlockAccessor.GetBlock((int)currentPosition.X, Y, (int)currentPosition.Z);
                if(biq.Id == 0)
                {
                    if(player.Entity.World.BlockAccessor.GetBlock((int)currentPosition.X, Y + 1, (int)currentPosition.Z).Id == 0)
                    {
                        player.Entity.TeleportToDouble(currentPosition.X, Y, currentPosition.Z);
                        return;
                    }
                }
                Y++;
            }
            IBlockAccessor world = player.Entity.World.BlockAccessor;
            for (int x = -1; x < 2; x++)
                for (int z = -1; z < 2; z++)
                    for (int y = 0; y < 3; y++)
                        world.SetBlock(0, new Vintagestory.API.MathTools.BlockPos((int)currentPosition.X + x, 1 + y, (int)currentPosition.Z + z));

            player.Entity.TeleportToDouble(currentPosition.X, 1, currentPosition.Z);
        }
    }
}
