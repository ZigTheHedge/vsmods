using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace necessaries.src.Mailbox
{
    class MailBox : Block
    {

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEMailBox be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMailBox;
            }
            
            bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer);
                }

                return true;
            }

            return handled;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            if (world.Side == EnumAppSide.Server)
            {
                Necessaries.AddMailbox("", blockPos.X, blockPos.Y, blockPos.Z);
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos blockPos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.Side == EnumAppSide.Server)
            {
                Necessaries.RemoveMailbox(blockPos.X, blockPos.Y, blockPos.Z);
            }
            base.OnBlockBroken(world, blockPos, byPlayer, dropQuantityMultiplier);
        }
    }


}
