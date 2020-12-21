using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace tastydays.src.kebabbrazier
{
    class CoalSlot : ItemSlotSurvival
    {
        public CoalSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public static bool IsCoal(ItemSlot itemSlot)
        {
            if (itemSlot != null)
            {
                string itemCode = itemSlot.Itemstack.Collectible.Code.FirstPathPart();
                return itemCode.StartsWith("charcoal") 
                    || itemCode.StartsWith("ore-lignite") 
                    || itemCode.StartsWith("ore-bituminouscoal") 
                    || itemCode.StartsWith("ore-anthracite") 
                    || itemCode.StartsWith("coke");
            }
            else
                return false;
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsCoal(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsCoal(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                return false;
            }
        }
    }
}
