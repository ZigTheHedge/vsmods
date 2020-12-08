using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cavepaintings.src
{
    class WallCanvas : Block
    {

        public static BlockFacing GetFacing(BlockPos me, BlockPos him)
        {
            if (him.Z == me.Z + 1) return BlockFacing.SOUTH;
            if (him.Z == me.Z - 1) return BlockFacing.NORTH;
            if (him.X == me.X + 1) return BlockFacing.EAST;
            if (him.X == me.X - 1) return BlockFacing.WEST;
            if (him.Y == me.Y + 1) return BlockFacing.UP;
            if (him.Y == me.Y - 1) return BlockFacing.DOWN;
            return null;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
            BlockFacing face = GetFacing(pos, neibpos).Opposite;
            BEWallCanvas be = api.World.BlockAccessor.GetBlockEntity(pos) as BEWallCanvas;
            if (be != null)
            {
                string block = world.BlockAccessor.GetBlock(neibpos).Code.FirstPathPart();
                if (block.StartsWith("air") || block.StartsWith("water"))
                {
                    be.RemoveSurface(face);
                    be.MarkDirty();
                    ShouldBlockStay(pos);
                }
            }
        }

        public void ShouldBlockStay(BlockPos pos)
        {
            if(api.Side == EnumAppSide.Server)
            {
                BEWallCanvas be = api.World.BlockAccessor.GetBlockEntity(pos) as BEWallCanvas;
                if(be != null)
                {
                    bool surfaceFound = false;
                    if (be.drawing.ContainsKey(BlockFacing.NORTH)) surfaceFound = true;
                    if (be.drawing.ContainsKey(BlockFacing.EAST)) surfaceFound = true;
                    if (be.drawing.ContainsKey(BlockFacing.SOUTH)) surfaceFound = true;
                    if (be.drawing.ContainsKey(BlockFacing.WEST)) surfaceFound = true;
                    if (be.drawing.ContainsKey(BlockFacing.UP)) surfaceFound = true;
                    if (be.drawing.ContainsKey(BlockFacing.DOWN)) surfaceFound = true;
                
                    if(!surfaceFound)
                    {
                        api.World.BlockAccessor.SetBlock(0, pos);
                    }
                }
            }
        }
    }
}
