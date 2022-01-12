using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!Necessaries.AbleToPlaceMailbox(byPlayer, api))
            {
                if (api.Side == EnumAppSide.Server)
                    ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("necessaries:nomore"), EnumChatType.CommandError);
                return false;
            }
            else
            {
                if (ModConfigFile.Current.mailboxesAllowed != 0)
                {
                    if (api.Side == EnumAppSide.Server)
                        ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("necessaries:count", (Necessaries.CountMailboxes(byPlayer, api) + 1), ModConfigFile.Current.mailboxesAllowed), EnumChatType.CommandSuccess);
                }
            }
            bool ret = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);

            return ret;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            if (api.Side == EnumAppSide.Server)
                Necessaries.AddMailbox((IServerPlayer)byPlayer, "", blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z);

            bool res = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            BlockPos blockPos = blockSel.Position;
            BEMailBox beMailBox = world.BlockAccessor.GetBlockEntity(blockPos) as BEMailBox;
            if (beMailBox != null && ModConfigFile.Current.mailHardmodeEnabled)
            {
                ItemStack itemStack = new ItemStack(world.GetItem(new AssetLocation("necessaries:regscroll")));
                itemStack.Attributes.SetInt("actX", blockPos.X);
                itemStack.Attributes.SetInt("actY", blockPos.Y);
                itemStack.Attributes.SetInt("actZ", blockPos.Z);
                beMailBox.inventory[0].Itemstack = itemStack;
                beMailBox.inventory[0].MarkDirty();
            }
            return res;
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            if (world.Side == EnumAppSide.Server)
            {
                Necessaries.RemoveMailbox(pos.X, pos.Y, pos.Z);
            }
            base.OnBlockRemoved(world, pos);
        }
    }
}
