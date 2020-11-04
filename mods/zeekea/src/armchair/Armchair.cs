using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace zeekea.src.armchair
{
    class Armchair : Block
    {
        public Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string orientation)
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
                if (blockLeft.Variant["color"] == Variant["color"])
                {
                    if(blockLeft.Variant["horizontalorientation"] == orientation)
                        leftConnected = true;
                }
            }
            if (blockRight.FirstCodePart() == "armchair")
            {
                if (blockRight.Variant["color"] == Variant["color"])
                {
                    if (blockLeft.Variant["horizontalorientation"] == orientation)
                        rightConnected = true;
                }
            }

            string thisArms;
            if (leftConnected && !rightConnected) thisArms = "right";
            else if (!leftConnected && rightConnected) thisArms = "left";
            else if (leftConnected && rightConnected) thisArms = "none";
            else thisArms = "both";
            blockCode = CodeWithVariants(new Dictionary<string, string>() {
                                { "color", Variant["color"] },
                                { "arms", thisArms },
                                { "horizontalorientation", orientation }
                            });

            return world.BlockAccessor.GetBlock(blockCode);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);
            
            Block newBlock = GetBlockVariant(world, blockSel.Position, horVer[0].Code);

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
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }
    }
}
