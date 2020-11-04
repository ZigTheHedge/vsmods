using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.nightlamp
{
    class Nightlamp : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            AssetLocation newCode;

            if (Variant["state"] == "on") newCode = CodeWithVariant("state", "off");
            else newCode = CodeWithVariant("state", "on");
            
            Block newBlock = world.BlockAccessor.GetBlock(newCode);

            world.BlockAccessor.ExchangeBlock(newBlock.BlockId, blockSel.Position);

            return true;
        }
        
        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "metal", Variant["metal"] },
                    { "color", Variant["color"] },
                    { "state", "off" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }
    }
}
