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
    public static class Tpx
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            // /tpx X Z
            SyncedEntityPos currentPosition = player.Entity.Pos;
            Random random = Edc.sApi.World.Rand;
            int min = 0;

            if (args.Length < 2) return;
            if (!int.TryParse(args[0], out int rangeX)) return;
            if (!int.TryParse(args[1], out int rangeZ)) return;
            if (args.Length == 3) min = (int.TryParse(args[2], out min)) ? min : 0;
            if (min > rangeX || min > rangeZ) min = Math.Min(rangeZ, rangeX);

            int chosenX = random.Next(Math.Abs(min), Math.Abs(rangeX));
            int chosenZ = random.Next(Math.Abs(min), Math.Abs(rangeZ));
            chosenX *= (random.NextDouble() > 0.5)? -1: 1;
            chosenZ *= (random.NextDouble() > 0.5)? -1: 1;

            int X = chosenX + (int)currentPosition.X;
            int Z = chosenZ + (int)currentPosition.Z;
            int chunkSize = Edc.sApi.World.BlockAccessor.ChunkSize;
            Edc.sApi.WorldManager.LoadChunkColumnPriority(X / chunkSize, Z / chunkSize, new ChunkLoadOptions()
            {
                OnLoaded = () =>
                {
                    int Y = player.Entity.World.BlockAccessor.GetTerrainMapheightAt(new Vintagestory.API.MathTools.BlockPos(X, (int)currentPosition.Y, Z)) + 1;
                    player.Entity.TeleportToDouble(X + 0.5D, Y, Z + 0.5D);
                }
            });

        }
    }
}
