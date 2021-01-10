using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace tradeomat.src.Coins
{
    class CoinsPile : Block, IBlockItemPile
    {
        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return new BlockDropItemStack[0];
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            // Handled by BlockEntityItemPile
            return new ItemStack[0];
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (be is BECoinsPile pile)
            {
                return pile.OnPlayerInteract(byPlayer);
            }

            return false;
        }

        public bool Construct(ItemSlot slot, IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
        {
            Block block = world.BlockAccessor.GetBlock(pos);
            if (!block.IsReplacableBy(this)) return false;
            Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
            if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP) && belowBlock != this) return false;

            world.BlockAccessor.SetBlock(BlockId, pos);

            BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
            if (be is BECoinsPile pile)
            {
                if (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                {
                    pile.inventory[0].Itemstack = (ItemStack)slot.TakeOut(byPlayer.Entity.Controls.Sprint ? pile.BulkTakeQuantity : pile.DefaultTakeQuantity);
                    slot.MarkDirty();
                }
                else
                {
                    pile.inventory[0].Itemstack = (ItemStack)slot.Itemstack.Clone();
                    pile.inventory[0].Itemstack.StackSize = Math.Min(pile.inventory[0].Itemstack.StackSize, pile.MaxStackSize);
                }

                pile.MarkDirty();
                world.BlockAccessor.MarkBlockDirty(pos);
                world.PlaySoundAt(pile.soundLocation, pos.X, pos.Y, pos.Z, byPlayer, true);
            }

            return true;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
            if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP))
            {
                world.BlockAccessor.BreakBlock(pos, null);
            }
        }


        public override bool CanAttachBlockAt(IBlockAccessor blockAccessor, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
        {
            /*
            BECoinsPile be = blockAccessor.GetBlockEntity(pos) as BECoinsPile;
            if (be != null)
            {
                return be.OwnStackSize == be.MaxStackSize;
            } 
            */
            return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
        }

    }
}
