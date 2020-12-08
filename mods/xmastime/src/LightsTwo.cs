using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace xmastime.src
{
    class LightsTwo : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStart(world, byPlayer, blockSel);

            string curType = world.BlockAccessor.GetBlock(blockSel.Position).Variant["type"];

            if (curType == "s") curType = "f";
            else curType = "s";

            StaticUtils.SetBlockState(world, blockSel.Position, "type", curType);

            return true;
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.AppendLine("\n" + Lang.Get("xmastime:rotator-help"));
        }

    }
}
