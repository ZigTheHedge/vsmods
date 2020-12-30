using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace tastydays.src.cuttingboard
{
    class FoodSlot : ItemSlotSurvival
    {
        public FoodSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public static bool IsCuttable(ItemSlot itemSlot)
        {
            if (itemSlot != null)
            {
                bool isCuttable = itemSlot.Itemstack.Collectible.Attributes != null && itemSlot.Itemstack.Collectible.Attributes["cuttable"].AsBool();
                return isCuttable;
            }
            else
                return false;
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsCuttable(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsCuttable(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                return false;
            }
        }
    }
}
