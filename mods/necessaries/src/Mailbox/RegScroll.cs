using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace necessaries.src.Mailbox
{
    class RegScroll : Item
    {
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            int centerX = world.BlockAccessor.MapSizeX / 2;
            int centerZ = world.BlockAccessor.MapSizeZ / 2;

            int X = inSlot.Itemstack.Attributes.GetInt("actX") - centerX;
            int Y = inSlot.Itemstack.Attributes.GetInt("actY");
            int Z = inSlot.Itemstack.Attributes.GetInt("actZ") - centerZ;

            dsc.AppendLine(Lang.Get("necessaries:scroll-coords", X, Y, Z));
        }
    }
}
