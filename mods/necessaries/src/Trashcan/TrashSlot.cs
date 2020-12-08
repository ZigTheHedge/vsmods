using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace necessaries.src.Trashcan
{
    class TrashSlot : ItemSlot
    {
        public TrashSlot(InventoryBase inventory) : base(inventory)
        {

        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            return false;
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            return false;
        }
    }
}
