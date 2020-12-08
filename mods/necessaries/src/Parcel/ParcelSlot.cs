using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace necessaries.src.Parcel
{
    class ParcelSlot : ItemSlotSurvival
    {
        public ParcelSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (itemstackFromSourceSlot.Itemstack?.Collectible.FirstCodePart().Equals("parcel") == true)
                return false;
            else
                return base.CanHold(itemstackFromSourceSlot);

        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (sourceSlot.Itemstack?.Collectible.FirstCodePart().Equals("parcel") == true)
                return false;
            else
                return base.CanTakeFrom(sourceSlot);
        }
    }
}
