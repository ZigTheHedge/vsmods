using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace edc.src.CommandHandlers
{
    public static class Disarm
    {
        public static void Execute(IServerPlayer player, int groupId, CmdArgs args)
        {
            IInventory clothing = player.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
            IInventory hotbar = player.InventoryManager.GetOwnInventory(GlobalConstants.hotBarInvClassName);
            Vec3d blockPos = new Vec3d(player.Entity.Pos.X, player.Entity.Pos.Y, player.Entity.Pos.Z);
            
            /*
            int idx = 0;
            foreach(ItemSlot slot in hotbar)
            {
                player.SendMessage(0, "IDX: " + idx + ", item: " + slot.ToString(), EnumChatType.CommandSuccess);
                idx++;
            }
            */

           //player.Entity.AddBehavior



            for(int i = 12; i < 15; i++)
                if (!clothing.ElementAt(i).Empty) player.InventoryManager.DropItem(clothing.ElementAt(i), true);
            if (!player.InventoryManager.ActiveHotbarSlot.Empty) player.InventoryManager.DropItem(player.InventoryManager.ActiveHotbarSlot, true);
            if (!hotbar.ElementAt(10).Empty) player.InventoryManager.DropItem(hotbar.ElementAt(10), true);

        }
    }
}
