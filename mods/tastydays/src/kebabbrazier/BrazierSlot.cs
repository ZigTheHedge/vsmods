using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace tastydays.src.kebabbrazier
{
    class BrazierSlot : ItemSlotSurvival
    {
        public BrazierSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public static bool IsSkewer(ItemSlot itemSlot)
        {
            if (itemSlot != null)
            {
                string itemCode = itemSlot.Itemstack.Collectible.Code.FirstPathPart();
                return itemCode.StartsWith("skewer");
            }
            else
                return false;
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsSkewer(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsSkewer(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                return false;
            }
        }
    }
}
