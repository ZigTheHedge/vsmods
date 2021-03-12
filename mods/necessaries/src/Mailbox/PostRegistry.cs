using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace necessaries.src.Mailbox
{
    class PostRegistry : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (!slot.Empty && slot.Itemstack.Collectible.Code.FirstPathPart().StartsWith("regscroll"))
            {
                if (world.Side == EnumAppSide.Server)
                {
                    ITreeAttribute tree = slot.Itemstack.Attributes;
                    Necessaries.ValidateMailbox(tree.GetInt("actX"), tree.GetInt("actY"), tree.GetInt("actZ"));
                }
                slot.Itemstack = null;
            }
            return true;
        }

    }
}
