using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace survivalcats.src
{
    class BlockChiselNameFix : BlockChisel
    {
        public override string GetHeldItemName(ItemStack itemStack)
        {
            if (itemStack.Attributes?.GetString("blockName") != null)
                return itemStack.Attributes?.GetString("blockName");
            else
                return base.GetHeldItemName(itemStack);
        }
    }
}
