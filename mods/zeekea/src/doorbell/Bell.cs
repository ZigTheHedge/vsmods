using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.doorbell
{
    class Bell : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEBell be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEBell;
            if (be != null)
                return be.Ring();
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public bool AbleToAttach(IWorldAccessor world, string facingCode, BlockPos blockPos)
        {

            if (facingCode == "north") return world.BlockAccessor.GetBlock(blockPos.NorthCopy()).SideSolid[BlockFacing.SOUTH.Index];
            if (facingCode == "east") return world.BlockAccessor.GetBlock(blockPos.EastCopy()).SideSolid[BlockFacing.WEST.Index];
            if (facingCode == "south") return world.BlockAccessor.GetBlock(blockPos.SouthCopy()).SideSolid[BlockFacing.NORTH.Index];
            if (facingCode == "west") return world.BlockAccessor.GetBlock(blockPos.WestCopy()).SideSolid[BlockFacing.EAST.Index];
            
            return false;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);

            bool ret = AbleToAttach(world, horVer[0].Code, blockSel.Position);
            if (!ret)
            {
                failureCode = "requirehorizontalattachable";
                return false;
            }
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if(!AbleToAttach(world, Variant["horizontalorientation"], pos))
                world.BlockAccessor.BreakBlock(pos, null);
        }
    }
}
