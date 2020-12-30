using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace fancydoors.src
{
    class ChiseledSlot : ItemSlotSurvival
    {
        public ChiseledSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public static bool IsChiseled(ItemSlot itemSlot)
        {
            if (itemSlot != null)
            {
                bool ret = (itemSlot.Itemstack != null && itemSlot.Itemstack.Class == EnumItemClass.Block && itemSlot.Itemstack.Block is BlockChisel);
                return ret;
            }
            else
                return false;
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsChiseled(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsChiseled(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                return false;
            }
        }
    }
}
