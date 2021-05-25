using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace necessaries.src.Mailbox
{
    class MailBoxInventory : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;
        public ItemSlot[] Slots { get { return slots; } }
        public override int Count
        {
            get { return slots.Length; }
        }

        public MailBoxInventory(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            // slots 0-3 = Incoming Mail
            // slot 4 = Envelope/Package
            // slot 5, 6 = Package inventory
            slots = GenEmptySlots(5);
            baseWeight = 4;
        }

        public MailBoxInventory(string className, string instanceID, ICoreAPI api) : base(className, instanceID, api)
        {
            slots = GenEmptySlots(5);
            baseWeight = 4;
        }

        protected override ItemSlot NewSlot(int i)
        {
            if (i == 4) return new ItemSlotSurvival (this);
            return new ItemSlotOutput(this);
        }

        public override WeightedSlot GetBestSuitedSlot(ItemSlot sourceSlot, List<ItemSlot> skipSlots = null)
        {
            return base.GetBestSuitedSlot(sourceSlot, skipSlots);
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
