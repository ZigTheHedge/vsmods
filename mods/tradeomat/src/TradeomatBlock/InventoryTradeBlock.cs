using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace tradeomat.src.TradeomatBlock
{
    class InventoryTradeBlock : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;
        public ItemSlot[] Slots { get { return slots; } }
        public override int Count
        {
            get { return slots.Length; }
        }

        public InventoryTradeBlock(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            // slot 0 - price
            // slot 1 - goods
            // slot 2-9 - trades storage
            // slot 10-17 - goods storage
            // slot 18 - payment In
            // slot 19 - goods out
            slots = GenEmptySlots(20);
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
