using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace necessaries.src.SharpenerStuff
{
    class Sharpener : Item
    {
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            /*
            byEntity.Attributes.SetBool("isInsertGear", false);

            if (ToolSlot.IsTool(byEntity.LeftHandItemSlot))
            {
                byEntity.Attributes.SetBool("isInsertGear", true);
                byEntity.Attributes.SetBool("sharpened", false);

                handling = EnumHandHandling.PreventDefault;
                return;
            }
            */
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            int totalDurabilityLoss = 1;
            int totalDurabilityRestored = 2;

            if (inSlot.Itemstack.Item.Variant["rock"] == "granite")
            {
                totalDurabilityLoss = ModConfigFile.Current.graniteDiskDamagePerCycle;
                totalDurabilityRestored = ModConfigFile.Current.graniteDiskRepairPerCycle;
            }
            if (inSlot.Itemstack.Item.Variant["rock"] == "basalt")
            {
                totalDurabilityLoss = ModConfigFile.Current.basaltDiskDamagePerCycle;
                totalDurabilityRestored = ModConfigFile.Current.basaltDiskRepairPerCycle;
            }
            if (inSlot.Itemstack.Item.Variant["rock"] == "sandstone")
            {
                totalDurabilityLoss = ModConfigFile.Current.sandstoneDiskDamagePerCycle;
                totalDurabilityRestored = ModConfigFile.Current.sandstoneDiskRepairPerCycle;
            }
            if (inSlot.Itemstack.Item.Variant["rock"] == "obsidian")
            {
                totalDurabilityLoss = ModConfigFile.Current.obsidianDiskDamagePerCycle;
                totalDurabilityRestored = ModConfigFile.Current.obsidianDiskRepairPerCycle;
            }
            if (inSlot.Itemstack.Item.Variant["rock"] == "diamond")
            {
                totalDurabilityLoss = ModConfigFile.Current.diamondDiskDamagePerCycle;
                totalDurabilityRestored = ModConfigFile.Current.diamondDiskRepairPerCycle;
            }

            dsc.AppendLine("\n" + Lang.Get("necessaries:sharpener-repair-amount", totalDurabilityRestored));
            dsc.AppendLine(Lang.Get("necessaries:sharpener-damage-amount", totalDurabilityLoss));
        }
    }
}
