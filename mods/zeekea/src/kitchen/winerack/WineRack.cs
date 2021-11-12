using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.kitchen.winerack
{
    class WineRack : Block
    {
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEWineRack bewinerack = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEWineRack;
            if (bewinerack != null) return bewinerack.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
