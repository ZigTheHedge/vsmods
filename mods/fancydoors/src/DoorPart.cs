using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace fancydoors.src
{
    abstract class DoorPart : Block
    {
        public abstract Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string horientation);

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEDoorPart be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEDoorPart;
            if(be != null)
            {
                return be.OnInteract(world, byPlayer, blockSel);
            } else
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);

            Block newBlock;

            newBlock = GetBlockVariant(world, blockSel.Position, horVer[0].Code);

            newBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

            return true;
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BEDoorPart be = blockAccessor.GetBlockEntity(pos) as BEDoorPart;
            if (be != null)
            {
                return new Cuboidf[] { be.GetCollision() };
            }
            else
                return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BEDoorPart be = blockAccessor.GetBlockEntity(pos) as BEDoorPart;
            if (be != null)
            {
                return new Cuboidf[] { be.GetCollision() };
            }
            else
                return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                        { "size", Variant["size"] },
                        { "orientation", "right" },
                        { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            BEDoorPart be = world.BlockAccessor.GetBlockEntity(selection.Position) as BEDoorPart;
            if (be == null) return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
            ItemSlot slot = forPlayer.InventoryManager.ActiveHotbarSlot;
            string[] help = new string[] { "fancydoors:emptyhand-sneak-help", "fancydoors:interact-help" };
            if (!slot.Empty)
            {
                help[0] = "fancydoors:removenotempty-sneak-help";
                if (!be.inventory[0].Empty)
                    help[0] = "fancydoors:justplace-sneak-help";
                if(ChiseledSlot.IsChiseled(slot))
                {
                    if (be.inventory[0].Empty)
                    {
                        help[1] = "fancydoors:placedoor-help";
                    } else
                    {
                        if (be.inventory[1].Empty)
                            help[1] = "fancydoors:placestatic-help";
                    }
                } else
                    help[1] = "fancydoors:nonchiseled-help";
            }
            else
            { 
                if (!be.inventory[1].Empty)
                    help[0] = "fancydoors:removestatic-sneak-help";
                else
                    if (!be.inventory[0].Empty)
                        help[0] = "fancydoors:remove-sneak-help";
            }
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = help[0],
                    HotKeyCode = "sneak",
                    MouseButton = EnumMouseButton.Right,
                },
                new WorldInteraction()
                {
                    ActionLangCode = help[1],
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

        public override int GetHeatRetention(BlockPos pos, BlockFacing facing)
        {
            return 3;
        }
    }
}
