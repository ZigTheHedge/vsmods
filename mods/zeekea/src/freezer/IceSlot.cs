using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace zeekea.src.freezer
{
    class IceSlot : ItemSlot
    {
        public IceSlot(InventoryBase inventory) : base(inventory)
        {
        }

        private bool IsIce(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack != null)
                return itemSlot.Itemstack.Collectible.FirstCodePart().EndsWith("ice");
            else
                return false;
        }
        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsIce(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                itemstackFromSourceSlot.MarkDirty();
                this.MarkDirty();
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot)
        {
            if (IsIce(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                sourceSlot.MarkDirty();
                this.MarkDirty();
                return false;
            }
        }

    }
}
