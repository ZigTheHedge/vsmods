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
using Vintagestory.API.Util;

namespace tradeomat.src.TradeomatBlock
{
    class TradeBlock : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {

            BETradeBlock be = null;
            if (blockSel.Position != null)
            {
                be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETradeBlock;
            }

            if (byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer, true);
                }

                return true;
            }
            
            if (!byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer, false);

                    if (world.Side == EnumAppSide.Client)
                    {
                        Tradeomat.clientChannel.SendPacket<OpenBuyerInterface>(new OpenBuyerInterface(blockSel.Position));
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockPos upperPart = blockSel.Position.UpCopy();
            bool shouldAffectUpperPart = false;
            if (itemstack.Collectible.Variant["type"] == "tall") shouldAffectUpperPart = true;
            if (shouldAffectUpperPart && world.BlockAccessor.GetBlockId(upperPart) != 0) return false;
            
            if (!Tradeomat.AbleToPlaceTomat(byPlayer, api))
            {
                if (api.Side == EnumAppSide.Server)
                    ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("tradeomat:nomore"), EnumChatType.CommandError);
                return false;
            } else
            {
                if(SDCFileConfig.Current.NumberOfTomatsAllowed != 0)
                {
                    if (api.Side == EnumAppSide.Server)
                        ((IServerPlayer)byPlayer).SendMessage(0, Lang.Get("tradeomat:count", (Tradeomat.CountTomatoes(byPlayer, api) + 1), SDCFileConfig.Current.NumberOfTomatsAllowed), EnumChatType.CommandSuccess);
                }
            }
            bool ret = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);

            Block upperBlock = world.GetBlock(new AssetLocation("tradeomat:tomat-up"));
            if(shouldAffectUpperPart) world.BlockAccessor.SetBlock(upperBlock.BlockId, upperPart);
            if (api.Side == EnumAppSide.Server)
                Tradeomat.AddTomat((IServerPlayer)byPlayer, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z);
            return ret;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool result = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);
            if (blockSel.Position != null)
            {
                BETradeBlock be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETradeBlock;
                if(be != null)
                    be.ownerName = byPlayer.PlayerName;
            }
            
            return result;
        }
        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos)
        {
            bool shouldAffectUpperPart = false;
            if (Variant["type"] == "tall") shouldAffectUpperPart = true;
            if(shouldAffectUpperPart)world.BlockAccessor.SetBlock(0, pos.UpCopy());
            if (api.Side == EnumAppSide.Server)
            {
                Tradeomat.RemoveTomat(pos.X, pos.Y, pos.Z);
            }
            base.OnBlockRemoved(world, pos);
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "tradeomat:blockhelp-tomat-owner",
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right,
                },
                new WorldInteraction()
                {
                    ActionLangCode = "tradeomat:blockhelp-tomat-customer",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            BETradeBlock be = null;
            string tooltip = "";
            be = world.BlockAccessor.GetBlockEntity(pos) as BETradeBlock;
            if (be != null)
                tooltip = Lang.Get("tradeomat:owner-tooltip", be.ownerName);
            
            return base.GetPlacedBlockInfo(world, pos, forPlayer) + tooltip; 
        }

    }
}
