using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace necessaries.src.SharpenerStuff
{
    class ToolSlot : ItemSlotSurvival
    {
        public ToolSlot(InventoryBase inventory) : base(inventory)
        {
        }

        public static bool IsTool(ItemSlot itemSlot)
        {
            if (itemSlot != null && itemSlot.Itemstack != null)
            {
                if (itemSlot.Itemstack.Class == EnumItemClass.Block) return false;
                //return itemSlot.Itemstack.Item.Tool != null;
                if (itemSlot.Itemstack.Item.Attributes == null) return false;
                return itemSlot.Itemstack.Item.Attributes["sharpenable"].AsBool();
            }
            else
                return false;
        }

        public static bool IsSharpener(ItemSlot itemSlot)
        {
            if (itemSlot != null && itemSlot.Itemstack != null)
            {
                if (itemSlot.Itemstack.Class == EnumItemClass.Block) return false;
                return itemSlot.Itemstack.Item.Code.FirstPathPart().StartsWith("sharpener");
            }
            else
                return false;
        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsTool(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsTool(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                return false;
            }
        }
    }
}
