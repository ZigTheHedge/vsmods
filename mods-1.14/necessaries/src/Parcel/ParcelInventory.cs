using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace necessaries.src.Parcel
{
    public delegate ItemSlot NewSlotDelegate(int slotId, InventoryBase self);
    class ParcelInventory : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;
        public ItemSlot[] Slots { get { return slots; } }
        
        NewSlotDelegate onNewSlot = null;
        public override int Count
        {
            get { return slots.Length; }
        }

        public ParcelInventory(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            this.onNewSlot = (id, inv) => new ParcelSlot(inv);
            slots = GenEmptySlots(2);
        }

        public ParcelInventory(string className, string instanceID, ICoreAPI api) : base(className, instanceID, api)
        {
            this.onNewSlot = (id, inv) => new ParcelSlot(inv);
            slots = GenEmptySlots(2);
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
            if (onNewSlot != null) return onNewSlot(slotId, this);
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
