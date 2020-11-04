using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock
{
    class TradeBlockUp : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockSelection downSelection = new BlockSelection
            {
                Position = blockSel.Position.DownCopy()
            };
            return world.BlockAccessor.GetBlock(blockSel.Position.DownCopy()).OnBlockInteractStart(world, byPlayer, downSelection);
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            world.BlockAccessor.BreakBlock(pos.DownCopy(), byPlayer);
            world.BlockAccessor.SetBlock(0, pos);
        }

    }
}
