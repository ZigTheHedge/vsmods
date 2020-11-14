using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace zeekea.src.armchair
{
    class Armchair : Block
    {
        public Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string orientation, bool isolated = false)
        {
            bool leftConnected = false;
            bool rightConnected = false;
            BlockPos leftPos;
            BlockPos rightPos;
            Block blockLeft;
            Block blockRight;

            if (orientation == "north")
            {
                leftPos = pos.WestCopy();
                rightPos = pos.EastCopy();
            }
            else if (orientation == "east")
            {
                leftPos = pos.NorthCopy();
                rightPos = pos.SouthCopy();
            }
            else if (orientation == "south")
            {
                leftPos = pos.EastCopy();
                rightPos = pos.WestCopy();
            }
            else
            {
                leftPos = pos.SouthCopy();
                rightPos = pos.NorthCopy();
            }

            AssetLocation blockCode;

            blockLeft = world.BlockAccessor.GetBlock(leftPos);
            blockRight = world.BlockAccessor.GetBlock(rightPos);

            if (blockLeft.FirstCodePart() == "armchair")
            {
                if (blockLeft.Variant["isolated"] == "no")
                {
                    if(blockLeft.Variant["horizontalorientation"] == orientation)
                        leftConnected = true;
                }
            }
            if (blockRight.FirstCodePart() == "armchair")
            {
                if (blockRight.Variant["isolated"] == "no")
                {
                    if (blockRight.Variant["horizontalorientation"] == orientation)
                        rightConnected = true;
                }
            }

            string thisArms;
            if (leftConnected && !rightConnected) thisArms = "right";
            else if (!leftConnected && rightConnected) thisArms = "left";
            else if (leftConnected && rightConnected) thisArms = "none";
            else thisArms = "both";
            
            if(isolated || Variant["isolated"] == "yes") thisArms = "both";
            blockCode = CodeWithVariants(new Dictionary<string, string>() {
                                { "color", Variant["color"] },
                                { "isolated", (isolated || Variant["isolated"] == "yes")?"yes":"no" },
                                { "arms", thisArms },
                                { "horizontalorientation", orientation }
                            });

            return world.BlockAccessor.GetBlock(blockCode);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);

            Block newBlock;

            if (byPlayer.WorldData.EntityControls.Sneak)
                newBlock = GetBlockVariant(world, blockSel.Position, horVer[0].Code, true);
            else
                newBlock = GetBlockVariant(world, blockSel.Position, horVer[0].Code);

            newBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

            return true;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            Block newBlock = GetBlockVariant(world, pos, Variant["horizontalorientation"]);

            world.BlockAccessor.ExchangeBlock(newBlock.Id, pos);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "arms", "both" },
                    { "color", Variant["color"] },
                    { "isolated", "no" },
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            dsc.AppendLine("\n" + Lang.Get("zeekea:armchair-help"));
        }

    }
}
