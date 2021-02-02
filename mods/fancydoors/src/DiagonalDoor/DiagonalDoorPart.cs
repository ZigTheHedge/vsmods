using fancydoors.src.RegularDoor;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace fancydoors.src.DiagonalDoor
{
    class DiagonalDoorPart : FancyDoorPart
    {
        public override Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string horientation)
        {
            BlockPos leftPos;
            BlockPos rightPos;
            BlockPos upPos;
            BlockPos downPos;
            Block blockLeft;
            Block blockRight;
            Block blockUp;
            Block blockDown;
            string orientation = "";

            if (horientation == "north")
            {
                leftPos = pos.WestCopy();
                rightPos = pos.EastCopy();
                upPos = pos.UpCopy().NorthCopy();
                downPos = pos.DownCopy().SouthCopy();
            }
            else if (horientation == "east")
            {
                leftPos = pos.NorthCopy();
                rightPos = pos.SouthCopy();
                upPos = pos.UpCopy().EastCopy();
                downPos = pos.DownCopy().WestCopy();
            }
            else if (horientation == "south")
            {
                leftPos = pos.EastCopy();
                rightPos = pos.WestCopy();
                upPos = pos.UpCopy().SouthCopy();
                downPos = pos.DownCopy().NorthCopy();
            }
            else
            {
                leftPos = pos.SouthCopy();
                rightPos = pos.NorthCopy();
                upPos = pos.UpCopy().WestCopy();
                downPos = pos.DownCopy().EastCopy();
            }

            AssetLocation blockCode;

            blockLeft = world.BlockAccessor.GetBlock(leftPos);
            blockRight = world.BlockAccessor.GetBlock(rightPos);
            blockUp = world.BlockAccessor.GetBlock(upPos);
            blockDown = world.BlockAccessor.GetBlock(downPos);

            if (blockUp.FirstCodePart() == "diagonaldoor") orientation = blockUp.Variant["orientation"];
            if (blockDown.FirstCodePart() == "diagonaldoor") orientation = blockDown.Variant["orientation"];

            if (orientation == "")
            {
                if (blockLeft.FirstCodePart() == "diagonaldoor")
                {
                    if (blockLeft.Variant["orientation"] == "left") orientation = "right";
                    else orientation = "left";
                }
            }
            if (orientation == "")
            {
                if (blockRight.FirstCodePart() == "diagonaldoor")
                {
                    if (blockRight.Variant["orientation"] == "left") orientation = "right";
                    else orientation = "left";
                }
            }
            if (orientation == "") orientation = "right";

            blockCode = CodeWithVariants(new Dictionary<string, string>() {
                                { "size", Variant["size"] },
                                { "orientation", orientation },
                                { "horizontalorientation", horientation }
                            });

            return world.BlockAccessor.GetBlock(blockCode);
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
            DiagonalDoorBE be = blockAccessor.GetBlockEntity(pos) as DiagonalDoorBE;
            if (be != null)
            {
                return be.GetCollisions();
            }
            else
                return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            DiagonalDoorBE be = blockAccessor.GetBlockEntity(pos) as DiagonalDoorBE;
            if (be != null)
            {
                return be.GetCollisions();
            }
            else
                return base.GetSelectionBoxes(blockAccessor, pos);
        }

    }
}
