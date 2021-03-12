using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace necessaries.src.Trashcan
{
    public delegate ItemSlot NewSlotDelegate(int slotId, InventoryBase self);
    class TrashcanInventory : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;
        public ItemSlot[] Slots { get { return slots; } }

        NewSlotDelegate onNewSlot = null;
        public override int Count
        {
            get { return slots.Length; }
        }

        public TrashcanInventory(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            // 0 - in/out slot (top)
            // 1-3 - out only slots
            this.onNewSlot = (id, inv) => new TrashSlot(inv);
            slots = GenEmptySlots(4);
            baseWeight = 4;
        }

        public TrashcanInventory(string className, string instanceID, ICoreAPI api) : base(className, instanceID, api)
        {
            this.onNewSlot = (id, inv) => new TrashSlot(inv);
            slots = GenEmptySlots(4);
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

        protected override ItemSlot NewSlot(int slotId)
        {
            if (onNewSlot != null && slotId != 0) return onNewSlot(slotId, this);
            return new ItemSlotSurvival(this);
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
