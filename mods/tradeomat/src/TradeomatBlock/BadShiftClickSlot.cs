using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace tradeomat.src.TradeomatBlock
{
    class BadShiftClickSlot : ItemSlotSurvival
    {
        public BadShiftClickSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority)
        {
            if (priority != EnumMergePriority.AutoMerge)
                return base.CanTakeFrom(sourceSlot, priority);
            else
            {
                return false;
            }
        }
    }
}
