using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace xmastime.src
{
    class LightsOne : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStart(world, byPlayer, blockSel);

            string curType = world.BlockAccessor.GetBlock(blockSel.Position).Variant["rotator"];

            if (byPlayer.WorldData.EntityControls.Sneak)
            {
                if (curType == "s") curType = "left";
                else if (curType == "left") curType = "down";
                else if (curType == "down") curType = "right";
                else if (curType == "right") curType = "up";
                else if (curType == "up") curType = "f";
                else if (curType == "f") curType = "s";
            } else
            {
                if (curType == "s") curType = "f";
                else if (curType == "f") curType = "up";
                else if (curType == "up") curType = "right";
                else if (curType == "right") curType = "down";
                else if (curType == "down") curType = "left";
                else if (curType == "left") curType = "s";
            }
            StaticUtils.SetBlockState(world, blockSel.Position, "rotator", curType);
            
            return true;
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.AppendLine("\n" + Lang.Get("xmastime:rotator-help"));
        }
    }
}
