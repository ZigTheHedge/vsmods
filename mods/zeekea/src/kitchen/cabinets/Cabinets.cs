using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace zeekea.src.kitchen.cabinets
{
    class Cabinets : Block
    {

        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BECabinets becabinet = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BECabinets;
            if (becabinet != null) return becabinet.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}
