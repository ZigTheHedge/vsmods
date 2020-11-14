using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace zeekea.src.freezer
{
    class FreezerInventory : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;

        public ItemSlot[] Slots { get { return slots; } }
        public override int Count
        {
            get { return slots.Length; }
        }

        public FreezerInventory(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            slots = GenEmptySlots(8);
            baseWeight = 4;
        }

        protected override ItemSlot NewSlot(int slotId)
        {
            if(slotId < 4)
                return new IceSlot(this);
            else
                return new ItemSlot(this);
        }

        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId < 0 || slotId >= Count) return null;
                return slots[slotId];
            }
            set
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));
                if (value == null) throw new ArgumentNullException(nameof(value));
                slots[slotId] = value;
            }
        }


        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            slots = SlotsFromTreeAttributes(tree);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            SlotsToTreeAttributes(slots, tree);
        }
    }
}
