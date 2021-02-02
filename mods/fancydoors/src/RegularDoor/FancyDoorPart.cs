using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace fancydoors.src.RegularDoor
{
    class FancyDoorPart : DoorPart
    {
        public override Block GetBlockVariant(IWorldAccessor world, BlockPos pos, string horientation)
        {
            BlockPos leftPos;
            BlockPos rightPos;
            Block blockLeft;
            Block blockRight;
            Block blockUp;
            Block blockDown;
            string orientation = "";

            if (horientation == "north")
            {
                leftPos = pos.WestCopy();
                rightPos = pos.EastCopy();
            }
            else if (horientation == "east")
            {
                leftPos = pos.NorthCopy();
                rightPos = pos.SouthCopy();
            }
            else if (horientation == "south")
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
            blockUp = world.BlockAccessor.GetBlock(pos.UpCopy());
            blockDown = world.BlockAccessor.GetBlock(pos.DownCopy());

            if (blockUp.FirstCodePart() == "fancydoor") orientation = blockUp.Variant["orientation"];
            if (blockDown.FirstCodePart() == "fancydoor") orientation = blockDown.Variant["orientation"];

            if (orientation == "")
            {
                if (blockLeft.FirstCodePart() == "fancydoor")
                {
                    if (blockLeft.Variant["orientation"] == "left") orientation = "right";
                    else orientation = "left";
                }
            }
            if (orientation == "")
            {
                if (blockRight.FirstCodePart() == "fancydoor")
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
    }
}
