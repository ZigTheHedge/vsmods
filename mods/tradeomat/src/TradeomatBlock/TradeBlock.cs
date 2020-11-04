using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
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

            bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (!handled && byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer, true);
                }

                return true;
            }
            
            if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
            {
                if (be != null)
                {
                    be.OnBlockInteract(byPlayer, false);
                }

                return true;
            }

            return handled;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            bool ret = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            BlockPos upperPart = blockSel.Position.UpCopy();
            if (world.BlockAccessor.GetBlockId(upperPart) != 0) return false;

            Block upperBlock = world.GetBlock(new AssetLocation("tradeomat:tomat-up"));
            world.BlockAccessor.SetBlock(upperBlock.BlockId, upperPart);
            
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
            world.BlockAccessor.SetBlock(0, pos.UpCopy());
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

    }
}
