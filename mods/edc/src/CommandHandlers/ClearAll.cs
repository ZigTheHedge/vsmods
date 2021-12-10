using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace edc.src.CommandHandlers
{
    public static class ClearAll
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            IInventory clothing = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
            IInventory hotbar = player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            IInventory main = player.InventoryManager.GetOwnInventory(GlobalConstants.backpackInvClassName);

            foreach (ItemSlot slot in clothing)
                if (!slot.Empty) slot.TakeOutWhole();
            foreach (ItemSlot slot in hotbar)
                if (!slot.Empty) slot.TakeOutWhole();
            foreach (ItemSlot slot in main)
                if (!slot.Empty) slot.TakeOutWhole();

        }
    }
}
